using Microsoft.EntityFrameworkCore;

namespace GateHub.Models
{
    public class GateHubContext:DbContext
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
        }


    }
}
