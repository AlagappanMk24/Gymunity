using Gymunity.Domain.Enums;

namespace Gymunity.Application.DTOs.Program
{
	public class ProgramResponse
	{
		public int Id { get; set; }
		public string Title { get; set; } = null!;
		public string Description { get; set; } = string.Empty;
		public ProgramType Type { get; set; }
		public int DurationWeeks { get; set; }
        public int TotalExercises { get; set; }
        public decimal? Price { get; set; }
		public bool IsPublic { get; set; }
		public int? MaxClients { get; set; }
		public string? ThumbnailUrl { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }
		// reference to TrainerProfile
		public int? TrainerProfileId { get; set; }
		public string? TrainerUserName { get; set; }
		public string? TrainerHandle { get; set; }

	}
}