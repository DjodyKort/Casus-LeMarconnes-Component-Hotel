// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LeMarconnes.Shared.DTOs;
using LeMarconnes.API.DAL.Interfaces;

// ======== Namespace ========
namespace LeMarconnes.API.DAL.Repositories {
    public class HotelRepository : IHotelRepository {
        // ==== Properties ====
        private readonly LeMarconnesContext _context;

        // ==== Constructor ====
        public HotelRepository(LeMarconnesContext context) {
            _context = context;
        }

        // ============================================================
        // ==== INVENTARIS ====
        // ============================================================
        public async Task<List<VerhuurEenheidDTO>> GetHotelKamersAsync() {
            // TypeID 3 = Hotelkamer (Hardcoded voor Hotel Component)
            return await _context.VerhuurEenheden
                .Where(k => k.TypeID == 3)
                .Include(k => k.Type) // Join type naam
                .OrderBy(k => k.EenheidID)
                .ToListAsync();
        }

        public async Task<VerhuurEenheidDTO?> GetKamerByIdAsync(int kamerId) {
            return await _context.VerhuurEenheden
                .Include(k => k.Type)
                .FirstOrDefaultAsync(k => k.EenheidID == kamerId && k.TypeID == 3);
        }

        // ============================================================
        // ==== BESCHIKBAARHEID ====
        // ============================================================
        public async Task<bool> IsKamerBeschikbaarAsync(int kamerId, DateTime start, DateTime eind) {
            bool isBezet = await _context.Reserveringen.AnyAsync(r => 
                r.EenheidID == kamerId &&
                r.Status != "Geannuleerd" &&
                r.Startdatum < eind && 
                r.Einddatum > start
            );
            return !isBezet;
        }

        // ============================================================
        // ==== TARIEVEN & STAMDATA ====
        // ============================================================
        public async Task<TariefDTO?> GetGeldigTariefAsync(int platformId, DateTime datum) {
            return await _context.Tarieven
                .Where(t => t.TypeID == 3 && // Hotel
                            t.CategorieID == 1 && // Logies
                            (t.PlatformID == platformId || t.PlatformID == null) &&
                            t.GeldigVan <= datum &&
                            (t.GeldigTot == null || t.GeldigTot >= datum))
                .OrderByDescending(t => t.PlatformID)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TariefDTO>> GetAllTarievenAsync() {
            return await _context.Tarieven
                .Include(t => t.Type)
                .Include(t => t.Platform)
                .Include(t => t.Categorie)
                .Where(t => t.TypeID == 3) // Alleen Hotel tarieven tonen
                .OrderBy(t => t.GeldigVan)
                .ToListAsync();
        }

        public async Task<List<PlatformDTO>> GetAllPlatformsAsync() {
            return await _context.Platformen.ToListAsync();
        }

        public async Task<List<AccommodatieTypeDTO>> GetAllAccommodatieTypesAsync() {
            return await _context.AccommodatieTypes.ToListAsync();
        }

        // ============================================================
        // ==== BOEKING & TRANSACTIES ====
        // ============================================================
        public async Task<int> MaakBoekingAsync(ReserveringDTO reservering, List<ReserveringDetailDTO> details) {
            _context.Reserveringen.Add(reservering);
            await _context.SaveChangesAsync(); // ID genereren

            foreach (var detail in details) {
                detail.ReserveringID = reservering.ReserveringID;
                _context.ReserveringDetails.Add(detail);
            }
            await _context.SaveChangesAsync();
            return reservering.ReserveringID;
        }

        // ============================================================
        // ==== RESERVERING BEHEER ====
        // ============================================================
        public async Task<List<ReserveringDTO>> GetAllReserveringenAsync() {
            return await _context.Reserveringen
                .Include(r => r.Gast)
                .Include(r => r.Eenheid)
                .Include(r => r.Platform)
                .Where(r => r.Eenheid.TypeID == 3) // Alleen Hotel reserveringen
                .OrderByDescending(r => r.Startdatum)
                .ToListAsync();
        }

        public async Task<ReserveringDTO?> GetReserveringByIdAsync(int id) {
            return await _context.Reserveringen
                .Include(r => r.Gast)
                .Include(r => r.Eenheid)
                .FirstOrDefaultAsync(r => r.ReserveringID == id);
        }

        public async Task<List<ReserveringDetailDTO>> GetReserveringDetailsAsync(int reserveringId) {
            return await _context.ReserveringDetails
                .Include(d => d.Categorie)
                .Where(d => d.ReserveringID == reserveringId)
                .ToListAsync();
        }

        public async Task<bool> UpdateReserveringStatusAsync(int id, string status) {
            var res = await _context.Reserveringen.FindAsync(id);
            if (res == null) return false;

            res.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReserveringAsync(int id) {
            var res = await _context.Reserveringen.FindAsync(id);
            if (res == null) return false;

            // Details worden automatisch verwijderd door Database Cascade Delete, 
            // of we doen het expliciet als we dat in EF Core configureren.
            // Voor de zekerheid halen we details ook weg:
            var details = _context.ReserveringDetails.Where(d => d.ReserveringID == id);
            _context.ReserveringDetails.RemoveRange(details);
            
            _context.Reserveringen.Remove(res);
            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // ==== GAST BEHEER ====
        // ============================================================
        public async Task<List<GastDTO>> GetAllGastenAsync() {
            return await _context.Gasten.OrderBy(g => g.Naam).ToListAsync();
        }

        public async Task<GastDTO?> GetGastByIdAsync(int id) {
            return await _context.Gasten.FindAsync(id);
        }

        public async Task<GastDTO?> GetGastByEmailAsync(string email) {
            return await _context.Gasten.FirstOrDefaultAsync(g => g.Email == email);
        }

        public async Task<int> CreateGastAsync(GastDTO gast) {
            _context.Gasten.Add(gast);
            await _context.SaveChangesAsync();
            return gast.GastID;
        }

        public async Task<bool> UpdateGastAsync(GastDTO gast) {
            var existing = await _context.Gasten.FindAsync(gast.GastID);
            if (existing == null) return false;

            // Update properties
            _context.Entry(existing).CurrentValues.SetValues(gast);
            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // ==== LOGGING ====
        // ============================================================
        public async Task CreateLogEntryAsync(LogboekDTO log) {
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LogboekDTO>> GetRecentLogsAsync(int count) {
            return await _context.Logs
                .OrderByDescending(l => l.Tijdstip)
                .Take(count)
                .ToListAsync();
        }
    }
}