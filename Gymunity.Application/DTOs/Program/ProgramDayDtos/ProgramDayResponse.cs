namespace Gymunity.Application.DTOs.Program.ProgramDayDtos
{
    public class ProgramDayResponse
    {
        public int Id { get; set; }
        public int ProgramWeekId { get; set; }
        public int DayNumber { get; set; } // 1–7
        public string? Title { get; set; } // "Lower Body A", "Rest", etc.
        public string? Notes { get; set; }
        public List<ProgramDayExerciseResponse> Exercises { get; set; }
    }
}