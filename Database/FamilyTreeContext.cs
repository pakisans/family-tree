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
    public DbSet<Union> Unions => Set<Union>();

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<FamilyAccess> FamilyAccesses => Set<FamilyAccess>();
    public DbSet<FamilyInvitation> FamilyInvitations => Set<FamilyInvitation>();




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

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasOne(x => x.Family)
                .WithMany(x => x.Persons)
                .HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => x.FamilyId);
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

            entity.HasIndex(x => new { x.FromPersonId, x.RelationshipType });
            entity.HasIndex(x => new { x.ToPersonId, x.RelationshipType });
        });

        modelBuilder.Entity<Union>(entity =>
        {
            entity.HasOne(x => x.Person1)
                .WithMany()
                .HasForeignKey(x => x.Person1Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Person2)
                .WithMany()
                .HasForeignKey(x => x.Person2Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.Person1Id, x.Person2Id, x.IsActive })
                .HasFilter("\"Deleted\" = false");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email)
                .IsUnique()
                .HasFilter("\"Deleted\" = false");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(x => x.Name)
                .IsUnique()
                .HasFilter("\"Deleted\" = false");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.UserId, x.RoleId })
                .IsUnique()
                .HasFilter("\"Deleted\" = false");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.Token)
                .IsUnique()
                .HasFilter("\"Deleted\" = false");
        });

        modelBuilder.Entity<FamilyAccess>(entity =>
        {
            entity.HasOne(x => x.Family)
                .WithMany(x => x.FamilyAccesses)
                .HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany(x => x.FamilyAccesses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.InvitedByUser)
                .WithMany()
                .HasForeignKey(x => x.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.FamilyId, x.UserId })
                .IsUnique()
                .HasFilter("\"Deleted\" = false");
        });

        modelBuilder.Entity<FamilyInvitation>(entity =>
        {
            entity.HasOne(x => x.Family)
                .WithMany(x => x.Invitations)
                .HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.InvitedByUser)
                .WithMany()
                .HasForeignKey(x => x.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.Token)
                .IsUnique()
                .HasFilter("\"Deleted\" = false");
        });
    }
}
