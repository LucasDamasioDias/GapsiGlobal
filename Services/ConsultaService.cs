using GapsiMVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GapsiMVC.Models;

namespace GapsiMVC.Services
{
    public class ConsultaService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConsultaService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> AgendarConsulta(string pacienteId, int grupoId, DateTime data)
        {
            var paciente = await _userManager.FindByIdAsync(pacienteId);
            if (paciente == null) return false;

            var isPaciente = await _userManager.IsInRoleAsync(paciente, "Paciente");
            if (!isPaciente) return false;

            var grupo = await _context.Grupos.FindAsync(grupoId);
            if (grupo == null) return false;

            int pacientesNoGrupo = await _context.Consultas
                .CountAsync(c => c.GrupoId == grupoId && c.Status != "Cancelada");

            if (pacientesNoGrupo >= 8) return false;

            var consulta = new Consulta
            {
                PacienteId = pacienteId,
                GrupoId = grupoId,
                Data = data,
                Status = "Agendada"
            };

            _context.Consultas.Add(consulta);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelarConsulta(string usuarioId, int consultaId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId);
            var consulta = await _context.Consultas.FindAsync(consultaId);

            if (consulta == null) return false;

            if (await _userManager.IsInRoleAsync(usuario, "Paciente") && consulta.PacienteId != usuarioId)
                return false; 

            consulta.Status = "Cancelada";

            if (await _userManager.IsInRoleAsync(usuario, "Paciente"))
            {
                var paciente = await _context.Users.FindAsync(usuarioId);
                paciente.Creditos += 1;
                _context.Users.Update(paciente);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
