// ======== Imports ========
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    [Table("PLATFORM")]
    public class PlatformDTO {
        // ==== Properties ====
        [Key]
        public int PlatformID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Naam { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal CommissiePercentage { get; set; }

        // ==== Constructor ====
        public PlatformDTO() { }

        public PlatformDTO(int platformId, string naam, decimal commissiePercentage) {
            PlatformID = platformId;
            Naam = naam;
            CommissiePercentage = commissiePercentage;
        }
    }
}