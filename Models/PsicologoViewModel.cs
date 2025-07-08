namespace GapsiMVC.ViewModels
{
    public class PsicologoViewModel
    {
        public string Id { get; set; } 
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public List<string> Grupos { get; set; } = new List<string>();
    }
}