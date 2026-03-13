using System.Linq.Expressions;
using FamilyTree.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FamilyTree.Database;

public class FamilyTreeContext : DbContext
{
    public FamilyTreeContext(DbContextOptions<FamilyTreeContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Family> Families => Set<Family>();

    public DbSet<Relationship> Relationships => Set<Relationship>();




    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        DateTime utcNow = DateTime.UtcNow;

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Property(x => x.DateCreated).CurrentValue == default)
                {
                    entry.Property(x => x.DateCreated).CurrentValue = utcNow;
                }

                entry.Property(x => x.DateUpdated).CurrentValue = utcNow;
                entry.Property(x => x.Deleted).CurrentValue = false;
                entry.Property(x => x.Archived).CurrentValue = false;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.DateUpdated).CurrentValue = utcNow;
                entry.Property(x => x.DateCreated).IsModified = false;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasOne(x => x.Family)
                .WithMany(x => x.Persons)
                .HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Family>(entity =>
        {
            entity.HasIndex(x => x.Slug)
                .IsUnique()
                .HasFilter("\"Deleted\" = false");
        });

        modelBuilder.Entity<Relationship>(entity =>
        {
            entity.HasOne(x => x.FromPerson)
                .WithMany()
                .HasForeignKey(x => x.FromPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ToPerson)
                .WithMany()
                .HasForeignKey(x => x.ToPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.FromPersonId, x.ToPersonId, x.RelationshipType })
                .IsUnique()
                .HasFilter("\"Deleted\" = false");
        });

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "entity");
                MemberExpression deletedProperty = Expression.Property(parameter, nameof(BaseEntity.Deleted));
                BinaryExpression compareExpression = Expression.Equal(deletedProperty, Expression.Constant(false));
                LambdaExpression lambda = Expression.Lambda(compareExpression, parameter);

                entityType.SetQueryFilter(lambda);
            }
        }
    }
}
