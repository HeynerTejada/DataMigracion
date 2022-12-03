using AppClases;
using Microsoft.EntityFrameworkCore;

namespace WepApi.Model
{
    public class AppDbContext : DbContext
    {
        public DbSet<DataMigacion> UserGroups { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataMigacion>().ToTable("DataMigracion");

            modelBuilder.Entity<DataMigacion>().HasKey(ug => ug.Cui).HasName("pk_datamigracion");

            modelBuilder.Entity<DataMigacion>().HasIndex(p => p.Cui).IsUnique().HasDatabaseName("uk_datamigracion");
        }
    }
}