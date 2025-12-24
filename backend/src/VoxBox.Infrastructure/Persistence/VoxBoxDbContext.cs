using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using VoxBox.Core.Common;
using VoxBox.Core.Entities;
using VoxBox.Core.Interfaces.Persistence;

namespace VoxBox.Infrastructure.Persistence;

/// <summary>
/// Application DbContext following SOLID and Clean Architecture:
/// - Infrastructure depends on Core (abstractions)
/// - KISS: Simple, focused DbContext
/// - Implements global query filters for soft delete and tenant isolation
/// </summary>
public class VoxBoxDbContext : DbContext, IVoxBoxDbContextFactory
{
    private readonly ITenantContext _tenantContext;
    private readonly Dictionary<Type, object> _repositories = new();

    public VoxBoxDbContext(DbContextOptions<VoxBoxDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();

    public IVoxBoxDbContextFactory CreateDbContextFactory() => this;

    public VoxBoxDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<VoxBoxDbContext>();
        optionsBuilder.UseSqlServer(Database.GetDbConnection().ConnectionString);
        return new VoxBoxDbContext(optionsBuilder.Options, _tenantContext);
    }

    public IRepository<T> GetRepository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new EfRepository<T>(Set<T>(), _tenantContext);
            _repositories[type] = repo;
        }
        return (IRepository<T>)repo;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SampleEntity (placeholder until first table is created)
        modelBuilder.Entity<SampleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Apply global query filters to all entities inheriting from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
        {
            // Skip Tenant entity from query filters as it needs to be accessed globally
            if (entityType.ClrType == typeof(Tenant))
                continue;

            // Soft delete filter
            entityType.AddQueryFilter($"{nameof(BaseEntity.IsDeleted)} == false");

            // Tenant isolation filter - only apply if tenant context has a value
            entityType.AddQueryFilter((BaseEntity e) =>
                EF.Property<Guid?>(e, nameof(BaseEntity.TenantId)) == _tenantContext.TenantId ||
                _tenantContext.TenantId == null);
        }
    }

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.TenantId = _tenantContext.TenantId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = "System"; // Will be replaced with actual user when auth is implemented
                    break;

                case EntityState.Deleted:
                    // Intercept delete operations to implement soft delete
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedBy = "System"; // Will be replaced with actual user when auth is implemented
                    entry.State = EntityState.Modified;
                    break;
            }
        }
    }
}
