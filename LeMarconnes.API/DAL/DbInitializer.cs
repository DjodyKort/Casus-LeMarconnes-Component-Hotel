// ======== Imports ========
using System;
using System.Linq;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.DAL {
    public static class DbInitializer {
        // ==== Main Method ====
        public static void Initialize(LeMarconnesContext context) {
            // 1. Ensure Database Exists
            context.Database.EnsureCreated();

            // 2. Check if seeding is needed (Look for Hotel Type)
            if (context.AccommodatieTypes.Any(t => t.TypeID == 3)) {
                return; // DB already seeded for Hotel
            }

            Console.WriteLine("--> Seeding Database met Hotel Data...");

            // ==== Lookup Data ====
            // Ensure types exist (Upsert logic mostly manually handled here via Any checks if needed, but assuming fresh DB or non-conflicting IDs)
            if (!context.AccommodatieTypes.Any(t => t.TypeID == 3)) {
                context.AccommodatieTypes.Add(new AccommodatieTypeDTO(3, "Hotelkamer"));
            }

            if (!context.Platformen.Any()) {
                context.Platformen.AddRange(
                    new PlatformDTO(1, "Eigen Website", 0.00m),
                    new PlatformDTO(2, "Booking.com", 15.00m),
                    new PlatformDTO(3, "Airbnb", 3.00m)
                );
            }

            if (!context.TariefCategorieen.Any()) {
                context.TariefCategorieen.AddRange(
                    new TariefCategorieDTO(1, "Logies"),
                    new TariefCategorieDTO(2, "Toeristenbelasting")
                );
            }
            
            // Save lookups first to generate IDs if needed (though we use fixed IDs here)
            context.SaveChanges();

            // ==== Hotel Inventaris (Fixed IDs 11-16) ====
            if (!context.VerhuurEenheden.Any(v => v.TypeID == 3)) {
                var hotelKamers = new VerhuurEenheidDTO[] {
                    // Begane Grond (2p)
                    new VerhuurEenheidDTO { EenheidID = 11, Naam = "Kamer 1 (2p - BG)", TypeID = 3, MaxCapaciteit = 2, ParentEenheidID = null },
                    new VerhuurEenheidDTO { EenheidID = 12, Naam = "Kamer 2 (2p - BG)", TypeID = 3, MaxCapaciteit = 2, ParentEenheidID = null },
                    
                    // 1e Verdieping (4p/5p)
                    new VerhuurEenheidDTO { EenheidID = 13, Naam = "Kamer 3 (4p - 1e)", TypeID = 3, MaxCapaciteit = 4, ParentEenheidID = null },
                    new VerhuurEenheidDTO { EenheidID = 14, Naam = "Kamer 4 (4p - 1e)", TypeID = 3, MaxCapaciteit = 4, ParentEenheidID = null },
                    new VerhuurEenheidDTO { EenheidID = 15, Naam = "Kamer 5 (5p - 1e)", TypeID = 3, MaxCapaciteit = 5, ParentEenheidID = null },
                    
                    // Zolder (4p)
                    new VerhuurEenheidDTO { EenheidID = 16, Naam = "Kamer 6 (4p - Zolder)", TypeID = 3, MaxCapaciteit = 4, ParentEenheidID = null }
                };
                context.VerhuurEenheden.AddRange(hotelKamers);
            }

            // ==== Hotel Tarieven (2025) ====
            // Rule: Hotel prices are EXCLUSIVE tax (TaxStatus = false)
            if (!context.Tarieven.Any(t => t.TypeID == 3)) {
                var seizoenStart = new DateTime(2025, 3, 1);
                var seizoenEind = new DateTime(2025, 10, 31);

                context.Tarieven.AddRange(
                    // Basisprijs
                    new TariefDTO { 
                        TypeID = 3, CategorieID = 1, PlatformID = 1, 
                        Prijs = 80.00m, TaxStatus = false, TaxTarief = 0, 
                        GeldigVan = seizoenStart, GeldigTot = seizoenEind 
                    },
                    // Toeristenbelasting (Apart tarief)
                    new TariefDTO { 
                        TypeID = 3, CategorieID = 2, PlatformID = null, 
                        Prijs = 0.50m, TaxStatus = false, TaxTarief = 0, 
                        GeldigVan = new DateTime(2025, 1, 1), GeldigTot = null 
                    }
                );
            }

            context.SaveChanges();
            Console.WriteLine("--> Hotel Seeding Voltooid.");
        }
    }
}