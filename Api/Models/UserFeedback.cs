namespace API.Models
{
    public class UserFeedback
    {
        public string Text { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
    }
}
