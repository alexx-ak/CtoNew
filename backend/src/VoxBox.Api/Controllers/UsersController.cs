using Microsoft.AspNetCore.Mvc;
using VoxBox.Api.DTOs;
using VoxBox.Api.Mappers;
using VoxBox.Api.Models;
using VoxBox.Core.Entities;
using VoxBox.Infrastructure.Persistence;

namespace VoxBox.Api.Controllers;

public class UsersController(VoxBoxDbContext dbContext, UserMapper mapper)
    : BaseControllerLong<User, UserDto, CreateUserDto, UpdateUserDto>(dbContext, mapper)
{
    protected override void BeforeCreate(User entity)
    {
        entity.UserName = entity.UserName.ToLower();
    }

    protected override void BeforeUpdate(User entity)
    {
        entity.UserName = entity.UserName.ToLower();
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetActiveUsers(CancellationToken cancellationToken)
    {
        var repository = DbContext.GetRepositoryLong<User>();
        var users = await repository.GetAllAsync(includeDeleted: false, cancellationToken);
        var activeUsers = users.Where(u => u.IsActive);
        var dtos = activeUsers.Select(u => Mapper.MapToDto(u));
        return Ok(dtos);
    }

    protected override IQueryable<User> ApplyCustomFilters(IQueryable<User> query, PagedRequest request)
    {
        return query;
    }
}
