namespace GapsiMVC.ViewModels
{
    public class MensagemParaPacienteViewModel
    {
        public int Id { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataEnvio { get; set; }
        public string DataEnvioFormatada => DataEnvio.ToString("dd/MM/yyyy 'às' HH:mm");
        public string EnviadoPorNome { get; set; } 
        public string NomeGrupo { get; set; } 
    }

    public class PacienteVisualizarMensagensViewModel
    {
        public List<MensagemParaPacienteViewModel> Mensagens { get; set; } = new List<MensagemParaPacienteViewModel>();
    }
}