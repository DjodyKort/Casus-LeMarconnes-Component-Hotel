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
        [Column("ReserveringID")]
        public int ReserveringID { get; set; }

        [Required]
        [Column("GastID")]
        public int GastID { get; set; }

        [Required]
        [Column("EenheidID")]
        public int EenheidID { get; set; }

        [Required]
        [Column("PlatformID")]
        public int PlatformID { get; set; }

        [Required]
        [Column("Startdatum")]
        public DateTime Startdatum { get; set; }

        [Required]
        [Column("Einddatum")]
        public DateTime Einddatum { get; set; }

        [StringLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "Gereserveerd";

        // ==== OO Navigatie Properties ====
        [ForeignKey(nameof(GastID))]
        public GastDTO? Gast { get; set; }

        [ForeignKey(nameof(EenheidID))]
        public VerhuurEenheidDTO? Eenheid { get; set; }

        [ForeignKey(nameof(PlatformID))]
        public PlatformDTO? Platform { get; set; }

        // Master-Detail relatie
        [InverseProperty(nameof(ReserveringDetailDTO.Reservering))]
        public List<ReserveringDetailDTO> Details { get; set; } = new();

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