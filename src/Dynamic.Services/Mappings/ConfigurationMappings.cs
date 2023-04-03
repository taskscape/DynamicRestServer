using AutoMapper;
using Dynamic.DAL.Entities;
using Dynamic.Services.Dto;

namespace Dynamic.Services.Mappings
{
    internal class ConfigurationMappings : Profile
    {
        public ConfigurationMappings()
        {
            CreateMap<Configuration, ConfigurationDto>().ReverseMap();
        }
    }
}
