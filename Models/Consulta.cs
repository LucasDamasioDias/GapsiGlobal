using System.ComponentModel.DataAnnotations;
using GapsiMVC.Models;
public class Consulta
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string Status { get; set; } = "Agendada"; 

    [Required]
    public int GrupoId { get; set; }
    public Grupo Grupo { get; set; }

    [Required]
    public string PacienteId { get; set; }
    public ApplicationUser Paciente { get; set; }
}
