using GapsiMVC.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NomeCompleto { get; set; }
        public int Creditos { get; set; }

        [StringLength(20)]
        public string? CRP { get; set; }

        [StringLength(2000)]
        public string? Biografia { get; set; }

        [StringLength(500)]
        public string? FotoUrl { get; set; }
        public virtual ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();
    }
}