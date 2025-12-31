// ======== Imports ========
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    [Table("LOGBOEK")]
    public class LogboekDTO {
        // ==== Properties ====
        [Key]
        public int LogID { get; set; }

        public int? GebruikerID { get; set; }

        public DateTime Tijdstip { get; set; }

        [Required]
        [MaxLength(50)]
        public string Actie { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? TabelNaam { get; set; }

        public int? RecordID { get; set; }

        public string? OudeWaarde { get; set; }

        public string? NieuweWaarde { get; set; }

        // ==== OOB (Relational) Properties ====
        [ForeignKey("GebruikerID")]
        public virtual GebruikerDTO? Gebruiker { get; set; }

        // ==== Constructors ====
        public LogboekDTO() {
            Tijdstip = DateTime.Now;
        }

        public LogboekDTO(string actie, string? tabelNaam = null, int? recordId = null) {
            Actie = actie;
            TabelNaam = tabelNaam;
            RecordID = recordId;
            Tijdstip = DateTime.Now;
        }
    }
}