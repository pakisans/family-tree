using FamilyTree.Converters.Core;
using FamilyTree.Dto;
using FamilyTree.Entity;

namespace FamilyTree.Converters;

public class PersonConverter : BaseConverter<Person, PersonDto>, IPersonConverter
{
    public override PersonDto MapToDto(Person entity)
    {
        return new PersonDto
        {
            Id = entity.Id,
            DateCreated = entity.DateCreated,
            OwnerId = entity.OwnerId,
            ItemOrder = entity.ItemOrder,
            Archived = entity.Archived,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            BirthDate = entity.BirthDate,
            DeathDate = entity.DeathDate,
            Gender = entity.Gender,
            BirthPlace = entity.BirthPlace,
            DeathPlace = entity.DeathPlace,
            Biography = entity.Biography,
            IsPublic = entity.IsPublic
        };
    }

    public override Person MapToEntity(PersonDto dto)
    {
        return new Person
        {
            Id = dto.Id,
            OwnerId = dto.OwnerId,
            ItemOrder = dto.ItemOrder,
            Archived = dto.Archived,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            BirthDate = dto.BirthDate,
            DeathDate = dto.DeathDate,
            Gender = dto.Gender,
            BirthPlace = dto.BirthPlace,
            DeathPlace = dto.DeathPlace,
            Biography = dto.Biography,
            IsPublic = dto.IsPublic
        };
    }
}
