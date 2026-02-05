using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Features.Locations;

namespace Selu383.SP26.Api.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }

    public DbSet<Location> Locations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // find all the "IEntityTypeConfiguration<TEntity>" implementations in this assembly and apply them
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
