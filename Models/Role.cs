using System.ComponentModel.DataAnnotations;

namespace ControlBoxPruebaTecnica.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "NameRole")]
        [Required(ErrorMessage = "Name Role Type is required")]
        public string NameRole { get; set; }

        public ICollection<User?> Users { get; set; } = new List<User?>();
    }
}
