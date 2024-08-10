using Microsoft.EntityFrameworkCore;

namespace CoreMasterDetailsWithProcAndAggre.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
            
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public virtual DbSet<Employee> Employees { get; set; }

        public virtual DbSet<Experience> Experiences { get; set; }
    }
}
