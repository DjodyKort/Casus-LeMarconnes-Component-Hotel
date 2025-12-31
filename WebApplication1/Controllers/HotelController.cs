// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase {
        // ==== Properties ====
        private readonly IHotelRepository _repository;

        // ==== Constructor ====
        public HotelController(IHotelRepository repository) {
            _repository = repository;
        }

        // ============================================================
        // ==== EENHEDEN (INVENTARIS) ====
        // ============================================================

        [HttpGet("eenheden")]
        public async Task<ActionResult<List<VerhuurEenheidDTO>>> GetAllUnits() {
            var units = await _repository.GetHotelKamersAsync();
            return Ok(units);
        }

        [HttpGet("eenheden/{id}")]
        public async Task<ActionResult<VerhuurEenheidDTO>> GetUnitById(int id) {
            var unit = await _repository.GetKamerByIdAsync(id);
            if (unit == null) return NotFound($"Hotelkamer {id} niet gevonden.");
            return Ok(unit);
        }

        // ============================================================
        // ==== BESCHIKBAARHEID ====
        // ============================================================

        [HttpGet("beschikbaarheid")]
        public async Task<ActionResult<List<VerhuurEenheidDTO>>> CheckBeschikbaarheid(
            [FromQuery] DateTime startDatum, [FromQuery] DateTime eindDatum) {
            
            if (eindDatum <= startDatum) return BadRequest("Einddatum moet na startdatum liggen.");

            // Seizoen Check (1 maart - 31 oktober)
            if (!IsBinnenSeizoen(startDatum) || !IsBinnenSeizoen(eindDatum)) {
                return BadRequest("Het hotel is gesloten (Seizoen: 1 mrt - 31 okt).");
            }

            var units = await _repository.GetHotelKamersAsync();
            foreach (var unit in units) {
                unit.IsBeschikbaar = await _repository.IsKamerBeschikbaarAsync(unit.EenheidID, startDatum, eindDatum);
            }
            
            // Log (optioneel, kan druk worden)
            // await _repository.CreateLogEntryAsync(new LogboekDTO("BESCHIKBAARHEID_CHECK", "HOTEL"));

            return Ok(units);
        }

        // ============================================================
        // ==== BOEKING (FLOW) ====
        // ============================================================

        [HttpPost("boek")]
        public async Task<ActionResult<BoekingResponseDTO>> MaakBoeking([FromBody] BoekingRequestDTO request) {
            // 1. Validatie
            if (request.EindDatum <= request.StartDatum) return BadRequest(BoekingResponseDTO.Failure("Ongeldige data."));
            if (!IsBinnenSeizoen(request.StartDatum) || !IsBinnenSeizoen(request.EindDatum)) return BadRequest(BoekingResponseDTO.Failure("Hotel gesloten."));

            var kamer = await _repository.GetKamerByIdAsync(request.EenheidID);
            if (kamer == null) return NotFound(BoekingResponseDTO.Failure("Kamer niet gevonden."));
            
            // Strikte Capaciteitscheck
            if (request.AantalPersonen > kamer.MaxCapaciteit) 
                return BadRequest(BoekingResponseDTO.Failure($"Max {kamer.MaxCapaciteit} personen."));

            if (!await _repository.IsKamerBeschikbaarAsync(request.EenheidID, request.StartDatum, request.EindDatum))
                return Conflict(BoekingResponseDTO.Failure("Kamer niet beschikbaar."));

            // 2. Gast
            int gastId;
            var bestaandeGast = await _repository.GetGastByEmailAsync(request.GastEmail);
            if (bestaandeGast != null) {
                gastId = bestaandeGast.GastID;
            } else {
                var nieuweGast = new GastDTO {
                    Naam = request.GastNaam, Email = request.GastEmail, Tel = request.GastTel,
                    Straat = request.GastStraat, Huisnr = request.GastHuisnr, 
                    Postcode = request.GastPostcode, Plaats = request.GastPlaats, Land = request.GastLand
                };
                gastId = await _repository.CreateGastAsync(nieuweGast);
            }

            // 3. Prijzen (Exclusief Tax)
            var basisTarief = await _repository.GetGeldigTariefAsync(request.PlatformID, request.StartDatum);
            if (basisTarief == null) return BadRequest(BoekingResponseDTO.Failure("Geen prijs gevonden."));

            decimal taxRate = 0.50m; // Hardcoded conform seed
            int nachten = (request.EindDatum - request.StartDatum).Days;
            
            decimal kamerKosten = basisTarief.Prijs * nachten;
            decimal taxKosten = (taxRate * request.AantalPersonen) * nachten;
            decimal totaalPrijs = kamerKosten + taxKosten;

            // 4. Opslaan
            var reservering = new ReserveringDTO {
                GastID = gastId, EenheidID = request.EenheidID, PlatformID = request.PlatformID,
                Startdatum = request.StartDatum, Einddatum = request.EindDatum, Status = "Gereserveerd"
            };

            var details = new List<ReserveringDetailDTO> {
                new ReserveringDetailDTO(0, 1, nachten, basisTarief.Prijs), // Logies
                new ReserveringDetailDTO(0, 2, request.AantalPersonen * nachten, taxRate) // Tax
            };

            int resId = await _repository.MaakBoekingAsync(reservering, details);
            await _repository.CreateLogEntryAsync(new LogboekDTO("HOTEL_BOEKING", "RESERVERING", resId));

            return Ok(BoekingResponseDTO.Success(resId, kamer.Naam, request.StartDatum, request.EindDatum, totaalPrijs));
        }

        // ============================================================
        // ==== RESERVERING BEHEER (ADMIN) ====
        // ============================================================

        [HttpGet("reserveringen")]
        public async Task<ActionResult<List<ReserveringDTO>>> GetAllReserveringen() {
            return Ok(await _repository.GetAllReserveringenAsync());
        }

        [HttpGet("reserveringen/{id}")]
        public async Task<ActionResult<ReserveringDTO>> GetReserveringById(int id) {
            var res = await _repository.GetReserveringByIdAsync(id);
            if (res == null) return NotFound();
            return Ok(res);
        }

        [HttpPut("reserveringen/{id}/annuleer")]
        public async Task<ActionResult> AnnuleerReservering(int id) {
            if (await _repository.UpdateReserveringStatusAsync(id, "Geannuleerd")) {
                await _repository.CreateLogEntryAsync(new LogboekDTO("GEANNULEERD", "RESERVERING", id));
                return Ok(new { Message = "Geannuleerd" });
            }
            return NotFound();
        }

        [HttpDelete("reserveringen/{id}")]
        public async Task<ActionResult> DeleteReservering(int id) {
            if (await _repository.DeleteReserveringAsync(id)) {
                await _repository.CreateLogEntryAsync(new LogboekDTO("VERWIJDERD", "RESERVERING", id));
                return NoContent();
            }
            return NotFound();
        }

        // ============================================================
        // ==== GASTEN (ADMIN) ====
        // ============================================================

        [HttpGet("gasten")]
        public async Task<ActionResult<List<GastDTO>>> GetGasten([FromQuery] string? email = null) {
            if (!string.IsNullOrEmpty(email)) {
                var gast = await _repository.GetGastByEmailAsync(email);
                return gast != null ? Ok(new List<GastDTO> { gast }) : NotFound();
            }
            return Ok(await _repository.GetAllGastenAsync());
        }

        [HttpGet("gasten/{id}")]
        public async Task<ActionResult<GastDTO>> GetGast(int id) {
            var gast = await _repository.GetGastByIdAsync(id);
            return gast != null ? Ok(gast) : NotFound();
        }

        [HttpPost("gasten")]
        public async Task<ActionResult> CreateGast([FromBody] GastDTO gast) {
            if (await _repository.GetGastByEmailAsync(gast.Email) != null) return Conflict("Email bestaat al.");
            var id = await _repository.CreateGastAsync(gast);
            return CreatedAtAction(nameof(GetGast), new { id }, gast);
        }

        [HttpPut("gasten/{id}")]
        public async Task<ActionResult> UpdateGast(int id, [FromBody] GastDTO gast) {
            if (id != gast.GastID) return BadRequest();
            if (await _repository.UpdateGastAsync(gast)) return NoContent();
            return NotFound();
        }

        // ============================================================
        // ==== STAMDATA & LOGS (ADMIN) ====
        // ============================================================

        [HttpGet("platformen")]
        public async Task<ActionResult<List<PlatformDTO>>> GetPlatforms() {
            return Ok(await _repository.GetAllPlatformsAsync());
        }

        [HttpGet("tarieven")]
        public async Task<ActionResult<List<TariefDTO>>> GetTarieven() {
            return Ok(await _repository.GetAllTarievenAsync());
        }

        [HttpGet("logs")]
        public async Task<ActionResult<List<LogboekDTO>>> GetLogs([FromQuery] int count = 50) {
            return Ok(await _repository.GetRecentLogsAsync(count));
        }

        // ==== Helpers ====
        private bool IsBinnenSeizoen(DateTime d) {
            // 1 maart (3) t/m 31 oktober (10)
            return d.Month >= 3 && d.Month <= 10;
        }
    }
}