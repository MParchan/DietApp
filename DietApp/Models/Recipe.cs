using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DietApp.Models
{
    public class Recipe
    {
        public int RecipeId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }
        public string Description { get; set; }
        public int DifficultyLevel { get; set; }
        public int PreparationTime { get; set; }
        public int Portions { get; set; }
        public double TotalKcal { get; set; }
        public double TotalFat { get; set; }
        public double TotalCarbo { get; set; }
        public double TotalProtein { get; set; }
        public int TotalWeight { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Category> Category { get; set; }
    }
}
