using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GapsiMVC.ViewModels
{
    public class EnviarLinkViewModel
    {
        [Required(ErrorMessage = "Selecione o grupo.")]
        [Display(Name = "Para o Grupo")]
        public int GrupoId { get; set; }

        [Required(ErrorMessage = "Selecione a data da consulta.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data da Consulta Referente ao Link")]
        public DateTime DataConsulta { get; set; } = DateTime.Today; 

        [Required(ErrorMessage = "O link da reunião é obrigatório.")]
        [Url(ErrorMessage = "Por favor, insira uma URL válida para o link da reunião.")]
        [Display(Name = "Insira aqui o link da reunião:")]
        public string LinkReuniao { get; set; }
        public List<SelectListItem> TodosOsGrupos { get; set; } = new List<SelectListItem>();
    }
}