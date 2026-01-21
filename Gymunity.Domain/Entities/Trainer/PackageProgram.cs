using Gymunity.Domain.Entities.ProgramAggregate;

namespace Gymunity.Domain.Entities.Trainer
{
    public class PackageProgram : BaseEntity
    {
        public int PackageId { get; set; }
        public Package Package { get; set; } = null!;

        public int ProgramId { get; set; }
        public Program Program { get; set; } = null!;
    }
}
