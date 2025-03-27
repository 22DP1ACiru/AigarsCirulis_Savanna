using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Savanna.Data.Entities
{
    public class GameSave
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }


        [Required]
        [MaxLength(128)]
        public string SaveName { get; set; }

        [Required]
        public string GameState { get; set; } // Serialized JSON

        public int Iteration { get; set; }

        public int LivingAnimalCount { get; set; }

        public DateTime SaveDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } // Navigation property
    }
}