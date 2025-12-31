// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.DAL.Interfaces {
    public interface IHotelRepository {
        // ==== Inventaris (Hotel Specifiek) ====
        Task<List<VerhuurEenheidDTO>> GetHotelKamersAsync();
        Task<VerhuurEenheidDTO?> GetKamerByIdAsync(int kamerId);

        // ==== Beschikbaarheid (Hotel Specifiek) ====
        Task<bool> IsKamerBeschikbaarAsync(int kamerId, DateTime start, DateTime eind);

        // ==== Tarieven & Stamdata ====
        Task<TariefDTO?> GetGeldigTariefAsync(int platformId, DateTime datum);
        Task<List<TariefDTO>> GetAllTarievenAsync();
        Task<List<PlatformDTO>> GetAllPlatformsAsync();
        Task<List<AccommodatieTypeDTO>> GetAllAccommodatieTypesAsync();

        // ==== Boeking Flow (Transacties) ====
        Task<int> MaakBoekingAsync(ReserveringDTO reservering, List<ReserveringDetailDTO> details);

        // ==== Reservering Beheer (Admin) ====
        Task<List<ReserveringDTO>> GetAllReserveringenAsync();
        Task<ReserveringDTO?> GetReserveringByIdAsync(int id);
        Task<List<ReserveringDetailDTO>> GetReserveringDetailsAsync(int reserveringId);
        Task<bool> UpdateReserveringStatusAsync(int id, string status); // Voor annuleren/inchecken
        Task<bool> DeleteReserveringAsync(int id); // Hard delete

        // ==== Gast Beheer ====
        Task<List<GastDTO>> GetAllGastenAsync();
        Task<GastDTO?> GetGastByIdAsync(int id);
        Task<GastDTO?> GetGastByEmailAsync(string email);
        Task<int> CreateGastAsync(GastDTO gast);
        Task<bool> UpdateGastAsync(GastDTO gast);

        // ==== Logging ====
        Task CreateLogEntryAsync(LogboekDTO log);
        Task<List<LogboekDTO>> GetRecentLogsAsync(int count);
    }
}