using AutoMapper;

namespace QueueTriggerCosmosOutput;

public class DataItemAutoMappingProfile : Profile
{
    public DataItemAutoMappingProfile()
    {
        CreateMap<Data, Item>()
            .ForMember(dest => dest.Identity, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Value));
    }
}

