// ======== Imports ========
using Microsoft.EntityFrameworkCore;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.DAL {
    public class LeMarconnesContext : DbContext {
        // ==== DbSets (Tables) ====
        public DbSet<GastDTO> Gasten { get; set; }
        public DbSet<GebruikerDTO> Gebruikers { get; set; }
        public DbSet<AccommodatieTypeDTO> AccommodatieTypes { get; set; }
        public DbSet<VerhuurEenheidDTO> VerhuurEenheden { get; set; }
        public DbSet<PlatformDTO> Platformen { get; set; }
        public DbSet<TariefCategorieDTO> TariefCategorieen { get; set; }
        public DbSet<TariefDTO> Tarieven { get; set; }
        public DbSet<ReserveringDTO> Reserveringen { get; set; }
        public DbSet<ReserveringDetailDTO> ReserveringDetails { get; set; }
        public DbSet<LogboekDTO> Logs { get; set; }

        // ==== Configuration ====
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision to prevent warnings and rounding errors
            modelBuilder.Entity<TariefDTO>().Property(t => t.Prijs).HasPrecision(10, 2);
            modelBuilder.Entity<TariefDTO>().Property(t => t.TaxTarief).HasPrecision(10, 2);
            modelBuilder.Entity<ReserveringDetailDTO>().Property(d => d.PrijsOpMoment).HasPrecision(10, 2);
            modelBuilder.Entity<PlatformDTO>().Property(p => p.CommissiePercentage).HasPrecision(5, 2);
        }

        // ==== Constructor ====
        public LeMarconnesContext(DbContextOptions<LeMarconnesContext> options) : base(options) { }
    }
}