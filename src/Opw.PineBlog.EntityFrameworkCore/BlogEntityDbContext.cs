using Microsoft.EntityFrameworkCore;
using Opw.PineBlog.Entities;
using Opw.EntityFrameworkCore;

namespace Opw.PineBlog.EntityFrameworkCore
{
    public class BlogEntityDbContext : EntityDbContext, IBlogEntityDbContext
    {
        public DbSet<BlogSettings> BlogSettings { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Author> Authors { get; set; }

        public BlogEntityDbContext(DbContextOptions<BlogEntityDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName($"PineBlog_{entityType.GetTableName()}");
            }
        }
    }
}
