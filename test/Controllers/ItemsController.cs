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
using log4net;

namespace test.Controllers
{
    [Route("api")]
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

        private static readonly ILog log = LogManager.GetLogger(typeof(ItemsController)); // Get logger instance
        /*
        {
  "partnerkey": "FAKEGOOGLE",
  "partnerrefno": "FG-00001",
  "partnerpassword": "RkFLRVBBU1NXT1JEMTIzNA==",
  "totalamount": 1000,
  "items": [
    {
      "partneritemref": "i-00001",
      "name": "Pen",
      "qty": 4,
      "unitprice": 200
    },
    {
      "partneritemref": "i-00002",
      "name": "Ruler",
      "qty": 2,
      "unitprice": 100
    }
  ],
  "timestamp": "2024-08-15T02:11:22.0000000Z",
  "sig": " MDE3ZTBkODg4ZDNhYzU0ZDBlZWRmNmU2NmUyOWRhZWU4Y2M1NzQ1OTIzZGRjYTc1ZGNjOTkwYzg2MWJlMDExMw=="
}

        */

        [HttpPost("submittrxmessage")]
        public IActionResult requestApi(Models.Request request)
        {

            string requestBody = System.Text.Json.JsonSerializer.Serialize(request);
            log.Info($"Request Body: {requestBody}");
            // Validate model state
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    result = 0,
                    resultmessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                };
                string responseBody = System.Text.Json.JsonSerializer.Serialize(errorResponse);
                log.Error($"Validation Error Response: {responseBody}");
                return BadRequest(errorResponse);
            }

            try
            {
                // Partner authentication
                if (!ItemRepository.AuthenticatePartner(request.partnerrefno, request.partnerpassword, request.partnerkey, AllowedPartners))
                {
                    var errorResponse = new ErrorResponse { result = 0, resultmessage = "Invalid Partner Key, Password, or Ref No." };
                    string responseBody = System.Text.Json.JsonSerializer.Serialize(errorResponse);
                    log.Warn($"Authentication Failed Response: {responseBody}");
                    return Unauthorized(errorResponse);
                }

                // Timestamp validation
                (bool isValid, string errorMessage) = ItemRepository.IsValidTimestamp(request.timestamp);
                if (!isValid)
                {
                    var errorResponse = new ErrorResponse { result = 0, resultmessage = errorMessage };
                    string responseBody = System.Text.Json.JsonSerializer.Serialize(errorResponse);
                    log.Warn($"Timestamp Validation Failed Response: {responseBody}");
                    return BadRequest(errorResponse);
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
                var successResponse = new SuccessResponse
                {
                    result = 1,
                    totalamount = totalAmount,
                    totaldiscount = (int)discountAmount,
                    finalamount = finalAmount
                };
                string successResponseBody = System.Text.Json.JsonSerializer.Serialize(successResponse);
                log.Info($"Success Response: {successResponseBody}");
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return StatusCode(500, new ErrorResponse { result = 0, resultmessage = "An error occurred during processing." });
            }
        }







    }
}
