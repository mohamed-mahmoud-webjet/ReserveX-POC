using AutoMapper;
using ToursApi.Models;
using ToursApi.ServiceLayer.Models;

namespace ToursApi.Mapping
{
    public class AutoCompleteMappingProfile : Profile
    {
        public AutoCompleteMappingProfile()
        {
            CreateMap<AutoCompleteDto, AutoCompleteResult>();
        }
    }
}
