using Azure.Core;
using System.Globalization;
using System.Text;
using test.Models;

namespace test.Repositories
{
    public class ItemRepository
    {
        internal static (decimal, decimal) CalculateDiscounts(long totalAmount)
        {
            decimal baseDiscountPercent = 0;
            decimal conditionalDiscountPercent = 0;

            // Base Discount
            if (totalAmount >= 200 && totalAmount <= 500)
            {
                baseDiscountPercent = 0.05m;
            }
            else if (totalAmount >= 501 && totalAmount <= 800)
            {
                baseDiscountPercent = 0.07m;
            }
            else if (totalAmount >= 801 && totalAmount <= 1200)
            {
                baseDiscountPercent = 0.10m;
            }
            else if (totalAmount > 1200)
            {
                baseDiscountPercent = 0.15m;
            }

            // Conditional Discounts
            if (totalAmount > 500 && IsPrime(totalAmount))
            {
                conditionalDiscountPercent += 0.08m;
            }

            if (totalAmount > 900 && totalAmount % 10 == 5)
            {
                conditionalDiscountPercent += 0.10m;
            }

            return (baseDiscountPercent, conditionalDiscountPercent);
        }

        internal static bool IsPrime(long number)
        {
            if (number <= 1) return false;
            if (number <= 3) return true;
            if (number % 2 == 0 || number % 3 == 0) return false;

            for (long i = 5; i * i <= number; i += 6)
            {
                if (number % i == 0 || number % (i + 2) == 0)
                    return false;
            }

            return true;
        }

        internal static bool AuthenticatePartner(string partnerRefNo, string partnerPassword, string partnerKey, Dictionary<string, Partner> AllowedPartners)
        {
            if (!AllowedPartners.TryGetValue(partnerRefNo, out var partner) ||
                 Convert.ToBase64String(Encoding.UTF8.GetBytes(partner.Password)) != partnerPassword ||
                 partner.Name != partnerKey) //Check partner ref no
            {
                return false;
            }

            return true;
        }

        internal static void ValidateItemDetailsAndTotalAmount(Models.Request request)
        {
            if (request.items != null && request.items.Length > 0)
            {
                if (!request.totalamount.HasValue)
                {
                    throw new Exception("Total amount is required when item details are provided.");
                }

                long calculatedTotal = request.items.Sum(item => (long)(item.qty * (item.unitprice/100)));
                if (request.totalamount != calculatedTotal)
                {
                    throw new Exception("Total amount does not match the sum of item details.");
                }
            }
            else if (request.totalamount.HasValue)
            {
                throw new Exception("Total amount should not be provided when item details are not provided.");
            }
        }

        internal static long CalculateTotalAmount(Models.Items[] items)
        {
            if (items == null || items.Length == 0)
            {
                return 0;
            }

            return items.Sum(item => (long)(item.qty * (item.unitprice/100)));
        }

        internal static (bool isValid, string errorMessage) IsValidTimestamp(string timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
            {
                return (false, "Timestamp is required.");
            }

            if (DateTime.TryParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime requestTime))
            {
                DateTime serverTimeUtc = DateTime.UtcNow;
                TimeSpan timeDifference = requestTime - serverTimeUtc;

                //if (Math.Abs(timeDifference.TotalMinutes) > 5)
                //{
                //    return (false, "Timestamp is outside the allowed 5-minute window."); // Or a more specific message
                //}
            }
            else if (DateTime.TryParseExact(timestamp, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime requestTime2))
            {
                DateTime serverTimeUtc = DateTime.UtcNow;
                TimeSpan timeDifference = requestTime2 - serverTimeUtc;

                if (Math.Abs(timeDifference.TotalMinutes) > 5)
                {
                    return (false, "Timestamp is outside the allowed 5-minute window."); // Or a more specific message
                }
            }

            return (true, "Valid.");
        }
    }
}
