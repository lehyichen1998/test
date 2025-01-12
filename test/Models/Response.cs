namespace test.Models
{
    internal class SuccessResponse
    {
        public int result { get; set; }
        public long? totalamount { get; set; }
        public int totaldiscount { get; set; }
        public long? finalamount { get; set; }
    }

    internal class ErrorResponse
    {
        public int result { get; set; }
        public string resultmessage { get; set; }
    }
}