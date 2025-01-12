using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using test.Models;
using test.Data;
using BCrypt.Net;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Azure.Core;
using test.Repositories;

namespace test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly APIContext _context;

        public ItemsController(APIContext context)
        {
            _context = context;
        }

        private static readonly Dictionary<string, Partner> AllowedPartners = new Dictionary<string, Partner>
        {
            { "FG-00001", new Partner { Name = "FAKEGOOGLE", Password = "FAKEPASSWORD1234" } },
            { "FG-00002", new Partner { Name = "FAKEPEOPLE", Password = "FAKEPASSWORD4578" } }
        };


        [HttpPost]
        public IActionResult requestApi(Models.Request request)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    result = 0,
                    resultmessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            try
            {
                // Partner authentication
                if (!ItemRepository.AuthenticatePartner(request.partnerrefno, request.partnerpassword, request.partnerkey,AllowedPartners))
                {
                    return Unauthorized(new ErrorResponse { result = 0, resultmessage = "Invalid Partner Key, Password, or Ref No." });
                }

                // Timestamp validation
                (bool isValid, string errorMessage) = ItemRepository.IsValidTimestamp(request.timestamp);
                if (!isValid)
                {
                    return StatusCode(400, new ErrorResponse { result = 0, resultmessage = errorMessage });
                }

                // Validate item details and total amount consistency
                ItemRepository.ValidateItemDetailsAndTotalAmount(request);

                // Calculate total amount (if items provided)
                long totalAmount = ItemRepository.CalculateTotalAmount(request.items);

                // Calculate discounts
                (decimal baseDiscountPercent, decimal conditionalDiscountPercent) = ItemRepository.CalculateDiscounts(totalAmount);

                // Apply maximum discount cap
                decimal totalDiscountPercent = Math.Min(baseDiscountPercent + conditionalDiscountPercent, 0.20m);

                // Calculate final amount
                long discountAmount = (long)(totalAmount * totalDiscountPercent);
                long finalAmount = totalAmount - discountAmount;

                // Return success response
                return Ok(new SuccessResponse
                {
                    result = 1,
                    totalamount = totalAmount,
                    totaldiscount = (int)discountAmount,
                    finalamount = finalAmount
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return StatusCode(500, new ErrorResponse { result = 0, resultmessage = "An error occurred during processing." });
            }
        }


      




    }
}
