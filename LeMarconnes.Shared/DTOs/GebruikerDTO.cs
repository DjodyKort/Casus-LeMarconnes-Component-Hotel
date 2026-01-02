// ======== Imports ========
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    [Table("GEBRUIKER")]
    public class GebruikerDTO {
        // ==== Properties ====
        [Key]
        public int GebruikerID { get; set; }

        public int? GastID { get; set; }

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        [Required]
        [MaxLength(255)]
        public string WachtwoordHash { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Rol { get; set; } = "Gast";

        // ==== OOB (Relational) Properties ====
        [ForeignKey("GastID")]
        public virtual GastDTO? Gast { get; set; }

        // ==== Constructor ====
        public GebruikerDTO() { }
        public GebruikerDTO(int gebruikerId, string email, string rol) {
            GebruikerID = gebruikerId;
            Email = email;
            Rol = rol;
        }
    }
}