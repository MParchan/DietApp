using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DietApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override string Id { get; set; }
        public int Height { get; set; }
        public double Weight { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public ActivityFactor Activity { get; set; }

        public virtual ICollection<Recipe> Recipes { get; set; }
        public virtual ICollection<DailyConsumption> DailyFood { get; set; }
    }
    public enum Gender
    {
        Male,
        Female,
    }
    public enum ActivityFactor
    {
        [Display(Name = "Brak aktywności fizycznej")]
        Lack = 12,
        [Display(Name = "Lekka aktywność (aktywność – ok. 140 minut tygodniowo)")]
        Light = 14,
        [Display(Name = "Średnia aktywność (aktywność – ok. 280 minut tygodniowo)")]
        Medium = 16,
        [Display(Name = "Wysoka aktywność (aktywność – ok. 420 minut tygodniowo)")]
        High = 18,
        [Display(Name = "Bardzo wysoka aktywność fizyczna (aktywność – ok. 560 minut tygodniowo)")]
        VeryHigh = 20,
    }

}
