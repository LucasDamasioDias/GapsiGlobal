namespace GapsiMVC.ViewModels
{
    public class PerfilProfissionalViewModel
    {
        public string NomeCompleto { get; set; }
        public string CRP { get; set; }
        public string Biografia { get; set; }
        public string FotoUrl { get; set; }
    }

    public class QuemSomosViewModel
    {
        public PerfilProfissionalViewModel SociaBruna { get; set; }
        public PerfilProfissionalViewModel SociaGloria { get; set; } 
        public List<PerfilProfissionalViewModel> PsicologosParceiros { get; set; } = new List<PerfilProfissionalViewModel>();
    }
}