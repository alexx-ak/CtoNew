using Microsoft.AspNetCore.Mvc;
using VoxBox.Api.Extensions;
using VoxBox.Api.Interfaces;
using VoxBox.Api.Models;
using VoxBox.Core.Common;
using VoxBox.Infrastructure.Persistence;

namespace VoxBox.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseControllerLong<TEntity, TDto, TCreateDto, TUpdateDto>(
    VoxBoxDbContext dbContext,
    IEntityMapper<TEntity, TDto, TCreateDto, TUpdateDto> mapper) : ControllerBase
    where TEntity : BaseEntityLong
{
    protected readonly VoxBoxDbContext DbContext = dbContext;
    protected readonly IEntityMapper<TEntity, TDto, TCreateDto, TUpdateDto> Mapper = mapper;

    [HttpGet]
    public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAll(CancellationToken cancellationToken)
    {
        var repository = DbContext.GetRepositoryLong<TEntity>();
        var entities = await repository.GetAllAsync(includeDeleted: false, cancellationToken);
        var dtos = entities.Select(e => Mapper.MapToDto(e));
        return Ok(dtos);
    }

    [HttpGet("paged")]
    public virtual async Task<ActionResult<PagedResponse<TDto>>> GetPaged(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var repository = DbContext.GetRepositoryLong<TEntity>();
        var query = (await repository.GetAllAsync(includeDeleted: false, cancellationToken)).AsQueryable();

        query = ApplyCustomFilters(query, request);
        query = query.ApplyFilters(request.Filter);
        query = query.ApplySorting(request.SortBy, request.SortDirection);

        var totalCount = query.Count();
        var entities = query.Skip(request.Skip).Take(request.Take).ToList();
        var dtos = entities.Select(e => Mapper.MapToDto(e));

        var response = new PagedResponse<TDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public virtual async Task<ActionResult<TDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var repository = DbContext.GetRepositoryLong<TEntity>();
        var entity = await repository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        
        if (entity == null)
        {
            return NotFound(new { message = $"{typeof(TEntity).Name} with ID {id} not found" });
        }

        var dto = Mapper.MapToDto(entity);
        return Ok(dto);
    }

    [HttpPost]
    public virtual async Task<ActionResult<TDto>> Create([FromBody] TCreateDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var entity = Mapper.MapToEntity(createDto);
        BeforeCreate(entity);

        var repository = DbContext.GetRepositoryLong<TEntity>();
        var createdEntity = await repository.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        AfterCreate(createdEntity);

        var dto = Mapper.MapToDto(createdEntity);
        return CreatedAtAction(nameof(GetById), new { id = createdEntity.Id }, dto);
    }

    [HttpPut("{id}")]
    public virtual async Task<ActionResult<TDto>> Update(Guid id, [FromBody] TUpdateDto updateDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var repository = DbContext.GetRepositoryLong<TEntity>();
        var existingEntity = await repository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        
        if (existingEntity == null)
        {
            return NotFound(new { message = $"{typeof(TEntity).Name} with ID {id} not found" });
        }

        Mapper.MapToEntity(updateDto, existingEntity);
        BeforeUpdate(existingEntity);

        await repository.UpdateAsync(existingEntity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        AfterUpdate(existingEntity);

        var dto = Mapper.MapToDto(existingEntity);
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    public virtual async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var repository = DbContext.GetRepositoryLong<TEntity>();
        var existingEntity = await repository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        
        if (existingEntity == null)
        {
            return NotFound(new { message = $"{typeof(TEntity).Name} with ID {id} not found" });
        }

        BeforeDelete(existingEntity);

        await repository.DeleteAsync(id, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        AfterDelete(existingEntity);

        return NoContent();
    }

    protected virtual IQueryable<TEntity> ApplyCustomFilters(IQueryable<TEntity> query, PagedRequest request)
    {
        return query;
    }

    protected virtual void BeforeCreate(TEntity entity) { }
    protected virtual void AfterCreate(TEntity entity) { }
    protected virtual void BeforeUpdate(TEntity entity) { }
    protected virtual void AfterUpdate(TEntity entity) { }
    protected virtual void BeforeDelete(TEntity entity) { }
    protected virtual void AfterDelete(TEntity entity) { }
}
