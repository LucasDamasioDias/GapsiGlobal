using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class Role : IdentityRole
{
    [Required]
    public string Description { get; set; } 
}
