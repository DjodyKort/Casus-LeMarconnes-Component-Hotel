// ======== Imports ========
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    [Table("ACCOMMODATIE_TYPE")]
    public class AccommodatieTypeDTO {
        // ==== Properties ====
        [Key]
        public int TypeID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Naam { get; set; } = string.Empty;

        // ==== Constructor ====
        public AccommodatieTypeDTO() { }
        public AccommodatieTypeDTO(int typeId, string naam) {
            TypeID = typeId;
            Naam = naam;
        }
    }
}