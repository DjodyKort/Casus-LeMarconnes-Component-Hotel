// ======== Imports ========
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    [Table("RESERVERING")]
    public class ReserveringDTO {
        // ==== Properties ====
        [Key]
        public int ReserveringID { get; set; }

        public int GastID { get; set; }

        public int EenheidID { get; set; }

        public int PlatformID { get; set; }

        public DateTime Startdatum { get; set; }

        public DateTime Einddatum { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Gereserveerd";

        // ==== OOB (Relational) Properties ====
        [ForeignKey("GastID")]
        public virtual GastDTO? Gast { get; set; }

        [ForeignKey("EenheidID")]
        public virtual VerhuurEenheidDTO? Eenheid { get; set; }

        [ForeignKey("PlatformID")]
        public virtual PlatformDTO? Platform { get; set; }

        // Hiermee kun je straks doen: reservering.Details.Sum(d => d.Prijs)
        [InverseProperty("Reservering")]
        public virtual List<ReserveringDetailDTO> Details { get; set; } = new();

        // ==== Constructors ====
        public ReserveringDTO() { }
        public ReserveringDTO(int gastId, int eenheidId, int platformId, DateTime startdatum, DateTime einddatum) {
            GastID = gastId;
            EenheidID = eenheidId;
            PlatformID = platformId;
            Startdatum = startdatum;
            Einddatum = einddatum;
        }
    }
}