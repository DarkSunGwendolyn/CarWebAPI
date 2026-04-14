namespace CarWebAPI.Models.Kafka
{
    public class ConfirmationRequest
    {
        public string? ObjectId { get; set; }
        public string? UserId { get; set; }

        public DateTime RequestTime { get; set; }
    }
}
