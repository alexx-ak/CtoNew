namespace VoxBox.Api.Interfaces;

public interface IEntityMapper<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : class
{
    TDto MapToDto(TEntity entity);
    TEntity MapToEntity(TCreateDto createDto);
    void MapToEntity(TUpdateDto updateDto, TEntity entity);
}
