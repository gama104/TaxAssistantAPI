using AutoMapper;
using IRSAssistantAPI.Domain.Entities;
using IRSAssistantAPI.Application.DTOs;

namespace IRSAssistantAPI.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Taxpayer mappings
        CreateMap<Taxpayer, TaxpayerDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt.HasValue ? src.LastLoginAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null));

        // ChatSession mappings
        CreateMap<ChatSession, ChatSessionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.TaxpayerId, opt => opt.MapFrom(src => src.TaxpayerId.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.LastActivityAt, opt => opt.MapFrom(src => src.LastActivityAt.HasValue ? src.LastActivityAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null));

        // ChatMessage mappings
        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ChatSessionId, opt => opt.MapFrom(src => src.ChatSessionId.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")));

        // TaxReturn mappings
        CreateMap<TaxReturn, TaxReturnDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.TaxpayerId, opt => opt.MapFrom(src => src.TaxpayerId.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null));

        // IncomeSource mappings
        CreateMap<IncomeSource, IncomeSourceDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ReturnId, opt => opt.MapFrom(src => src.ReturnId.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null));

        // Property mappings
        CreateMap<Property, PropertyDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.TaxpayerId, opt => opt.MapFrom(src => src.TaxpayerId.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null));

        // Asset mappings
        CreateMap<Asset, AssetDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.TaxpayerId, opt => opt.MapFrom(src => src.TaxpayerId.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null));

        // Dependent mappings
        CreateMap<Dependent, DependentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.TaxpayerId, opt => opt.MapFrom(src => src.TaxpayerId.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null));

        // TaxDocument mappings
        CreateMap<TaxDocument, TaxDocumentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.TaxpayerId, opt => opt.MapFrom(src => src.TaxpayerId.ToString()))
            .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => src.UploadedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.ProcessedAt, opt => opt.MapFrom(src => src.ProcessedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")));
    }
}