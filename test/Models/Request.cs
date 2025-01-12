using System.ComponentModel.DataAnnotations;

namespace test.Models
{
    public class Request
    {
        [Required(ErrorMessage = "Partner Key is required.")]
        [RegularExpression(@"\S+", ErrorMessage = "Partner Key cannot consist only of whitespace.")]
        [MaxLength(50, ErrorMessage = "Partner Key cannot exceed 50 characters.")]
        public string partnerkey { get; set; }
        [Required(ErrorMessage = "partnerrefno is required.")]
        [RegularExpression(@"\S+", ErrorMessage = "partnerrefno cannot consist only of whitespace.")]
        [MaxLength(50, ErrorMessage = "Partner ref no cannot exceed 50 characters.")]
        public string partnerrefno { get; set; }
        [Required(ErrorMessage = "partnerpassword is required.")]
        [RegularExpression(@"\S+", ErrorMessage = "partnerpassword cannot consist only of whitespace.")]
        [MaxLength(50, ErrorMessage = "Partner password cannot exceed 50 characters.")]
        public string partnerpassword { get; set; }
        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0, long.MaxValue, ErrorMessage = "Total amount must be a non-negative number.")] // Optional: Add range validation
        public long? totalamount { get; set; }
        public Items[] items { get; set; }
        [Required(ErrorMessage = "Timestamp is required.")]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d{1,7})?Z$", ErrorMessage = "Timestamp must be in ISO 8601 UTC format (YYYY-MM-DDTHH:mm:ss.ffffffZ).")]
        public string timestamp { get; set; }
        [Required(ErrorMessage = "Signature is required.")]
        public string sig { get; set; }
    }
}
