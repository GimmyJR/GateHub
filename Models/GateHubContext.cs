using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GateHub.Models
{
    public class GateHubContext:IdentityDbContext<AppUser>
    {
        public GateHubContext()
        {
            
        }
        public GateHubContext(DbContextOptions options):base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // vehicle - gate maping (M,M) Relationship
            modelBuilder.Entity<VehicleEntry>()
                .HasOne(v => v.vehicle)
                .WithMany(ve => ve.VehicleEntries)
                .HasForeignKey(ve => ve.VehicleId);

            modelBuilder.Entity<VehicleEntry>()
                .HasOne(g => g.gate )
                .WithMany(ve => ve.VehicleEntries)
                .HasForeignKey(ve => ve.GateId);

            // Configure composite key for GateFee
            modelBuilder.Entity<GateFee>()
                .HasKey(gf => new { gf.GateId, gf.VehicleType });
        }
        public virtual DbSet<VehicleEntry> VehicleEntries { get; set; }
        public virtual DbSet<Gate> Gates { get; set; }
        public virtual DbSet<VehicleOwner> VehicleOwners { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<Objection> Objections { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<LostVehicle> LostVehicles { get; set; }
        public virtual DbSet<GateStaff> GateStaff { get; set; }
        public virtual DbSet<GateFee> GateFees { get; set; }

    }
}
