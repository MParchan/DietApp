using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace DietApp.Models
{
    public class DailyConsumption
    {
        [HiddenInput(DisplayValue = false)]
        public int DailyConsumptionId { get; set; }
        public string UserId { get; set; }
        [Display(Name = "Data")]
        public string Date { get; set; }
        [Display(Name = "Bilans kaloryczny")]
        public double CaloricBalance { get; set; }

        public virtual ApplicationUser User { get; set; }
        [Display(Name = "Posiłki")]
        public virtual ICollection<Meal> Meals { get; set; }
    }
}
