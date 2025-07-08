using Microsoft.AspNetCore.Mvc;

public class ContratoController : Controller
{
    public IActionResult Index()
    {
        return View("Contrato"); 
    }
}
