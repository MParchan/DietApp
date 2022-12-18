using Microsoft.AspNetCore.Mvc;

namespace DietApp.Models
{
    public class DailyConsumption
    {
        [HiddenInput(DisplayValue = false)]
        public int DailyConsumptionId { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Meal> Meals { get; set; }
    }
}
