using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DietApp.Models
{
    public class Comment
    {
        [HiddenInput(DisplayValue = false)]
        public int CommentId { get; set; }
        public string UserId { get; set; }
        public int RecipeId { get; set; }
        public string Content { get; set; 
        }
        [Required(ErrorMessage = "Ocena jest wymagana!")]
        [Range(1, 5, ErrorMessage = "Ocena jest wymagana!")]
        public int Rating { get; set; }
        public DateTime Date { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Recipe Recipe { get; set; }
    }
}
