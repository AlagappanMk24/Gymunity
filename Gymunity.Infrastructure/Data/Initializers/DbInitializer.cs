using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using Gymunity.Infrastructure.Data.Context;
using KS_Sweets.Infrastructure.Data.Initializers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Data.Initializers
{
    public class DbInitializer(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext dbContext) : IDbInitializer
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly AppDbContext _dbContext = dbContext;

        public async Task InitializeAsync()
        {
            try
            {
                // 1. Apply pending migrations
                await ApplyMigrationsAsync();

                // 2. Seed roles
                await SeedRolesAsync();

                // 3. Seed users
                await SeedUsersAsync();

                // 4. Seed static data (Exercises)
                await SeedExercisesAsync();

                // 5. Seed trainer profile and related data
                await SeedTrainerAndRelatedDataAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Database initialization failed", ex);
            }
        }
        private async Task ApplyMigrationsAsync()
        {
            if ((await _dbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                await _dbContext.Database.MigrateAsync();
            }
        }
        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "Trainer", "Client" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task SeedUsersAsync()
        {
            // Admin user
            var adminEmail = "admin@Gymunity.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    FullName = "Admin User",
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Role = UserRole.Admin
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

                // Assign role
                if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Trainer user
            var trainerEmail = "trainer@Gymunity.com";
            var trainerUser = await _userManager.FindByEmailAsync(trainerEmail);

            if (trainerUser == null)
            {
                trainerUser = new AppUser
                {
                    FullName = "Trainer Test",
                    UserName = "trainerfit",
                    Email = trainerEmail,
                    EmailConfirmed = true,
                    Role = UserRole.Trainer
                };

                var result = await _userManager.CreateAsync(trainerUser, "P@ssw0rd");
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

                if (!await _userManager.IsInRoleAsync(trainerUser, "Trainer"))
                {
                    await _userManager.AddToRoleAsync(trainerUser, "Trainer");
                }
            }

            // Client user
            var clientEmail = "client@Gymunity.com";
            var clientUser = await _userManager.FindByEmailAsync(clientEmail);

            if (clientUser == null)
            {
                clientUser = new AppUser
                {
                    FullName = "Refaat",
                    UserName = "client1",
                    Email = clientEmail,
                    EmailConfirmed = true,
                    Role = UserRole.Client
                };

                var result = await _userManager.CreateAsync(clientUser, "P@ssw0rd");
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

                if (!await _userManager.IsInRoleAsync(clientUser, "Client"))
                {
                    await _userManager.AddToRoleAsync(clientUser, "Client");
                }
            }
        }
        private async Task SeedExercisesAsync()
        {
            if (!await _dbContext.Exercises.AnyAsync())
            {
                var fixedDateTime = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
                var fixedDateTimeOffset = new DateTimeOffset(fixedDateTime);

                var exercises = new List<Exercise>
                {
                    new() { Name = "Barbell Back Squat", Category = "Strength", MuscleGroup = "Quadriceps", Equipment = "Barbell", VideoDemoUrl = "https://picsum.photos/seed/squat-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/squat/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Goblet Squat", Category = "Strength", MuscleGroup = "Quadriceps", Equipment = "Dumbbell", VideoDemoUrl = "https://picsum.photos/seed/goblet-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/goblet/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Romanian Deadlift", Category = "Strength", MuscleGroup = "Hamstrings", Equipment = "Barbell", VideoDemoUrl = "https://picsum.photos/seed/rdl-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/rdl/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Bulgarian Split Squat", Category = "Strength", MuscleGroup = "Quadriceps", Equipment = "Dumbbell", VideoDemoUrl = "https://picsum.photos/seed/bulgarian-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/bulgarian/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Leg Press", Category = "Strength", MuscleGroup = "Quadriceps", Equipment = "Machine", VideoDemoUrl = "https://picsum.photos/seed/legpress-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/legpress/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Leg Curl", Category = "Strength", MuscleGroup = "Hamstrings", Equipment = "Machine", VideoDemoUrl = "https://picsum.photos/seed/legcurl-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/legcurl/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Barbell Bench Press", Category = "Strength", MuscleGroup = "Chest", Equipment = "Barbell", VideoDemoUrl = "https://picsum.photos/seed/bench-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/bench/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Dumbbell Shoulder Press", Category = "Strength", MuscleGroup = "Shoulders", Equipment = "Dumbbell", VideoDemoUrl = "https://picsum.photos/seed/shoulderpress-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/shoulderpress/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Incline Dumbbell Press", Category = "Strength", MuscleGroup = "Chest", Equipment = "Dumbbell", VideoDemoUrl = "https://picsum.photos/seed/incline-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/incline/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Lateral Raise", Category = "Strength", MuscleGroup = "Shoulders", Equipment = "Dumbbell", VideoDemoUrl = "https://picsum.photos/seed/lateral-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/lateral/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Tricep Dips", Category = "Strength", MuscleGroup = "Triceps", Equipment = "Bodyweight", VideoDemoUrl = "https://picsum.photos/seed/dips-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/dips/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Pull-ups", Category = "Strength", MuscleGroup = "Back", Equipment = "Bodyweight", VideoDemoUrl = "https://picsum.photos/seed/pullups-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/pullups/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Barbell Row", Category = "Strength", MuscleGroup = "Back", Equipment = "Barbell", VideoDemoUrl = "https://picsum.photos/seed/row-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/row/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Lat Pulldown", Category = "Strength", MuscleGroup = "Back", Equipment = "Cable", VideoDemoUrl = "https://picsum.photos/seed/latpull-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/latpull/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Face Pulls", Category = "Strength", MuscleGroup = "Shoulders", Equipment = "Cable", VideoDemoUrl = "https://picsum.photos/seed/facepulls-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/facepulls/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Bicep Curl", Category = "Strength", MuscleGroup = "Biceps", Equipment = "Dumbbell", VideoDemoUrl = "https://picsum.photos/seed/bicep-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/bicep/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Plank", Category = "Core", MuscleGroup = "Abs", Equipment = "Bodyweight", VideoDemoUrl = "https://picsum.photos/seed/plank-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/plank/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Cable Crunch", Category = "Core", MuscleGroup = "Abs", Equipment = "Cable", VideoDemoUrl = "https://picsum.photos/seed/crunch-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/crunch/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Walking Lunges", Category = "Strength", MuscleGroup = "Quadriceps", Equipment = "Dumbbell", VideoDemoUrl = "https://picsum.photos/seed/lunges-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/lunges/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Calf Raise", Category = "Strength", MuscleGroup = "Calves", Equipment = "Machine", VideoDemoUrl = "https://picsum.photos/seed/calf-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/calf/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Chest Fly", Category = "Strength", MuscleGroup = "Chest", Equipment = "Dumbbell", VideoDemoUrl = "https://picsum.photos/seed/fly-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/fly/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Hammer Curl", Category = "Strength", MuscleGroup = "Biceps", Equipment = "Dumbbell", VideoDemoUrl = "https://picsum.photos/seed/hammer-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/hammer/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Tricep Pushdown", Category = "Strength", MuscleGroup = "Triceps", Equipment = "Cable", VideoDemoUrl = "https://picsum.photos/seed/pushdown-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/pushdown/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Seated Cable Row", Category = "Strength", MuscleGroup = "Back", Equipment = "Cable", VideoDemoUrl = "https://picsum.photos/seed/cablerow-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/cablerow/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false },
                    new() { Name = "Leg Extension", Category = "Strength", MuscleGroup = "Quadriceps", Equipment = "Machine", VideoDemoUrl = "https://picsum.photos/seed/legext-video/1920/1080", ThumbnailUrl = "https://picsum.photos/seed/legext/400/300", IsCustom = false, TrainerId = null, CreatedAt = fixedDateTimeOffset, UpdatedAt = fixedDateTimeOffset, IsDeleted = false }
                };
                await _dbContext.Exercises.AddRangeAsync(exercises);
                await _dbContext.SaveChangesAsync();
            }
        }
        private async Task SeedTrainerAndRelatedDataAsync()
        {
            // 1. Get the trainer user
            var trainerUser = await _userManager.FindByEmailAsync("trainer@Gymunity.com");
            if (trainerUser == null) return;

            // 2. CHECK IF PROFILE ALREADY EXISTS (Prevents the Duplicate Key Exception)
            var existingProfile = await _dbContext.TrainerProfiles
                .FirstOrDefaultAsync(tp => tp.Handle == "trainerfit" || tp.UserId == trainerUser.Id);

            if (existingProfile == null)
            {
                var fixedDateTime = DateTime.UtcNow;
                var fixedDateTimeOffset = new DateTimeOffset(fixedDateTime);

                var trainerProfile = new TrainerProfile
                {
                    UserId = trainerUser.Id,
                    Handle = "trainerfit",
                    Bio = "Professional fitness trainer with 5+ years experience",
                    YearsExperience = 5,
                    IsVerified = true,
                    VerifiedAt = fixedDateTime,
                    CreatedAt = fixedDateTimeOffset,
                    UpdatedAt = fixedDateTimeOffset,
                    IsDeleted = false
                };

                await _dbContext.TrainerProfiles.AddAsync(trainerProfile);
                await _dbContext.SaveChangesAsync();

                // Seed related data only for a newly created profile
                await SeedProgramAndPackageDataAsync(trainerProfile.Id, trainerUser.Id);
            }
            //else
            //{
            //    // Optional: If profile exists, still check if related data needs seeding
            //    await SeedProgramAndPackageDataAsync(existingProfile.Id, trainerUser.Id);
            //}
        }
        private async Task SeedProgramAndPackageDataAsync(int trainerProfileId, string trainerUserId)
        {
            // Check if Program already exists
            if (!await _dbContext.Programs.AnyAsync(p => p.Id == 2001))
            {
                var fixedDateTimeOffset = new DateTimeOffset(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));

                // Create Program
                var program = new Program
                {
                    TrainerId = trainerUserId,
                    TrainerProfileId = trainerProfileId,
                    Title = "Beginner Strength - 8 Weeks",
                    Description = "8-week foundational strength program",
                    Type = ProgramType.Workout,
                    DurationWeeks = 8,
                    Price = 49.99m,
                    IsPublic = true,
                    MaxClients = 500,
                    ThumbnailUrl = "https://www.example.com/images/programs/beginner-strength-8w.jpg",
                    CreatedAt = fixedDateTimeOffset,
                    UpdatedAt = fixedDateTimeOffset,
                    IsDeleted = false
                };

                await _dbContext.Programs.AddAsync(program);
                await _dbContext.SaveChangesAsync();

                var programId = program.Id;

                // Create ProgramWeeks - DON'T set Id
                var weeks = new List<ProgramWeek>();
                for (int i = 0; i < 4; i++)
                {
                    weeks.Add(new ProgramWeek
                    {
                        ProgramId = programId,
                        WeekNumber = i + 1,
                        CreatedAt = fixedDateTimeOffset,
                        UpdatedAt = fixedDateTimeOffset,
                        IsDeleted = false
                    });
                }

                await _dbContext.ProgramWeeks.AddRangeAsync(weeks);
                await _dbContext.SaveChangesAsync();

                // Create ProgramDays 
                var dayTitles = new[] { "Lower Body A", "Upper Body Push", "Lower Body B", "Upper Body Pull" };
                var dayNotes = new[] { "Squat pattern + accessories", "Chest, shoulders, triceps", "Hinge pattern + unilateral", "Back, biceps, rear delts" };

                var days = new List<ProgramDay>();

                for (int weekIndex = 0; weekIndex < weeks.Count; weekIndex++)
                {
                    for (int dayIndex = 0; dayIndex < 4; dayIndex++)
                    {
                        days.Add(new ProgramDay
                        {
                            ProgramWeekId = weeks[weekIndex].Id,
                            DayNumber = dayIndex + 1,
                            Title = dayTitles[dayIndex],
                            Notes = dayNotes[dayIndex],
                            CreatedAt = fixedDateTimeOffset,
                            UpdatedAt = fixedDateTimeOffset,
                            IsDeleted = false
                        });
                    }
                }

                await _dbContext.ProgramDays.AddRangeAsync(days);
                await _dbContext.SaveChangesAsync();

                // Create ProgramDayExercises
                await SeedProgramDayExercisesAsync(days, fixedDateTimeOffset);
            }

            // Check if Package already exists
            if (!await _dbContext.Packages.AnyAsync(p => p.Name == "Starter Pack"))
            {
                var fixedDateTimeOffset = new DateTimeOffset(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));

                // Create Package
                var package = new Package
                {
                    TrainerId = trainerProfileId,
                    Name = "Starter Pack",
                    Description = "8-week beginner program + monthly check-in",
                    PriceMonthly = 29.99m,
                    Currency = "EGP",
                    FeaturesJson = "{\"allPrograms\":false}",
                    IsActive = true,
                    ThumbnailUrl = "https://www.example.com/images/packages/starter-pack.jpg",
                    CreatedAt = fixedDateTimeOffset.DateTime,
                    UpdatedAt = fixedDateTimeOffset,
                    PromoCode = "STARTER6",
                    IsDeleted = false
                };

                await _dbContext.Packages.AddAsync(package);
                await _dbContext.SaveChangesAsync();

                var packageId = package.Id;
                var program = await _dbContext.Programs.FirstOrDefaultAsync(p => p.Title == "Beginner Strength - 8 Weeks");
                if (program != null)
                {
                    // Create PackageProgram - DON'T set Id
                    var packageProgram = new PackageProgram
                    {
                        PackageId = packageId,
                        ProgramId = program.Id,
                        CreatedAt = fixedDateTimeOffset,
                        UpdatedAt = fixedDateTimeOffset,
                        IsDeleted = false
                    };
                    await _dbContext.PackagePrograms.AddAsync(packageProgram);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        private async Task SeedProgramDayExercisesAsync(List<ProgramDay> days, DateTimeOffset fixedDateTimeOffset)
        {
            var exercises = await _dbContext.Exercises.ToListAsync();
            var exerciseMap = exercises.ToDictionary(e => e.Id);

            // Define exercise templates by Name (since IDs are auto-generated)
            var lowerAExerciseNames = new[] { "Barbell Back Squat", "Romanian Deadlift", "Leg Press", "Leg Curl", "Leg Extension", "Calf Raise", "Plank" };
            var upperPushExerciseNames = new[] { "Barbell Bench Press", "Dumbbell Shoulder Press", "Incline Dumbbell Press", "Chest Fly", "Lateral Raise", "Tricep Dips", "Tricep Pushdown", "Cable Crunch" };
            var lowerBExerciseNames = new[] { "Romanian Deadlift", "Bulgarian Split Squat", "Walking Lunges", "Leg Press", "Leg Curl", "Calf Raise", "Cable Crunch" };
            var upperPullExerciseNames = new[] { "Pull-ups", "Barbell Row", "Lat Pulldown", "Seated Cable Row", "Face Pulls", "Bicep Curl", "Hammer Curl", "Plank" };
            var programDayExercises = new List<ProgramDayExercise>();

            // Exercise details templates
            var exerciseDetails = new Dictionary<string, (string sets, string reps, int rest, string notes)>
            {
                // Lower Body A exercises
                { "Barbell Back Squat", ("4", "6-8", 180, "Main squat movement") },
                { "Romanian Deadlift", ("3", "8-10", 120, "Hamstring focus") },
                { "Leg Press", ("3", "10-12", 90, "Quad volume") },
                { "Leg Curl", ("3", "12-15", 60, "Hamstring isolation") },
                { "Leg Extension", ("3", "12-15", 60, "Quad isolation") },
                { "Calf Raise", ("4", "15-20", 45, "Calf development") },
                { "Plank", ("3", "45-60s", 60, "Core stability") },
    
                // Upper Push exercises
                { "Barbell Bench Press", ("4", "6-8", 180, "Main pressing") },
                { "Dumbbell Shoulder Press", ("3", "8-10", 120, "Overhead press") },
                { "Incline Dumbbell Press", ("3", "8-12", 90, "Incline work") },
                { "Chest Fly", ("3", "12-15", 60, "Chest isolation") },
                { "Lateral Raise", ("3", "12-15", 60, "Lateral delts") },
                { "Tricep Dips", ("3", "8-12", 90, "Tricep compound") },
                { "Tricep Pushdown", ("3", "12-15", 45, "Tricep isolation") },
                { "Cable Crunch", ("3", "30-45s", 60, "Core work") },
    
                // Lower Body B exercises
                { "Bulgarian Split Squat", ("3", "8-10", 120, "Single leg") },
                { "Walking Lunges", ("3", "10-12", 90, "Unilateral") },
    
                // Upper Pull exercises
                { "Pull-ups", ("4", "6-10", 180, "Main pull") },
                { "Barbell Row", ("3", "8-10", 120, "Horizontal pull") },
                { "Lat Pulldown", ("3", "10-12", 90, "Vertical pull") },
                { "Seated Cable Row", ("3", "10-12", 90, "Cable row") },
                { "Face Pulls", ("3", "15-20", 60, "Rear delts") },
                { "Bicep Curl", ("3", "10-12", 60, "Bicep work") },
                { "Hammer Curl", ("3", "10-12", 60, "Hammer curls") },
    
                // Additional exercises that appear in templates
                { "Goblet Squat", ("3", "10-12", 90, "Front-loaded squat") }
            };

            for (int week = 0; week < 4; week++)
            {
                for (int day = 0; day < 4; day++)
                {
                    int dayIndex = week * 4 + day;
                    var exerciseNames = day switch
                    {
                        0 => lowerAExerciseNames,      // Lower Body A
                        1 => upperPushExerciseNames,   // Upper Body Push
                        2 => lowerBExerciseNames,      // Lower Body B
                        3 => upperPullExerciseNames,   // Upper Body Pull
                        _ => lowerAExerciseNames
                    };

                    for (int i = 0; i < exerciseNames.Length; i++)
                    {
                        var exerciseName = exerciseNames[i];
                        var exercise = exercises.FirstOrDefault(e => e.Name == exerciseName);

                        if (exercise != null && exerciseDetails.TryGetValue(exerciseName, out var details))
                        {
                            programDayExercises.Add(new ProgramDayExercise
                            {
                                ProgramDayId = days[dayIndex].Id,
                                ExerciseId = exercise.Id,
                                OrderIndex = i + 1,
                                Sets = details.sets,
                                Reps = details.reps,
                                RestSeconds = details.rest,
                                Notes = details.notes,
                                CreatedAt = fixedDateTimeOffset,
                                UpdatedAt = fixedDateTimeOffset,
                                IsDeleted = false
                            });
                        }
                    }
                }
            }

            await _dbContext.ProgramDayExercises.AddRangeAsync(programDayExercises);
            await _dbContext.SaveChangesAsync();
        }
    }
}