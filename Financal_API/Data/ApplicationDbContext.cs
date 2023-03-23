using Financal_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Financal_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) : base(option)
        {

        }

        public DbSet<GoldPrice> GoldPrice { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GoldPrice>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()"); // CreatedAt 속성에 대한 기본값 설정
            });
        }
    }
}
