namespace GapsiMVC.ViewModels
{
    public class PacienteViewModel
    {
        public string Id { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public decimal Creditos { get; set; }
        public List<string> Grupos { get; set; }
    }
}
