// ======== Imports ========
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    [Table("TARIEF")]
    public class TariefDTO {
        // ==== Properties ====
        [Key]
        public int TariefID { get; set; }

        public int TypeID { get; set; }

        public int CategorieID { get; set; }

        public int? PlatformID { get; set; }

        [Column(TypeName = "decimal(19,4)")] // Standaard voor 'MONEY' in C# -> SQL mapping
        public decimal Prijs { get; set; }

        public bool TaxStatus { get; set; }

        [Column(TypeName = "decimal(19,4)")]
        public decimal TaxTarief { get; set; }

        public DateTime GeldigVan { get; set; }

        public DateTime? GeldigTot { get; set; }

        // ==== OOB (Relational) Properties ====
        [ForeignKey("TypeID")]
        public virtual AccommodatieTypeDTO? Type { get; set; }

        [ForeignKey("CategorieID")]
        public virtual TariefCategorieDTO? Categorie { get; set; }

        [ForeignKey("PlatformID")]
        public virtual PlatformDTO? Platform { get; set; }

        // ==== Constructor ====
        public TariefDTO() { }

        public TariefDTO(int tariefId, int typeId, int categorieId, decimal prijs, DateTime geldigVan) {
            TariefID = tariefId;
            TypeID = typeId;
            CategorieID = categorieId;
            Prijs = prijs;
            GeldigVan = geldigVan;
        }
    }
}