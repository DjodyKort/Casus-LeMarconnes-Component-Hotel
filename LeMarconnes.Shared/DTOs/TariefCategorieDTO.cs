// ======== Imports ========
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    [Table("TARIEF_CATEGORIE")]
    public class TariefCategorieDTO {
        // ==== Properties ====
        [Key]
        public int CategorieID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Naam { get; set; } = string.Empty;

        // ==== Constructor ====
        public TariefCategorieDTO() { }

        public TariefCategorieDTO(int categorieId, string naam) {
            CategorieID = categorieId;
            Naam = naam;
        }
    }
}