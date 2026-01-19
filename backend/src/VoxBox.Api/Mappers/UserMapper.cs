using VoxBox.Api.DTOs;
using VoxBox.Api.Interfaces;
using VoxBox.Core.Entities;

namespace VoxBox.Api.Mappers;

public class UserMapper : IEntityMapper<User, UserDto, CreateUserDto, UpdateUserDto>
{
    public UserDto MapToDto(User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            UserName = entity.UserName,
            Name = entity.Name,
            Surname = entity.Surname,
            EmailAddress = entity.EmailAddress,
            PhoneNumber = entity.PhoneNumber,
            IsActive = entity.IsActive,
            Identifier = entity.Identifier,
            VoteWeight = entity.VoteWeight,
            IdentyumUuid = entity.IdentyumUuid,
            PreviousName = entity.PreviousName,
            PreviousSurname = entity.PreviousSurname,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            ModifiedBy = entity.ModifiedBy
        };
    }

    public User MapToEntity(CreateUserDto createDto)
    {
        return new User
        {
            UserName = createDto.UserName,
            Name = createDto.Name,
            Surname = createDto.Surname,
            EmailAddress = createDto.EmailAddress,
            PhoneNumber = createDto.PhoneNumber,
            IsActive = createDto.IsActive,
            Identifier = createDto.Identifier,
            VoteWeight = createDto.VoteWeight,
            IdentyumUuid = createDto.IdentyumUuid
        };
    }

    public void MapToEntity(UpdateUserDto updateDto, User entity)
    {
        entity.UserName = updateDto.UserName;
        entity.Name = updateDto.Name;
        entity.Surname = updateDto.Surname;
        entity.EmailAddress = updateDto.EmailAddress;
        entity.PhoneNumber = updateDto.PhoneNumber;
        entity.IsActive = updateDto.IsActive;
        entity.Identifier = updateDto.Identifier;
        entity.VoteWeight = updateDto.VoteWeight;
        entity.IdentyumUuid = updateDto.IdentyumUuid;
        entity.PreviousName = updateDto.PreviousName;
        entity.PreviousSurname = updateDto.PreviousSurname;
    }
}
