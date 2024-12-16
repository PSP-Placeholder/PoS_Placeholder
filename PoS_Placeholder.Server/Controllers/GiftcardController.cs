using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PoS_Placeholder.Server.Models;
using PoS_Placeholder.Server.Models.Dto;
using PoS_Placeholder.Server.Models.Enum;
using PoS_Placeholder.Server.Repositories;

namespace PoS_Placeholder.Server.Controllers;

[ApiController]
[Route("/api/giftcards")]
public class GiftcardController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly GiftcardRepository _giftcardRepository;

    public GiftcardController(UserManager<User> userManager, GiftcardRepository giftcardRepository)
    {
        _userManager = userManager;
        _giftcardRepository = giftcardRepository;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllGiftcards()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found.");

        var businessGiftcards = _giftcardRepository.GetWhere(g => g.BusinessId == user.BusinessId);
        if (businessGiftcards.IsNullOrEmpty())
            return NotFound("No business giftcards found.");

        return Ok(businessGiftcards);
    }

    [HttpGet("{id}", Name = "GetGiftcardById")]
    [Authorize]
    public async Task<IActionResult> GetGiftcardById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Invalid giftcard ID.");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found.");

        var giftcard = await _giftcardRepository.GetGiftcardByIdAndBusinessIdAsync(id.Trim(), user.BusinessId);
        if (giftcard == null)
            return NotFound("Giftcard not found.");

        return Ok(giftcard);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> CreateGiftcard([FromBody] CreateGiftcardDto createGiftcardDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");


            var newGiftcard = new Giftcard
            {
                Balance = createGiftcardDto.BalanceAmount,
                BusinessId = user.BusinessId,
            };

            _giftcardRepository.Add(newGiftcard);
            await _giftcardRepository.SaveChangesAsync();

            return Ok(newGiftcard);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}