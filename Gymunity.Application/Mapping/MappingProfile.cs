using AutoMapper;
using Gymunity.Application.DTOs.Admin;
using Gymunity.Application.DTOs.Client;
using Gymunity.Application.DTOs.ClientDto;
using Gymunity.Application.DTOs.ExerciseLibrary;
using Gymunity.Application.DTOs.Guest;
using Gymunity.Application.DTOs.Messaging;
using Gymunity.Application.DTOs.Notifications;
using Gymunity.Application.DTOs.Packages;
using Gymunity.Application.DTOs.Program;
using Gymunity.Application.DTOs.Program.ProgramDayDtos;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Application.DTOs.User.Payment;
using Gymunity.Application.DTOs.User.Subscriptions;
using Gymunity.Application.Mapping.Resolvers;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Entities.Messaging;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using ITI.Gymunity.FP.Application.DTOs.Client;
using System.Text.Json;

namespace Gymunity.Application.Mapping
{
    /// <summary>
    /// AutoMapper profile configuration for entity-DTO mappings across the application.
    /// Centralizes all object-to-object mapping rules to ensure consistency and maintainability.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the MappingProfile class and configures all mappings.
        /// </summary>
        public MappingProfile()
        {
            ConfigureClientMappings();
            ConfigureTrainerMappings();
            ConfigureProgramMappings();
            ConfigurePackageMappings();
            ConfigurePaymentAndSubscriptionMappings();
            ConfigureCommunicationMappings();
            ConfigureAdminAndGuestMappings();
        }

        #region Client Domain Mappings

