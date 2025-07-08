namespace GapsiMVC.ViewModels
{
    public class MensagemParaPsicologoViewModel 
    {
        public int Id { get; set; } 
        public string Conteudo { get; set; }
        public DateTime DataEnvio { get; set; }
        public string DataEnvioFormatada => DataEnvio.ToString("dd/MM/yyyy 'às' HH:mm");
        public string EnviadoPorNome { get; set; }
    }

    public class PsicologoVisualizarMensagensViewModel 
    {
        public List<MensagemParaPsicologoViewModel> MensagensRecebidas { get; set; } = new List<MensagemParaPsicologoViewModel>();
    }
}