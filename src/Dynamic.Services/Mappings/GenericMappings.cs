using AutoMapper;
using Dynamic.DbScaffolder;
using System.Linq;

namespace Dynamic.Services.Mappings
{
    internal class GenericMappings : Profile
    {
        public GenericMappings()
        {
            var entities = ScaffolderHelper.GetScaffoldedDbContextEntityTypes();
            var dtos = ScaffolderHelper.GetDtosTypes();

            foreach (var entity in entities)
            {
                var dtoType = dtos.Single(x => x.Name == $"{entity.Name}Dto");
                var flatDtoType = dtos.Single(x => x.Name == $"{entity.Name}FlatDto");
                var editDtoType = dtos.Single(x => x.Name == $"{entity.Name}EditDto");

                CreateMap(entity, dtoType).ReverseMap();
                CreateMap(entity, flatDtoType).ReverseMap();
                CreateMap(entity, editDtoType).ReverseMap();
            }
        }
    }
}
