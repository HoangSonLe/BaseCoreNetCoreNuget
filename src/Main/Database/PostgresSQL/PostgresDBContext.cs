using BaseNetCore.Core.src.Main.Common.Attributes;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseNetCore.Core.src.Main.Database.PostgresSQL
{
    /// <summary>
    /// Base PostgreSQL DbContext with automatic searchable entity support.
    /// Automatically generates search strings for entities marked with [SearchableEntity] attribute.
    /// 
    /// Usage:
    /// public class ApplicationDbContext : PostgresDBContext
    /// {
    ///     public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    ///     
    ///     public DbSet<Product> Products { get; set; }
    ///   public DbSet<User> Users { get; set; }
    /// }
    /// 
    /// Features:
    /// - ✅ Automatic search string generation for [SearchableEntity] entities
    /// - ✅ Legacy timestamp behavior support for Npgsql
    /// - ✅ No additional configuration needed
    /// </summary>
    public class PostgresDBContext : DbContext
    {
        public PostgresDBContext(DbContextOptions options)
  : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public PostgresDBContext() : base()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// ✅ Automatically generates search strings for entities marked with [SearchableEntity] before saving.
        /// Checks both Added and Modified entities.
        /// </summary>
        public override int SaveChanges()
        {
            GenerateSearchStringsBeforeSave();
            return base.SaveChanges();
        }

        /// <summary>
        /// ✅ Automatically generates search strings for entities marked with [SearchableEntity] before saving (async).
        /// Checks both Added and Modified entities.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            GenerateSearchStringsBeforeSave();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Generates search strings for all tracked entities that:
        /// 1. Implement ISearchableEntity interface
        /// 2. Are marked with [SearchableEntity] attribute and Enabled = true
        /// 3. Are in Added or Modified state
        /// </summary>
        private void GenerateSearchStringsBeforeSave()
        {
            var searchableEntries = ChangeTracker.Entries<ISearchableEntity>()
      .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in searchableEntries)
            {
                var entityType = entry.Entity.GetType();

                // Check if entity has [SearchableEntity] attribute and it's enabled
                var searchableAttr = entityType.GetCustomAttributes(typeof(SearchableEntityAttribute), true)
             .FirstOrDefault() as SearchableEntityAttribute;

                // Only generate if attribute exists and is enabled
                if (searchableAttr != null && searchableAttr.Enabled)
                {
                    entry.Entity.GenerateSearchString();
                }
            }
        }
    }
}
