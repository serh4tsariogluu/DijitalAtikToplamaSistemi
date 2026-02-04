using AtikDonusum.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AtikDonusum.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<WastePoint> WastePoints { get; set; }

        // ÖNEMLİ: Route yerine CollectionRoute tablosu
        public DbSet<CollectionRoute> CollectionRoutes { get; set; }
    }
}