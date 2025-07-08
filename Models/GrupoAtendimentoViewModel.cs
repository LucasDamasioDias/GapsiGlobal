namespace GapsiMVC.ViewModels
{
    public class GrupoAtendimentoViewModel
    {
        public string Nome { get; set; }
        public string Descricao { get; set; } 
        public string ImagemUrl { get; set; } 

        public bool TemAgendaAberta { get; set; }
        public string HorarioFormatado { get; set; } 
    }
}