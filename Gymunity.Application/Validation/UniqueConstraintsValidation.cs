using Gymunity.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.Validation
{
    public class UniqueTrainerHandleAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var handle = value as string;
            if (string.IsNullOrWhiteSpace(handle)) return ValidationResult.Success!;

            var repo = (ITrainerProfileRepository?)validationContext.GetService(typeof(ITrainerProfileRepository));
            if (repo == null) return ValidationResult.Success!; // can't validate without repo

            var exists = repo.HandleExistsAsync(handle).GetAwaiter().GetResult();
            if (exists)
                return new ValidationResult($"Handle '{handle}' already exists.");

            return ValidationResult.Success!;
        }
    }

    public class UniquePackageNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var name = value as string;
            if (string.IsNullOrWhiteSpace(name)) return ValidationResult.Success!;

            // the object instance should be the DTO with TrainerId property
            var instance = validationContext.ObjectInstance;
            var trainerIdProp = instance.GetType().GetProperty("TrainerId") ?? instance.GetType().GetProperty("TrainerProfileId");
            if (trainerIdProp == null) return ValidationResult.Success!; // cannot validate without trainer id

            var trainerIdObj = trainerIdProp.GetValue(instance);
            if (trainerIdObj == null) return ValidationResult.Success!;

            var trainerId = Convert.ToInt32(trainerIdObj);
            if (trainerId <= 0) return ValidationResult.Success!;

            var pkgRepo = (IPackageRepository?)validationContext.GetService(typeof(IPackageRepository));
            if (pkgRepo == null) return ValidationResult.Success!;

            var list = pkgRepo.GetByTrainerIdAsync(trainerId).GetAwaiter().GetResult();
            var exists = list.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (exists)
                return new ValidationResult($"A package with name '{name}' already exists for this trainer.");

            return ValidationResult.Success!;
        }
    }

    public class UniqueProgramTitleAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var title = value as string;
            if (string.IsNullOrWhiteSpace(title)) return ValidationResult.Success!;

            var instance = validationContext.ObjectInstance;
            var trainerIdProp = instance.GetType().GetProperty("TrainerProfileId") ?? instance.GetType().GetProperty("TrainerId");
            if (trainerIdProp == null) return ValidationResult.Success!;

            var trainerIdObj = trainerIdProp.GetValue(instance);
            if (trainerIdObj == null) return ValidationResult.Success!;

            var trainerId = Convert.ToInt32(trainerIdObj);
            if (trainerId <= 0) return ValidationResult.Success!;

            var progRepo = (IProgramRepository?)validationContext.GetService(typeof(IProgramRepository));
            if (progRepo == null) return ValidationResult.Success!;

            // reuse repository method to get by profile id
            var list = progRepo.GetByTrainerAsyncProfileId(trainerId).GetAwaiter().GetResult();
            var exists = list.Any(p => string.Equals(p.Title, title, StringComparison.OrdinalIgnoreCase));
            if (exists)
                return new ValidationResult($"A program with title '{title}' already exists for this trainer.");

            return ValidationResult.Success!;
        }
    }
}
