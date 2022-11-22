namespace DietApp.Models
{
    public class DailyConsumption
    {
        public int DailyConsumptionId { get; set; }
        public string Id { get; set; }
        public DateTime Date { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Meal> Meals { get; set; }
    }
}
