namespace Gymunity.Application.DTOs.Program
{
    public class ProgramWeekGetAllResponse
    {
        public int Id { get; set; }
        public int ProgramId { get; set; }
        public int WeekNumber { get; set; }
        public ICollection<ProgramDayGetAllResponse> Days { get; set; } = new List<ProgramDayGetAllResponse>();
    }
}