// ======== Imports ========
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    [Table("GAST")]
    public class GastDTO {
        // ==== Properties ====
        [Key]
        public int GastID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Naam { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Tel { get; set; }

        [Required]
        [MaxLength(100)]
        public string Straat { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Huisnr { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Postcode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Plaats { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Land { get; set; } = "Nederland";

        [MaxLength(34)]
        public string? IBAN { get; set; }

        // ==== Constructor ====
        public GastDTO() { }
        public GastDTO(int gastId, string naam, string email) {
            GastID = gastId;
            Naam = naam;
            Email = email;
        }
    }
}