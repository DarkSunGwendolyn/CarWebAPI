namespace UserWebAPI.Models.Kafka
{
    public class ConfirmationResponse
    {
        public string? ObjectId { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }

        public DateTime? ConfirmedAt { get; set; }
    }
}
