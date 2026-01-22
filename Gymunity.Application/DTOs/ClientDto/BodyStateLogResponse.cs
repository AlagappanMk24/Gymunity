namespace Gymunity.Application.DTOs.ClientDto
{
    public class BodyStateLogResponse
    {
        public decimal? WeightKg { get; set; }
        public decimal? BodyFatPercent { get; set; }
        public string? MeasurementsJson { get; set; } // { "neck": 40, "waist": 80, ... }
        public string? PhotoFrontUrl { get; set; }
        public string? PhotoSideUrl { get; set; }
        public string? PhotoBackUrl { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset LoggedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}