using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PoS_Placeholder.Server.Data;
using PoS_Placeholder.Server.Models;
using PoS_Placeholder.Server.Models.Dto;
using PoS_Placeholder.Server.Models.Enum;
using PoS_Placeholder.Server.Repositories;
using Stripe;
using PaymentMethod = PoS_Placeholder.Server.Models.Enum.PaymentMethod;

namespace PoS_Placeholder.Server.Controllers;

[ApiController]
[Route("/api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly OrderRepository _orderRepository;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _db;

    public PaymentController(IConfiguration configuration, OrderRepository orderRepository,
        UserManager<User> userManager, ApplicationDbContext db)
    {
        _configuration = configuration;
        _orderRepository = orderRepository;
        _userManager = userManager;
        _db = db;
    }

    [HttpPost("makepayment")]
    [Authorize]
    public async Task<IActionResult> MakePayment([FromBody] PaymentRequestDto paymentRequestDto)
    {
        StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)Math.Round(paymentRequestDto.TotalAmount * 100m),
            Currency = "usd",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        };
        var service = new PaymentIntentService();
        var paymentIntentResponse = await service.CreateAsync(options);

        var paymentResponseDto = new PaymentResponseDto
        {
            ClientSecret = paymentIntentResponse.ClientSecret,
            PaymentIntentId = paymentIntentResponse.Id
        };

        return Ok(paymentResponseDto);
    }

    [HttpPost("refund/{orderId:int}")]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> RefundOrder([FromRoute] int orderId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found.");

        var order = await _orderRepository.GetOrderAndPaymentsByUserIdAndBID(orderId, user.BusinessId);
        if (order == null)
            return NotFound("Order not found.");
        
        if (order.Status == OrderStatus.Refunded)
            return BadRequest("Order is already refunded.");

        using (var transaction = await _db.Database.BeginTransactionAsync())
        {
            try
            {
                decimal totalPricePaid = 0m;
                foreach (var payment in order.Payments)
                {
                    switch (payment.Method)
                    {
                        case PaymentMethod.Cash:
                            order.Status = OrderStatus.Refunded;
                            _orderRepository.Update(order);
                            await _db.SaveChangesAsync();
                            totalPricePaid += payment.PaidPrice;
                            break;
                        case PaymentMethod.GiftCard:
                            if (payment.GiftCardId == null)
                            {
                                await transaction.RollbackAsync();
                                return NotFound("Gift card to process refund was not found.");
                            }

                            var giftcard = _db.Giftcards.FirstOrDefault(g => g.Id == payment.GiftCardId);
                            if (giftcard == null)
                                return NotFound("Gift card no longer exists (Not found).");

                            giftcard.Balance += payment.PaidPrice;
                            _db.Update(giftcard);
                            order.Status = OrderStatus.Refunded;
                            _orderRepository.Update(order);
                            await _db.SaveChangesAsync();
                            totalPricePaid += payment.PaidPrice;
                            break;
                        case PaymentMethod.Card:
                            if (payment.PaymentIntentId == null)
                            {
                                await transaction.RollbackAsync();
                                return NotFound("Transaction (Payment Intent Id) to process refund was not found.");
                            }

                            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];

                            var refundService = new RefundService();
                            var refundOptions = new RefundCreateOptions
                            {
                                Amount = (long)Math.Round(payment.PaidPrice * 100m),
                                PaymentIntent = payment.PaymentIntentId
                            };

                            var refundResponse = await refundService.CreateAsync(refundOptions);
                            if (refundResponse.Status == "succeeded")
                            {
                                order.Status = OrderStatus.Refunded;
                                _orderRepository.Update(order);
                                await _db.SaveChangesAsync();
                                totalPricePaid += payment.PaidPrice;
                            }
                            else
                            {
                                await transaction.RollbackAsync();
                                return StatusCode(StatusCodes.Status500InternalServerError,
                                    "Stripe refund failed. Status: " + refundResponse.Status);
                            }
                            break;
                        default:
                            await transaction.RollbackAsync();
                            return BadRequest("Unknown payment method for refund.");
                    }
                }
                await transaction.CommitAsync();
                
                var orderResponseDtoWithStatusChanged = new OrderResponseDto
                {
                    Id = order.Id,
                    Tip = order.Tip,
                    Date = order.Date,
                    Status = order.Status.ToString(),
                    TotalPrice = totalPricePaid,
                    SubTotal = null,
                    TaxesTotal = null,
                    DiscountsTotal = null,
                    Products = order.Products.Select(pa => new OrderProductDto
                    {
                        FullName = pa.FullName,
                        Price = pa.Price,
                        Quantity = pa.Quantity
                    }).ToList()
                };
                
                return Ok(orderResponseDtoWithStatusChanged);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error: {e.Message}, StackTrace: {e.StackTrace}");
            }
        }
    }
}