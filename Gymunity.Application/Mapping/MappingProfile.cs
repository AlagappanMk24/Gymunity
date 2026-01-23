using AutoMapper;
using Gymunity.Application.DTOs.ClientDto;
using Gymunity.Application.DTOs.Packages;
using Gymunity.Application.DTOs.Program;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Application.Mapping.Resolvers;
using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using ITI.Gymunity.FP.Application.DTOs.Client;

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
        }
    }
}