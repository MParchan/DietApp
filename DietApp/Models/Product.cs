using MessagePack;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace DietApp.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Nazwa produktu jest wymagana!")]
        [MaxLength(50, ErrorMessage = "Nazwa może mieć maksymalnie 50 znaków")]
        [Display(Name = "Nazwa produktu")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Wartość energetyczna w 100g produktu jest wymagana!")]
        [Range(0, 900, ErrorMessage = "Nieprawidłowa liczba!")]
        [RegularExpression("([0-9][0-9]*)", ErrorMessage = "Wpisana wartość musi być liczbą")]
        [Display(Name = "Wartość energetyczna w 100g produktu")]
        public double KcalPer100 { get; set; }
        
        [Required(ErrorMessage = "Tłuszcz w 100g produktu jest wymagany!")]
        [Range(0, 100, ErrorMessage = "Nieprawidłowa liczba!")]
        [RegularExpression("([0-9][0-9]*)", ErrorMessage ="Wpisana wartość musi być liczbą")]
        [Display(Name = "Tłuszcz w 100g produktu")]
        public double FatPer100 { get; set; }

        [Required(ErrorMessage = "Węglowodany w 100g produktu są wymagane!")]
        [Range(0, 100, ErrorMessage = "Nieprawidłowa liczba!")]
        [RegularExpression("([0-9][0-9]*)", ErrorMessage = "Wpisana wartość musi być liczbą")]
        [Display(Name = "Węglowodany w 100g produktu")]
        public double CarboPer100 { get; set; }

        [Required(ErrorMessage = "Białko w 100g produktu jest wymagane!")]
        [Range(0, 100, ErrorMessage = "Nieprawidłowa liczba!")]
        [RegularExpression("([0-9][0-9]*)", ErrorMessage = "Wpisana wartość musi być liczbą")]
        [Display(Name = "Białko w 100g produktu")]
        public double ProteinPer100 { get; set; }

        public string ImageName { get; set; }

        [NotMapped]
        [Display(Name = "Zdjęcie produktu")]
        public IFormFile Image { get; set; }
    }
}
