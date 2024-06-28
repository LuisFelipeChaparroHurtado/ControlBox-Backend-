using System.ComponentModel.DataAnnotations;

namespace ControlBoxPruebaTecnica.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Titulo { get; set; }
        [Required]
        public string Autor { get; set; }
        [Required]
        public string Categoria { get; set; }
        [Required]
        [MaxLength]
        public string Resumen { get; set; }

        public ICollection<Review?> Reviews { get; set; } = new List<Review?>();
    }
}
