namespace Gymunity.Domain.Entities.ProgramAggregate
{
    public class ProgramWeek : BaseEntity
    {
        public int ProgramId { get; set; }
        public int WeekNumber { get; set; }

        public Program Program { get; set; } = null!;
        public ICollection<ProgramDay> Days { get; set; } = [];
    }
}