using Microsoft.EntityFrameworkCore;
using VoxBox.Core.Common;
using VoxBox.Core.Entities;
using VoxBox.Core.Interfaces.Persistence;

namespace VoxBox.Infrastructure.Persistence;

/// <summary>
/// Application DbContext following SOLID and Clean Architecture:
/// - Infrastructure depends on Core (abstractions)
/// - KISS: Simple, focused DbContext
/// </summary>
public class VoxBoxDbContext : DbContext, IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = new();
    
    public VoxBoxDbContext(DbContextOptions<VoxBoxDbContext> options) : base(options)
    {
    }
    
    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();
    
    public IRepository<T> GetRepository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new EfRepository<T>(Set<T>());
            _repositories[type] = repo;
        }
        return (IRepository<T>)repo;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<SampleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });
    }
}
