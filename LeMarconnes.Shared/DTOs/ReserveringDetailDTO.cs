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
        public int DetailID { get; set; }

        public int ReserveringID { get; set; }

        public int CategorieID { get; set; }

        public int Aantal { get; set; } = 1;

        [Column(TypeName = "decimal(19,4)")]
        public decimal PrijsOpMoment { get; set; }

        // ==== OOB (Relational) Properties ====
        [JsonIgnore]
        [ForeignKey("ReserveringID")]
        public virtual ReserveringDTO? Reservering { get; set; }

        [ForeignKey("CategorieID")]
        public virtual TariefCategorieDTO? Categorie { get; set; }

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