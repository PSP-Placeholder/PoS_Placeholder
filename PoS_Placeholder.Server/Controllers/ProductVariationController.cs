using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PoS_Placeholder.Server.Models;
using PoS_Placeholder.Server.Models.Dto;
using PoS_Placeholder.Server.Models.Enum;
using PoS_Placeholder.Server.Repositories;
using PoS_Placeholder.Server.Services;

namespace PoS_Placeholder.Server.Controllers;

[ApiController]
[Route("/api/productVariations")]
public class ProductVariationController : ControllerBase
{
    private readonly ProductVariationRepository _variationRepository;
    private readonly ProductRepository _productRepository;
    private readonly UserManager<User> _userManager;
    private readonly IImageService _imageService;

    public ProductVariationController(ProductVariationRepository productVariationRepository,
        ProductRepository productRepository, UserManager<User> userManager, IImageService imageService)
    {
        _variationRepository = productVariationRepository;
        _productRepository = productRepository;
        _userManager = userManager;
        _imageService = imageService;
    }

    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllProductVariations()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        var userBusinessId = user.BusinessId;

        var productVariations = await _variationRepository.GetWhereAsync(variation => variation.Product.BusinessId == userBusinessId);
         
        return Ok(productVariations);
    }
    
    [HttpGet("{id:int}", Name = "GetAllProductVariationsById")]
    [Authorize]
    public async Task<IActionResult> GetAllProductVariationsById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid product ID.");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        var userBusinessId = user.BusinessId;

        var productVariations = await _variationRepository.GetByProductAndBusinessId(id, userBusinessId);

        if (productVariations == null)
        {
            return NotFound("Product variations not found.");
        }

        return Ok(productVariations);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> CreateProductVariation([FromForm] CreateProductVariationDto createProductVariationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (createProductVariationDto.PictureFile == null || createProductVariationDto.PictureFile.Length == 0)
            {
                return BadRequest("Picture file is required.");
            }

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(createProductVariationDto.PictureFile.FileName)}";
            string pictureUrl =
                await _imageService.UploadFileBlobAsync(fileName, createProductVariationDto.PictureFile);

            var newProductVariation = new ProductVariation()
            {
                Name = createProductVariationDto.Name,
                PictureUrl = pictureUrl,
                Price = createProductVariationDto.Price,
                ProductId = createProductVariationDto.ProductId
            };

            _variationRepository.Add(newProductVariation);
            await _variationRepository.SaveChangesAsync();

            return CreatedAtRoute("GetAllProductVariationsById", new { id = newProductVariation.Id },
                newProductVariation);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error: {e.Message}, StackTrace: {e.StackTrace}");
        }
    }

    [HttpPut]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> UpdateProductVariation([FromForm] UpdateProductVariationDto updateProductVariationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var userBusinessId = user.BusinessId;

            var productVariation = await _variationRepository.GetByIdAsync(updateProductVariationDto.Id);
            if (productVariation == null)
            {
                return NotFound("ProductVariation not found.");
            }

            var product = await _productRepository.GetByIdAsync(updateProductVariationDto.ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (userBusinessId != product.BusinessId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to update this product.");
            }

            // If user passed the new image file, we must delete old one and upload new image to image storage container
            if (updateProductVariationDto.PictureFile != null || updateProductVariationDto.PictureFile?.Length > 0)
            {
                string oldFileName = productVariation.PictureUrl.Split('/').Last();
                await _imageService.DeleteFileBlobAsync(oldFileName);

                string newFileName = $"{Guid.NewGuid()}{Path.GetExtension(updateProductVariationDto.PictureFile.FileName)}";
                string pictureUrl = await _imageService.UploadFileBlobAsync(newFileName, updateProductVariationDto.PictureFile);

                productVariation.PictureUrl = pictureUrl;
            }

            if (updateProductVariationDto.Name != null)
                productVariation.Name = updateProductVariationDto.Name;

            if (updateProductVariationDto.Price != null)
                productVariation.Price = updateProductVariationDto.Price;

            _variationRepository.Update(productVariation);
            await _variationRepository.SaveChangesAsync();

            return Ok(productVariation);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}, StackTrace: {ex.StackTrace}");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> DeleteProductVariation(int id)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var userBusinessId = user.BusinessId;

            var productVariation = await _variationRepository.GetByIdAsync(id);
            if (productVariation == null)
            {
                return NotFound("ProductVariation not found.");
            }

            var product = await _productRepository.GetByIdAsync(productVariation.ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (userBusinessId != product.BusinessId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to delete this product.");
            }

            _variationRepository.Remove(productVariation);
            await _variationRepository.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}