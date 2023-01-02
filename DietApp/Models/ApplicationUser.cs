using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DietApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override string Id { get; set; }

        [Display(Name = "Wzrost")]
        [Range(1, int.MaxValue, ErrorMessage = "Podaj poprawny wzrost.")]
        public int Height { get; set; }

        [Display(Name = "Waga")]
        [Range(1, int.MaxValue, ErrorMessage = "Podaj poprawną wagę.")]
        public int Weight { get; set; }

        [Display(Name = "Wiek")]
        [Range(1, int.MaxValue, ErrorMessage = "Podaj poprawny wiek.")]
        public int Age { get; set; }

        public double BMR { get; set; }
        public double BMI { get; set; }

        [Display(Name = "Płeć")]
        [Range(1, 2, ErrorMessage = "Wybierz płeć.")]
        [EnumDataType(typeof(Gender))]
        public Gender Gender { get; set; }

        [Display(Name = "Aktywność fizyczna")]
        [Required(ErrorMessage = "Określ aktywność fizyczną")]
        [EnumDataType(typeof(ActivityFactor))]
        public ActivityFactor Activity { get; set; }

        public virtual ICollection<Recipe> Recipes { get; set; }
        public virtual ICollection<DailyConsumption> DailyFood { get; set; }
        public virtual ICollection<Product> FavoriteProducts { get; set; }
    }
    public enum Gender
    {
        NoGender,
        Male,
        Female,
    }
    public enum ActivityFactor
    {
        Lack = 12,
        Light = 14,
        Medium = 16,
        High = 18,
        VeryHigh = 20,
    }

}
