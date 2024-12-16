using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PoS_Placeholder.Server.Models;
using PoS_Placeholder.Server.Models.Builders;
using PoS_Placeholder.Server.Models.Dto;
using PoS_Placeholder.Server.Models.Enum;
using PoS_Placeholder.Server.Repositories;

namespace PoS_Placeholder.Server.Controllers;

[ApiController]
[Route("/api/discounts")]

public class DiscountController : ControllerBase
{
    private DiscountRepository _discountRepository;
    private ProductVariationRepository _variationRepository;
    private UserManager<User> _userManager;
    private ILogger<DiscountController> _logger;
    
    public DiscountController(DiscountRepository repository, UserManager<User> userManager, ProductVariationRepository variationRepository, ILogger<DiscountController> logger)
    {
        _discountRepository = repository;
        _variationRepository = variationRepository;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllDiscounts()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        var userBusinessId = user.BusinessId;
        var businessDiscounts = await _discountRepository.GetWhereAsync(discount => discount.BusinessId == userBusinessId);

        businessDiscounts = businessDiscounts.Where(d => d.EndDate >= DateTime.Now);
        
        return Ok(businessDiscounts);
    }
    
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetDiscount(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        var variationDiscount = await _discountRepository.GetByIdAsync(id);

        if (variationDiscount?.EndDate <= DateTime.Now || variationDiscount == null)
            return NotFound("Discount not valid");
        
        return Ok(variationDiscount);
    }
    
    [HttpGet("productVariations/{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetAllProductVariationsByDiscountId(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        var userBusinessId = user.BusinessId;
        var discount = await _discountRepository.GetDiscountByDiscountAndBusinessId(id, userBusinessId);
        if (discount == null)
        {
            return BadRequest("Discount not found.");
        }

        var discountVariations = await _variationRepository.GetWhereAsync(variation => variation.DiscountId == discount.Id);
        
        return Ok(discountVariations);
    }
    
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountDto createDiscountDto)
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

            var businessId = user.BusinessId;

            var newDiscount = new DiscountBuilder().FromCreateDto(createDiscountDto, businessId).Build();

            _discountRepository.Add(newDiscount);
            await _discountRepository.SaveChangesAsync();

            return Ok(newDiscount);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
    
    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> AddProductVariationsToDiscount(int id, [FromBody] IEnumerable<UpdateProductVariationDiscountDto> dto)
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
            
            var discount = await _discountRepository.GetByIdAsync(id);
            if (discount == null)
            {
                return NotFound("Discount not found.");
            }
            
            if (userBusinessId != discount.BusinessId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to update this discount.");
            }
            
            List<int> variationIdsForAdding = dto
                .Where(d => d.IsAdd)
                .Select(d => d.ProductVariationId)
                .ToList();
            
            List<int> variationIdsForRemoving = dto
                .Where(d => !d.IsAdd)
                .Select(d => d.ProductVariationId)
                .ToList();
            
            
            var variationsForAdding =
                await _variationRepository.GetByVariationIdsAndBusinessIdAsync(variationIdsForAdding, userBusinessId);
            
            var variationsForRemoving =
                await _variationRepository.GetByVariationIdsAndBusinessIdAsync(variationIdsForRemoving, userBusinessId);
            
            foreach (var variation in variationsForAdding)
            {
                variation.DiscountId = discount.Id;
            }
            
            foreach (var variation in variationsForRemoving)
            {
                variation.DiscountId = null;
            }
            
            
            _variationRepository.BulkUpdate(variationsForAdding);
            _variationRepository.BulkUpdate(variationsForRemoving);
            await _variationRepository.SaveChangesAsync();
            
            return Ok(variationsForAdding);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
    
    [HttpPut]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> UpdateDiscount([FromBody] UpdateDiscountDto updateDiscountDto)
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

            var discount = await _discountRepository.GetByIdAsync(updateDiscountDto.Id);
            if (discount == null)
            {
                return NotFound("Discount not found.");
            }

            if (userBusinessId != discount.BusinessId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to update this discount.");
            }

            var updatedDiscount = new DiscountBuilder().FromUpdateDto(updateDiscountDto, discount).Build();

            _discountRepository.Update(updatedDiscount);
            await _discountRepository.SaveChangesAsync();

            return Ok(updatedDiscount);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> DeleteDiscount(int id)
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

            var discount = await _discountRepository.GetByIdAsync(id);
            if (discount == null)
            {
                return NotFound("Discount not found.");
            }

            if (userBusinessId != discount.BusinessId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to delete this discount.");
            }

            _discountRepository.Remove(discount);
            await _discountRepository.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}