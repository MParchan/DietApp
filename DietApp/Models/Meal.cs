using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DietApp.Models
{
    public class Meal
    {
        [HiddenInput(DisplayValue = false)]
        public int MealId { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int DailyConsumptionId { get; set; }
        public int Weight { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Product Product { get; set; }
        public virtual DailyConsumption DailyConsumption { get; set; }
    }
}
