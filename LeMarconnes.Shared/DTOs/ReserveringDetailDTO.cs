// ======== Imports ========
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    [Table("RESERVERING_DETAIL")]
    public class ReserveringDetailDTO {
        // ==== Properties ====
        [Key]
        [Column("DetailID")]
        public int DetailID { get; set; }

        [Required]
        [Column("ReserveringID")]
        public int ReserveringID { get; set; }

        [Required]
        [Column("CategorieID")]
        public int CategorieID { get; set; }

        [Required]
        [Column("Aantal")]
        public int Aantal { get; set; } = 1;

        [Required]
        [Column("PrijsOpMoment", TypeName = "money")] // Of decimal(18,2)
        public decimal PrijsOpMoment { get; set; }

        // ==== OO Navigatie Properties ====
        [ForeignKey(nameof(CategorieID))]
        public TariefCategorieDTO? Categorie { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(ReserveringID))]
        public ReserveringDTO? Reservering { get; set; }

        // ==== Constructor ====
        public ReserveringDetailDTO() { }

        public ReserveringDetailDTO(int reserveringId, int categorieId, int aantal, decimal prijsOpMoment) {
            ReserveringID = reserveringId;
            CategorieID = categorieId;
            Aantal = aantal;
            PrijsOpMoment = prijsOpMoment;
        }
    }
}