using GapsiMVC.Data;
using GapsiMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GapsiGlobal.Controllers
{
    public class InvestimentoController : Controller
    {
        private readonly ApplicationDbContext  _context;

        public InvestimentoController (ApplicationDbContext context)
        {
             _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var config = await _context.Configuracoes.FindAsync("ValorConsulta");
            var valor = decimal.Parse(config?.Valor ?? "100.00", CultureInfo.InvariantCulture);

            var viewModel = new InvestimentoViewModel { ValorConsulta = valor };
           
            return View("Investimento", viewModel);
        }
    }
}