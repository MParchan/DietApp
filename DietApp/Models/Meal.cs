using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DietApp.Models
{
    public class Meal
    {
        [HiddenInput(DisplayValue = false)]
        public int MealId { get; set; }
        public string UserId { get; set; }
        [Required(ErrorMessage="Wybierz produkt")]
        [Display(Name="Produkt")]
        public int ProductId { get; set; }
        public int DailyConsumptionId { get; set; }
        [Required(ErrorMessage = "Podaj wagę produktu")]
        [Display(Name = "Waga")]
        [Range(1, int.MaxValue, ErrorMessage = "{0} musi być większa niż 0.")]
        public int Weight { get; set; }
        [Display(Name = "Kalorie")]
        public double TotalKcal { get; set; }
        [Display(Name = "Tłuszcz")]
        public double TotalFat { get; set; }
        [Display(Name = "Węglowodany")]
        public double TotalCarbo { get; set; }
        [Display(Name = "Białko")]
        public double TotalProtein { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Product Product { get; set; }
        public virtual DailyConsumption DailyConsumption { get; set; }
    }
}
