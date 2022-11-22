namespace DietApp.Models
{
    public class Meal
    {
        public int MealId { get; set; }
        public string Id { get; set; }
        public int ProductId { get; set; }
        public int DailyConsumptionId { get; set; }
        public int Weight { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Product Product { get; set; }
        public virtual DailyConsumption DailyConsumption { get; set; }
    }
}
