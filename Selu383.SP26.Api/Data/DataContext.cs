using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.User;

namespace Selu383.SP26.Api.Data;

public class DataContext : IdentityDbContext<User, Role, int,IdentityUserClaim<int>, 
    UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {

    }

    public DbSet<Location> Locations { get; set; } = null!;
    // optional DbSet for the join entity to make queries easier in tests/services
    public DbSet<UserRole> UserRoles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var userRoleBuilder = builder.Entity<UserRole>();

        userRoleBuilder.HasKey(x => new { x.UserId, x.RoleId });

        userRoleBuilder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId);
        userRoleBuilder.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId);

        // apply other IEntityTypeConfiguration<> implementations in this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
