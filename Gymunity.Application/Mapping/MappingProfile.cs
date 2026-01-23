using AutoMapper;
using Gymunity.Application.DTOs.ClientDto;
using Gymunity.Application.DTOs.Packages;
using Gymunity.Application.DTOs.Program;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Application.DTOs.User.Payment;
using Gymunity.Application.DTOs.User.Subscriptions;
using Gymunity.Application.Mapping.Resolvers;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using ITI.Gymunity.FP.Application.DTOs.Client;
using System.Text.Json;

namespace Gymunity.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //*********************     Client Profile Mapping       ******************************//
            CreateMap<ClientProfileRequest, ClientProfile>()
                .ForMember(dest => dest.ExperienceLevel, opt => opt.MapFrom(r => r.ExperienceLevel.ToString() ?? ""))
                .ForMember(dest => dest.Goal, opt => opt.MapFrom(r => r.Goal.ToString() ?? ""))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(r => r.Gender.ToString()));
            CreateMap<ClientProfile, ClientProfileResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(cp => cp.User.UserName))
                .ForMember(dest => dest.BodyStateLog, opt => opt.MapFrom(src => src.BodyStatLogs
                .OrderByDescending(b => b.LoggedAt).FirstOrDefault()))
                .ForMember(dest => dest.ExperienceLevel, opt => opt.MapFrom(p => Enum.Parse<ExperienceLevel>(p.ExperienceLevel ?? "")))
                .ForMember(dest => dest.Goal, opt => opt.MapFrom(p => Enum.Parse<ClientGoal>(p.Goal ?? "")))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(p => Enum.Parse<Gender>(p.Gender ?? "")));


            CreateMap<TrainerProfile, TrainerCardDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.ProfilePhotoUrl, opt => opt.MapFrom(src => src.User.ProfilePhotoUrl))
            .ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src => src.TrainerReviews.Count))
            .ForMember(dest => dest.Specializations, opt => opt.Ignore()) // Can be added if you have this field
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => "EGP"))
            .ForMember(dest => dest.HasActiveSubscription, opt => opt.Ignore());

            // TrainerProfile to List Response (excludes status fields)
            CreateMap<TrainerProfile, TrainerProfileListResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(tp => tp.User.UserName))
                .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileListResponse>, string?>(tp => tp.CoverImageUrl))
                .ForMember(dest => dest.VideoIntroUrl, opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileListResponse>, string?>(tp => tp.VideoIntroUrl));

            // TrainerProfile to Detail Response (includes status fields)
            CreateMap<TrainerProfile, TrainerProfileDetailResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(tp => tp.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(tp => tp.User.Email))
                .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileDetailResponse>, string?>(tp => tp.CoverImageUrl))
                .ForMember(dest => dest.VideoIntroUrl, opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileDetailResponse>, string?>(tp => tp.VideoIntroUrl))
                .ForMember(dest => dest.StatusImageUrl, opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileDetailResponse>, string?>(tp => tp.StatusImageUrl));

            CreateMap<ClientProfile, ClientProfileDashboardResponse>()
                    .ForMember(dest => dest.UserName,
                        opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "User"))
                    .ForMember(dest => dest.LastBodyState,
                        opt => opt.MapFrom(src => src.BodyStatLogs != null && src.BodyStatLogs.Any()
                            ? src.BodyStatLogs.OrderByDescending(b => b.LoggedAt).First()
                            : null))
                    .ForMember(dest => dest.BodyStateHistory,
                        opt => opt.MapFrom(src => src.BodyStatLogs != null
                            ? src.BodyStatLogs.OrderByDescending(b => b.LoggedAt).ToList()
                            : new List<BodyStatLog>()));

            // ======================
            // Program mappings
            // Used by:
            // - ProgramsController (GET/POST/PUT/DELETE/Search)
            // - ProgramManagerService / ProgramService
            // - HomeClientController for client-facing program responses
            // ======================
            CreateMap<Program, ProgramGetAllResponse>()
                .ForMember(dest => dest.TrainerUserName, opt => opt.MapFrom(p => p.TrainerProfile != null ? p.TrainerProfile.User.UserName : null))
                .ForMember(dest => dest.TrainerHandle, opt => opt.MapFrom(p => p.TrainerProfile != null ? p.TrainerProfile.Handle : null))
                .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(p => p.TrainerProfileId))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom<GenericImageUrlResolver<Program, ProgramGetAllResponse>, string?>(p => p.ThumbnailUrl));

            CreateMap<Program, ProgramGetByIdResponse>()
                .ForMember(dest => dest.TrainerUserName, opt => opt.MapFrom(p => p.TrainerProfile != null ? p.TrainerProfile.User.UserName : null))
                .ForMember(dest => dest.TrainerHandle, opt => opt.MapFrom(p => p.TrainerProfile != null ? p.TrainerProfile.Handle : null))
                .ForMember(dest => dest.TrainerEmail, opt => opt.MapFrom(p => p.TrainerProfile != null ? p.TrainerProfile.User.Email : null))
                .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(p => p.TrainerProfileId))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom<GenericImageUrlResolver<Program, ProgramGetByIdResponse>, string?>(p => p.ThumbnailUrl));

            // Program -> ProgramResponse (used by ProgramService)
            CreateMap<Program, ProgramResponse>()
                .ForMember(dest => dest.TrainerUserName, opt => opt.MapFrom(p => p.TrainerProfile != null ? p.TrainerProfile.User.UserName : null))
                .ForMember(dest => dest.TrainerHandle, opt => opt.MapFrom(p => p.TrainerProfile != null ? p.TrainerProfile.Handle : null))
                .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(p => p.TrainerProfileId))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom<GenericImageUrlResolver<Program, ProgramResponse>, string?>(p => p.ThumbnailUrl));

            CreateMap<Program, ProgramClientResponse>()
                .ForMember(dest => dest.TrainerUserName, opt => opt.MapFrom(p => p.TrainerProfile != null ? p.TrainerProfile.User.UserName : null))
                .ForMember(dest => dest.TrainerHandle, opt => opt.MapFrom(p => p.TrainerProfile != null ? p.TrainerProfile.Handle : null))
                .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(p => p.TrainerProfileId))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom<GenericImageUrlResolver<Program, ProgramClientResponse>, string?>(p => p.ThumbnailUrl));

            // ======================
            // Week / Day / DayExercise mappings
            // Used by:
            // - WeeksController (GET/POST/PUT/DELETE)
            // - DaysController (GET/POST/PUT/DELETE)
            // - DayExercisesController (GET/POST/PUT/DELETE)
            // ======================
            CreateMap<ProgramWeek, ProgramWeekGetAllResponse>();
            CreateMap<ProgramDay, ProgramDayGetAllResponse>();
            CreateMap<ProgramDayExercise, ProgramDayExerciseGetAllResponse>()
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom<GenericImageUrlResolver<ProgramDayExercise, ProgramDayExerciseGetAllResponse>, string?>(p => p.VideoUrl));

            // ======================
            // Package mappings (updated)
            // Used by:
            // - PackagesController / PackageService
            // - HomeClientService for client-facing package responses
            // ======================
            CreateMap<Package, PackageResponse>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(p => new DateTimeOffset(p.CreatedAt)))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(p => p.UpdatedAt))
                //.ForMember(dest => dest.IsAnnual, opt => opt.MapFrom(p => p.IsAnnual))
                .ForMember(dest => dest.PromoCode, opt => opt.MapFrom(p => p.PromoCode))
                .ForMember(dest => dest.ProgramIds, opt => opt.MapFrom(p => p.PackagePrograms != null ? p.PackagePrograms.Where(pp => !pp.IsDeleted).Select(pp => pp.ProgramId).ToArray() : new int[0]))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom<GenericImageUrlResolver<Package, PackageResponse>, string?>(p => p.ThumbnailUrl));

            CreateMap<PackageCreateRequest, Package>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Trainer, opt => opt.Ignore())
                .ForMember(dest => dest.PackagePrograms, opt => opt.Ignore())
                .ForMember(dest => dest.Subscriptions, opt => opt.Ignore());
            CreateMap<PackageUpdateRequest, Package>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Subscription, SubscriptionResponse>()
             .ForMember(dest => dest.PackageId,
                 opt => opt.MapFrom(src => src.PackageId))

             .ForMember(dest => dest.PackageName,
                 opt => opt.MapFrom(src => src.Package != null ? src.Package.Name : string.Empty))

             .ForMember(dest => dest.PackageDescription,
                 opt => opt.MapFrom(src => src.Package != null ? src.Package.Description : string.Empty))


             .ForMember(dest => dest.TrainerId,
                 opt => opt.MapFrom(src =>
                     src.Package != null && src.Package.Trainer != null
                         ? src.Package.Trainer.UserId
                         : string.Empty))

             .ForMember(dest => dest.TrainerProfileId,
                 opt => opt.MapFrom(src =>
                     src.Package != null && src.Package.Trainer != null
                         ? src.Package.Trainer.Id
                         : 0))

             .ForMember(dest => dest.TrainerName,
                 opt => opt.MapFrom(src =>
                     src.Package != null && src.Package.Trainer != null && src.Package.Trainer.User != null
                         ? src.Package.Trainer.User.FullName
                         : string.Empty))

             .ForMember(dest => dest.TrainerHandle,
                 opt => opt.MapFrom(src =>
                     src.Package != null && src.Package.Trainer != null
                         ? src.Package.Trainer.Handle
                         : string.Empty))

             .ForMember(dest => dest.TrainerPhotoUrl,
                 opt => opt.MapFrom(src =>
                     src.Package != null && src.Package.Trainer != null && src.Package.Trainer.User != null
                         ? src.Package.Trainer.User.ProfilePhotoUrl
                         : null))

             .ForMember(dest => dest.FeaturesIncluded,
                 opt => opt.MapFrom(src =>
                     src.Package != null
                         ? ParseFeatures(src.Package.FeaturesJson)
                         : new List<string>()));

            CreateMap<Payment, PaymentResponse>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId))

              // Client Information
              .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))
              .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src =>
                  src.Subscription != null && src.Subscription.Client != null
                      ? src.Subscription.Client.FullName
                      : string.Empty))
              .ForMember(dest => dest.ClientEmail, opt => opt.MapFrom(src =>
                  src.Subscription != null && src.Subscription.Client != null
                      ? src.Subscription.Client.Email ?? string.Empty
                      : string.Empty))

              // Subscription & Package Information
              .ForMember(dest => dest.PackageId, opt => opt.MapFrom(src =>
                  src.Subscription != null && src.Subscription.Package != null
                      ? src.Subscription.Package.Id
                      : 0))
              .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src =>
                  src.Subscription != null && src.Subscription.Package != null
                      ? src.Subscription.Package.Name
                      : string.Empty))
              .ForMember(dest => dest.SubscriptionStatus, opt => opt.MapFrom(src =>
                  src.Subscription != null
                      ? (SubscriptionStatus?)src.Subscription.Status
                      : null))
              .ForMember(dest => dest.SubscriptionStartDate, opt => opt.MapFrom(src =>
                  src.Subscription != null
                      ? (DateTime?)src.Subscription.StartDate
                      : null))
              .ForMember(dest => dest.SubscriptionEndDate, opt => opt.MapFrom(src =>
                  src.Subscription != null
                      ? (DateTime?)src.Subscription.CurrentPeriodEnd
                      : null))
              .ForMember(dest => dest.IsAnnualSubscription, opt => opt.MapFrom(src =>
                  src.Subscription != null
                      ? src.Subscription.IsAnnual
                      : false))

              // Trainer Information
              .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(src =>
                  src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null
                      ? src.Subscription.Package.Trainer.Id
                      : 0))
              .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src =>
                  src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null && src.Subscription.Package.Trainer.User != null
                      ? src.Subscription.Package.Trainer.User.FullName
                      : string.Empty))
              .ForMember(dest => dest.TrainerHandle, opt => opt.MapFrom(src =>
                  src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null
                      ? src.Subscription.Package.Trainer.Handle
                      : null))
              .ForMember(dest => dest.IsTrainerVerified, opt => opt.MapFrom(src =>
                  src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null
                      ? src.Subscription.Package.Trainer.IsVerified
                      : false))
              .ForMember(dest => dest.TrainerRating, opt => opt.MapFrom(src =>
                  src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null
                      ? (decimal?)src.Subscription.Package.Trainer.RatingAverage
                      : null))
              .ForMember(dest => dest.TrainerTotalClients, opt => opt.MapFrom(src =>
                  src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null
                      ? (int?)src.Subscription.Package.Trainer.TotalClients
                      : null))

              // Payment Amount Details
              .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
              .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
              .ForMember(dest => dest.PlatformFee, opt => opt.MapFrom(src => src.PlatformFee))
              .ForMember(dest => dest.TrainerPayout, opt => opt.MapFrom(src => src.TrainerPayout))

              // Status & Method
              .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
              .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method))

              // Transaction Information
              .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src =>
                  src.Method == PaymentMethod.Paymob
                      ? src.PaymobTransactionId
                      : src.PayPalCaptureId))
              .ForMember(dest => dest.FailureReason, opt => opt.MapFrom(src => src.FailureReason))

              // Payment URLs
              .ForMember(dest => dest.PaymentUrl, opt => opt.Ignore())
              .ForMember(dest => dest.PaymobOrderId, opt => opt.MapFrom(src => src.PaymobOrderId))
              .ForMember(dest => dest.PayPalOrderId, opt => opt.MapFrom(src => src.PayPalOrderId))

              // Dates
              .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
              .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(src => src.PaidAt))
              .ForMember(dest => dest.FailedAt, opt => opt.MapFrom(src => src.FailedAt));

        }
        private static List<string> ParseFeatures(string featuresJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(featuresJson) || featuresJson == "{}")
                    return new List<string>();

                var features = JsonSerializer.Deserialize<Dictionary<string, object>>(featuresJson);

                if (features == null)
                    return new List<string>();

                var featureList = new List<string>();

                // Extract common features
                if (features.TryGetValue("programs", out var programs))
                    featureList.Add($"Access to {programs} program(s)");

                if (features.TryGetValue("communityAccess", out var community) && community.ToString() == "True")
                    featureList.Add("Community access");

                if (features.TryGetValue("formChecks", out var formChecks))
                    featureList.Add($"{formChecks} form check(s) per week");

                if (features.TryGetValue("customProgram", out var customProg) && customProg.ToString() == "True")
                    featureList.Add("Custom program every 8 weeks");

                if (features.TryGetValue("messaging", out var messaging) && messaging.ToString() == "True")
                    featureList.Add("1:1 messaging with trainer");

                if (features.TryGetValue("videoCalls", out var calls))
                    featureList.Add($"{calls} video call(s) per month");

                return featureList;
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}