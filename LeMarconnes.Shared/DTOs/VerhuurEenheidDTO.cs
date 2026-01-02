// ======== Imports ========
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs {
    [Table("VERHUUR_EENHEID")]
    public class VerhuurEenheidDTO {
        // ==== Properties ====
        [Key]
        public int EenheidID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Naam { get; set; } = string.Empty;

        public int TypeID { get; set; }

        public int MaxCapaciteit { get; set; }

        public int? ParentEenheidID { get; set; }

        // ==== OOB (Relational) Properties ====
        [ForeignKey("TypeID")]
        public virtual AccommodatieTypeDTO? Type { get; set; }

        [JsonIgnore]
        [ForeignKey("ParentEenheidID")]
        public virtual VerhuurEenheidDTO? ParentEenheid { get; set; }

        // Deze lijst bevat alle 'kinderen' die naar DEZE eenheid wijzen als parent
        [InverseProperty("ParentEenheid")]
        public virtual List<VerhuurEenheidDTO> ChildEenheden { get; set; } = new();

        // ==== Runtime Properties ====
        [NotMapped] // Dit betekent: EF Core, negeer dit veld voor de database!
        public bool IsBeschikbaar { get; set; } = true;

        // ==== Constructor ====
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