        /// <summary>
        /// Configures mappings related to client entities (profiles, body stats, workout logs).
        /// Used by: ClientProfileController, BodyStateLogController, WorkoutLogController
        /// </summary>
        private void ConfigureClientMappings()
        {
            // Client Profile: Request ↔ Entity ↔ Response
            CreateMap<ClientProfileRequest, ClientProfile>()
                .ForMember(dest => dest.ExperienceLevel,
                    opt => opt.MapFrom(src => src.ExperienceLevel.ToString() ?? string.Empty))
                .ForMember(dest => dest.Goal,
                    opt => opt.MapFrom(src => src.Goal.ToString() ?? string.Empty))
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src => src.Gender.ToString()));

            CreateMap<ClientProfile, ClientProfileResponse>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.BodyStateLog,
                    opt => opt.MapFrom(src => src.BodyStatLogs
                        .OrderByDescending(b => b.LoggedAt)
                        .FirstOrDefault()))
                .ForMember(dest => dest.ExperienceLevel,
                    opt => opt.MapFrom(src => Enum.Parse<ExperienceLevel>(src.ExperienceLevel ?? string.Empty)))
                .ForMember(dest => dest.Goal,
                    opt => opt.MapFrom(src => Enum.Parse<ClientGoal>(src.Goal ?? string.Empty)))
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src => Enum.Parse<Gender>(src.Gender ?? string.Empty)));

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

            // Body State Log Mappings
            CreateMap<CreateBodyStateLogRequest, BodyStatLog>();
            CreateMap<BodyStatLog, BodyStateLogResponse>();

            // Workout Log Mappings
            CreateMap<WorkoutLogRequest, WorkoutLog>();
            CreateMap<WorkoutLog, WorkoutLogResponse>();

            // Trainer Card for Client View
            CreateMap<TrainerProfile, TrainerCardDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.ProfilePhotoUrl, opt => opt.MapFrom(src => src.User.ProfilePhotoUrl))
                .ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src => src.TrainerReviews.Count))
                .ForMember(dest => dest.Specializations, opt => opt.Ignore()) // To be populated if available
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => "EGP"))
                .ForMember(dest => dest.HasActiveSubscription, opt => opt.Ignore()); // Runtime population
        }

        #endregion

        #region Trainer Domain Mappings

        /// <summary>
        /// Configures mappings related to trainer entities (profiles, reviews).
        /// Used by: TrainerProfileController, ClientsController, ReviewsController
        /// </summary>
        private void ConfigureTrainerMappings()
        {
            // Trainer Profile: Multiple Response Types
            CreateMap<TrainerProfile, TrainerProfileListResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.CoverImageUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileListResponse>, string?>(src => src.CoverImageUrl))
                .ForMember(dest => dest.VideoIntroUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileListResponse>, string?>(src => src.VideoIntroUrl));

            CreateMap<TrainerProfile, TrainerProfileDetailResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.CoverImageUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileDetailResponse>, string?>(src => src.CoverImageUrl))
                .ForMember(dest => dest.VideoIntroUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileDetailResponse>, string?>(src => src.VideoIntroUrl))
                .ForMember(dest => dest.StatusImageUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileDetailResponse>, string?>(src => src.StatusImageUrl));

            CreateMap<TrainerProfile, TrainerProfileResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.CoverImageUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileResponse>, string?>(src => src.CoverImageUrl))
                .ForMember(dest => dest.VideoIntroUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileResponse>, string?>(src => src.VideoIntroUrl))
                .ForMember(dest => dest.StatusImageUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileResponse>, string?>(src => src.StatusImageUrl));

            CreateMap<TrainerProfile, TrainerProfileGetResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CoverImageUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileGetResponse>, string?>(src => src.CoverImageUrl))
                .ForMember(dest => dest.VideoIntroUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileGetResponse>, string?>(src => src.VideoIntroUrl))
                .ForMember(dest => dest.StatusImageUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerProfileGetResponse>, string?>(src => src.StatusImageUrl));

            // Trainer Profile: Request to Entity
            CreateMap<CreateTrainerProfileRequest, TrainerProfile>()
                .ForMember(dest => dest.CoverImageUrl, opt => opt.Ignore()) // Handled by service
                .ForMember(dest => dest.StatusImageUrl, opt => opt.Ignore()) // Handled by service
                .ForMember(dest => dest.StatusDescription, opt => opt.Ignore()); // Handled by service

            CreateMap<UpdateTrainerProfileRequest, TrainerProfile>()
                .ForMember(dest => dest.CoverImageUrl, opt => opt.Ignore()) // Handled by service
                .ForMember(dest => dest.StatusImageUrl, opt => opt.Ignore()) // Handled by service
                .ForMember(dest => dest.StatusDescription, opt => opt.Ignore()) // Handled by service
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Partial updates

            // Trainer for Client View
            CreateMap<TrainerProfile, DTOs.Client.TrainerClientResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Handle, opt => opt.MapFrom(src => src.Handle))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
                .ForMember(dest => dest.CoverImageUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, DTOs.Client.TrainerClientResponse>, string?>(src => src.CoverImageUrl))
                .ForMember(dest => dest.RatingAverage, opt => opt.MapFrom(src => src.RatingAverage))
                .ForMember(dest => dest.TotalClients, opt => opt.MapFrom(src => src.TotalClients))
                .ForMember(dest => dest.YearsExperience, opt => opt.MapFrom(src => src.YearsExperience));

            // Trainer Brief Information
            CreateMap<TrainerProfile, TrainerBriefResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.ProfilePhotoUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<TrainerProfile, TrainerBriefResponse>, string?>(src => src.User.ProfilePhotoUrl))
                .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Handle, opt => opt.MapFrom(src => src.Handle));
        }

        #endregion

        #region Program Domain Mappings

        /// <summary>
        /// Configures mappings for program entities and their hierarchical components.
        /// Used by: ProgramsController, WeeksController, DaysController, DayExercisesController
        /// </summary>
        private void ConfigureProgramMappings()
        {
            // Program Mappings (Multiple Response Types)
            CreateMap<Program, ProgramGetAllResponse>()
                .ForMember(dest => dest.TrainerUserName,
                    opt => opt.MapFrom(src => src.TrainerProfile != null ? src.TrainerProfile.User.UserName : null))
                .ForMember(dest => dest.TrainerHandle,
                    opt => opt.MapFrom(src => src.TrainerProfile != null ? src.TrainerProfile.Handle : null))
                .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(src => src.TrainerProfileId))
                .ForMember(dest => dest.ThumbnailUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<Program, ProgramGetAllResponse>, string?>(src => src.ThumbnailUrl));

            CreateMap<Program, ProgramGetByIdResponse>()
                .ForMember(dest => dest.TrainerUserName,
                    opt => opt.MapFrom(src => src.TrainerProfile != null ? src.TrainerProfile.User.UserName : null))
                .ForMember(dest => dest.TrainerHandle,
                    opt => opt.MapFrom(src => src.TrainerProfile != null ? src.TrainerProfile.Handle : null))
                .ForMember(dest => dest.TrainerEmail,
                    opt => opt.MapFrom(src => src.TrainerProfile != null ? src.TrainerProfile.User.Email : null))
                .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(src => src.TrainerProfileId))
                .ForMember(dest => dest.ThumbnailUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<Program, ProgramGetByIdResponse>, string?>(src => src.ThumbnailUrl));

            CreateMap<Program, ProgramResponse>()
                .ForMember(dest => dest.TrainerUserName,
                    opt => opt.MapFrom(src => src.TrainerProfile != null ? src.TrainerProfile.User.UserName : null))
                .ForMember(dest => dest.TrainerHandle,
                    opt => opt.MapFrom(src => src.TrainerProfile != null ? src.TrainerProfile.Handle : null))
                .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(src => src.TrainerProfileId))
                .ForMember(dest => dest.ThumbnailUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<Program, ProgramResponse>, string?>(src => src.ThumbnailUrl));

            CreateMap<Program, ProgramClientResponse>()
                .ForMember(dest => dest.TrainerUserName,
                    opt => opt.MapFrom(src => src.TrainerProfile != null ? src.TrainerProfile.User.UserName : null))
                .ForMember(dest => dest.TrainerHandle,
                    opt => opt.MapFrom(src => src.TrainerProfile != null ? src.TrainerProfile.Handle : null))
                .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(src => src.TrainerProfileId))
                .ForMember(dest => dest.ThumbnailUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<Program, ProgramClientResponse>, string?>(src => src.ThumbnailUrl));

            // Week Mappings
            CreateMap<ProgramWeek, ProgramWeekResponse>();
            CreateMap<ProgramWeek, ProgramWeekGetAllResponse>();

            // Day Mappings
            CreateMap<ProgramDay, ProgramDayResponse>()
                .ForMember(dest => dest.Exercises, opt => opt.Ignore()); // Populated separately
            CreateMap<ProgramDay, ProgramDayGetAllResponse>();

            // Day Exercise Mappings
            CreateMap<ProgramDayExercise, ProgramDayExerciseResponse>()
                .ForMember(dest => dest.VideoUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<ProgramDayExercise, ProgramDayExerciseResponse>, string?>(src => src.VideoUrl))
                .ForMember(dest => dest.ThumbnailUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<ProgramDayExercise, ProgramDayExerciseResponse>, string?>(src =>
                        src.Exercise != null ? src.Exercise.ThumbnailUrl : null))
                .ForMember(dest => dest.ExcersiceName,
                    opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Name : string.Empty))
                .ForMember(dest => dest.Category,
                    opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Category : string.Empty))
                .ForMember(dest => dest.MuscleGroup,
                    opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.MuscleGroup : string.Empty))
                .ForMember(dest => dest.Equipment,
                    opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Equipment : null))
                .ForMember(dest => dest.TrainerId,
                    opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.TrainerId : null));

            CreateMap<ProgramDayExercise, ProgramDayExerciseGetAllResponse>()
                .ForMember(dest => dest.VideoUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<ProgramDayExercise, ProgramDayExerciseGetAllResponse>, string?>(src => src.VideoUrl));
        }

        #endregion

        #region Package Domain Mappings

        /// <summary>
        /// Configures mappings for package entities (subscription bundles).
        /// Used by: PackagesController, SubscriptionsController, Client-facing services
        /// </summary>
        private void ConfigurePackageMappings()
        {
            // Package to Response Mappings
            CreateMap<Package, PackageResponse>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => new DateTimeOffset(src.CreatedAt)))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.PromoCode, opt => opt.MapFrom(src => src.PromoCode))
                .ForMember(dest => dest.ProgramIds,
                    opt => opt.MapFrom(src => src.PackagePrograms != null
                        ? src.PackagePrograms.Where(pp => !pp.IsDeleted).Select(pp => pp.ProgramId).ToArray()
                        : Array.Empty<int>()))
                .ForMember(dest => dest.ThumbnailUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<Package, PackageResponse>, string?>(src => src.ThumbnailUrl));

            CreateMap<Package, DTOs.Client.PackageClientResponse>()
                .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => new DateTimeOffset(src.CreatedAt)))
                .ForMember(dest => dest.PromoCode, opt => opt.MapFrom(src => src.PromoCode))
                .ForMember(dest => dest.ThumbnailUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<Package, DTOs.Client.PackageClientResponse>, string?>(src => src.ThumbnailUrl));

            // Package Request to Entity Mappings
            CreateMap<PackageCreateRequest, Package>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Trainer, opt => opt.Ignore())
                .ForMember(dest => dest.PackagePrograms, opt => opt.Ignore())
                .ForMember(dest => dest.Subscriptions, opt => opt.Ignore());

            CreateMap<PackageUpdateRequest, Package>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Partial updates
        }

        #endregion

        #region Payment and Subscription Mappings

        /// <summary>
        /// Configures mappings for payment and subscription entities.
        /// Used by: PaymentsController, SubscriptionsController, Admin reporting
        /// </summary>
        private void ConfigurePaymentAndSubscriptionMappings()
        {
            // Subscription Mappings
            CreateMap<Subscription, SubscriptionResponse>()
                .ForMember(dest => dest.PackageId, opt => opt.MapFrom(src => src.PackageId))
                .ForMember(dest => dest.PackageName,
                    opt => opt.MapFrom(src => src.Package != null ? src.Package.Name : string.Empty))
                .ForMember(dest => dest.PackageDescription,
                    opt => opt.MapFrom(src => src.Package != null ? src.Package.Description : string.Empty))
                .ForMember(dest => dest.TrainerId,
                    opt => opt.MapFrom(src => src.Package != null && src.Package.Trainer != null
                        ? src.Package.Trainer.UserId
                        : string.Empty))
                .ForMember(dest => dest.TrainerProfileId,
                    opt => opt.MapFrom(src => src.Package != null && src.Package.Trainer != null
                        ? src.Package.Trainer.Id
                        : 0))
                .ForMember(dest => dest.TrainerName,
                    opt => opt.MapFrom(src => src.Package != null && src.Package.Trainer != null && src.Package.Trainer.User != null
                        ? src.Package.Trainer.User.FullName
                        : string.Empty))
                .ForMember(dest => dest.TrainerHandle,
                    opt => opt.MapFrom(src => src.Package != null && src.Package.Trainer != null
                        ? src.Package.Trainer.Handle
                        : string.Empty))
                .ForMember(dest => dest.TrainerPhotoUrl,
                    opt => opt.MapFrom(src => src.Package != null && src.Package.Trainer != null && src.Package.Trainer.User != null
                        ? src.Package.Trainer.User.ProfilePhotoUrl
                        : null))
                .ForMember(dest => dest.FeaturesIncluded,
                    opt => opt.MapFrom(src => src.Package != null
                        ? ParseFeatures(src.Package.FeaturesJson)
                        : new List<string>()));

            // Payment Mappings
            CreateMap<Payment, PaymentResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId))
                // Client Information
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))
                .ForMember(dest => dest.ClientName,
                    opt => opt.MapFrom(src => src.Subscription != null && src.Subscription.Client != null
                        ? src.Subscription.Client.FullName
                        : string.Empty))
                .ForMember(dest => dest.ClientEmail,
                    opt => opt.MapFrom(src => src.Subscription != null && src.Subscription.Client != null
                        ? src.Subscription.Client.Email ?? string.Empty
                        : string.Empty))
                // Subscription & Package Information
                .ForMember(dest => dest.PackageId,
                    opt => opt.MapFrom(src => src.Subscription != null && src.Subscription.Package != null
                        ? src.Subscription.Package.Id
                        : 0))
                .ForMember(dest => dest.PackageName,
                    opt => opt.MapFrom(src => src.Subscription != null && src.Subscription.Package != null
                        ? src.Subscription.Package.Name
                        : string.Empty))
                .ForMember(dest => dest.SubscriptionStatus,
                    opt => opt.MapFrom(src => src.Subscription != null
                        ? (SubscriptionStatus?)src.Subscription.Status
                        : null))
                .ForMember(dest => dest.SubscriptionStartDate,
                    opt => opt.MapFrom(src => src.Subscription != null
                        ? (DateTime?)src.Subscription.StartDate
                        : null))
                .ForMember(dest => dest.SubscriptionEndDate,
                    opt => opt.MapFrom(src => src.Subscription != null
                        ? (DateTime?)src.Subscription.CurrentPeriodEnd
                        : null))
                .ForMember(dest => dest.IsAnnualSubscription,
                    opt => opt.MapFrom(src => src.Subscription != null
                        ? src.Subscription.IsAnnual
                        : false))
                // Trainer Information
                .ForMember(dest => dest.TrainerProfileId,
                    opt => opt.MapFrom(src => src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null
                        ? src.Subscription.Package.Trainer.Id
                        : 0))
                .ForMember(dest => dest.TrainerName,
                    opt => opt.MapFrom(src => src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null && src.Subscription.Package.Trainer.User != null
                        ? src.Subscription.Package.Trainer.User.FullName
                        : string.Empty))
                .ForMember(dest => dest.TrainerHandle,
                    opt => opt.MapFrom(src => src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null
                        ? src.Subscription.Package.Trainer.Handle
                        : null))
                .ForMember(dest => dest.IsTrainerVerified,
                    opt => opt.MapFrom(src => src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null
                        ? src.Subscription.Package.Trainer.IsVerified
                        : false))
                .ForMember(dest => dest.TrainerRating,
                    opt => opt.MapFrom(src => src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null
                        ? (decimal?)src.Subscription.Package.Trainer.RatingAverage
                        : null))
                .ForMember(dest => dest.TrainerTotalClients,
                    opt => opt.MapFrom(src => src.Subscription != null && src.Subscription.Package != null && src.Subscription.Package.Trainer != null
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
                .ForMember(dest => dest.TransactionId,
                    opt => opt.MapFrom(src => src.Method == PaymentMethod.Paymob
                        ? src.PaymobTransactionId
                        : src.PayPalCaptureId))
                .ForMember(dest => dest.FailureReason, opt => opt.MapFrom(src => src.FailureReason))
                // Payment URLs
                .ForMember(dest => dest.PaymentUrl, opt => opt.Ignore()) // Generated at runtime
                .ForMember(dest => dest.PaymobOrderId, opt => opt.MapFrom(src => src.PaymobOrderId))
                .ForMember(dest => dest.PayPalOrderId, opt => opt.MapFrom(src => src.PayPalOrderId))
                // Dates
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(src => src.PaidAt))
                .ForMember(dest => dest.FailedAt, opt => opt.MapFrom(src => src.FailedAt));
        }

        #endregion

        #region Exercise Library Mappings

        /// <summary>
        /// Configures mappings for exercise library entities.
        /// Used by: ExerciseLibraryController, DayExercisesController
        /// </summary>
        private void ConfigureExerciseLibraryMappings()
        {
            CreateMap<Exercise, ExerciseGetAllResponse>()
                .ForMember(dest => dest.VideoDemoUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<Exercise, ExerciseGetAllResponse>, string?>(src => src.VideoDemoUrl))
                .ForMember(dest => dest.ThumbnailUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<Exercise, ExerciseGetAllResponse>, string?>(src => src.ThumbnailUrl));

            CreateMap<Exercise, ExerciseGetByIdResponse>()
                .ForMember(dest => dest.VideoDemoUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<Exercise, ExerciseGetByIdResponse>, string?>(src => src.VideoDemoUrl))
                .ForMember(dest => dest.ThumbnailUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<Exercise, ExerciseGetByIdResponse>, string?>(src => src.ThumbnailUrl));

            CreateMap<ExerciseCreateRequest, Exercise>();
            CreateMap<ExerciseUpdateRequest, Exercise>();
        }

        #endregion

        #region Communication Mappings

        /// <summary>
        /// Configures mappings for messaging and notification entities.
        /// Used by: ChatController, NotificationsController
        /// </summary>
        private void ConfigureCommunicationMappings()
        {
            // Message Mappings
            CreateMap<Message, MessageResponse>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.FullName))
                .ForMember(dest => dest.SenderProfilePhoto, opt => opt.MapFrom(src => src.Sender.ProfilePhotoUrl));

            // Notification Mappings
            CreateMap<Notification, NotificationResponse>();
        }

        #endregion

        #region Admin and Guest Mappings

        /// <summary>
        /// Configures mappings for admin operations and guest/public views.
        /// Used by: Admin controllers, Guest/Public endpoints
        /// </summary>
        private void ConfigureAdminAndGuestMappings()
        {
            // User/Client Mappings for Admin
            CreateMap<AppUser, ClientGetAllResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.ProfilePhotoUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<AppUser, ClientGetAllResponse>, string?>(src => src.ProfilePhotoUrl));

            CreateMap<AppUser, ClientGetByIdResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.ProfilePhotoUrl,
                    opt => opt.MapFrom<GenericImageUrlResolver<AppUser, ClientGetByIdResponse>, string?>(src => src.ProfilePhotoUrl));

            // Review Mappings
            CreateMap<TrainerReview, TrainerAreaReviewResponse>();
            CreateMap<TrainerReview, TrainerReviewClientResponse>();
            CreateMap<TrainerReview, TrainerReviewResponse>();
            CreateMap<TrainerReview, GuestReviewResponseItem>()
                .ForMember(dest => dest.ClientUserName,
                    opt => opt.MapFrom(src => src.Client != null ? src.Client.User.UserName : string.Empty));

            CreateMap<TrainerReview, AdminReviewActionResponse>()
                .ForMember(dest => dest.Message, opt => opt.Ignore()); // Populated at runtime

            // Top Trainer for Public View
            CreateMap<TrainerProfile, TopTrainerResponse>()
                .ForMember(dest => dest.TrainerProfileId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Handle, opt => opt.MapFrom(src => src.Handle))
                .ForMember(dest => dest.TotalClients, opt => opt.MapFrom(src => src.TotalClients));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Parses JSON features string from package into a readable list of feature descriptions.
        /// </summary>
        /// <param name="featuresJson">JSON string containing package features</param>
        /// <returns>List of human-readable feature descriptions</returns>
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

                // Extract and format common features
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
                // Return empty list on parsing error to avoid breaking the response
                return new List<string>();
            }
        }

        #endregion
    }
}