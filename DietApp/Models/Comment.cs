namespace DietApp.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Id { get; set; }
        public int RecipeId { get; set; }
        public string Content { get; set; }
        public double Rating { get; set; }
        public DateTime Date { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Recipe Recipe { get; set; }
    }
}
