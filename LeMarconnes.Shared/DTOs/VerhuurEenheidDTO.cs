// ======== Imports ========
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    // Mapt naar tabel VERHUUR_EENHEID
    [Table("VERHUUR_EENHEID")]
    public class VerhuurEenheidDTO {
        // ==== Properties ====
        [Key]
        [Column("EenheidID")]
        public int EenheidID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Naam")]
        public string Naam { get; set; } = string.Empty;

        [Required]
        [Column("TypeID")]
        public int TypeID { get; set; }

        [Required]
        [Column("MaxCapaciteit")]
        public int MaxCapaciteit { get; set; }

        [Column("ParentEenheidID")]
        public int? ParentEenheidID { get; set; }

        // ==== OO Navigatie Properties ====
        [ForeignKey(nameof(TypeID))]
        public AccommodatieTypeDTO? Type { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(ParentEenheidID))]
        public VerhuurEenheidDTO? ParentEenheid { get; set; }

        [InverseProperty(nameof(ParentEenheid))]
        public List<VerhuurEenheidDTO> ChildEenheden { get; set; } = new();

        // ==== Runtime Properties ====
        [NotMapped] // Dit veld bestaat niet in de database
        public bool IsBeschikbaar { get; set; } = true;

        // ==== Constructors ====
        public VerhuurEenheidDTO() { }

        public VerhuurEenheidDTO(int eenheidId, string naam, int typeId, int maxCapaciteit, int? parentEenheidId) {
            EenheidID = eenheidId;
            Naam = naam;
            TypeID = typeId;
            MaxCapaciteit = maxCapaciteit;
            ParentEenheidID = parentEenheidId;
        }
    }
}