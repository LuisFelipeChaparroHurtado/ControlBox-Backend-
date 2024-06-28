using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ControlBoxPruebaTecnica.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Configurar para autoincremento
        public int Id { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Password { get; set; }
        public string? Token { get; set; }

        public int? RoleId { get; set; }

        [ForeignKey("RoleId")] // Corregir la ForeignKey aquí si apunta a RoleId
        public Role? Role { get; set; }

        [Required]
        public string Email { get; set; }

        public ICollection<Review?> Reviews { get; set; } = new List<Review?>();
    }
}
