using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GapsiMVC.Models;

namespace GapsiMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<MensagemBroadcast> MensagensBroadcast { get; set; }
        public DbSet<GrupoMensagemBroadcast> GrupoMensagensBroadcast { get; set; }
        public DbSet<UsuarioMensagemBroadcast> UsuarioMensagensBroadcast { get; set; }
        public DbSet<HorarioGrupo> HorariosGrupos { get; set; }
        public DbSet<ComprovantePagamento> ComprovantesPagamento { get; set; }
        public DbSet<Configuracao> Configuracoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "role-adm", Name = "ADM", NormalizedName = "ADM" },
                new IdentityRole { Id = "role-psicologo", Name = "Psicologo", NormalizedName = "PSICOLOGO" },
                new IdentityRole { Id = "role-paciente", Name = "Paciente", NormalizedName = "PACIENTE" }
            );
        
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Grupos)
                .WithMany(g => g.Usuarios)
                .UsingEntity<Dictionary<string, object>>(
                    "GrupoUsuarios",
                    j => j.HasOne<Grupo>().WithMany().HasForeignKey("GruposId").OnDelete(DeleteBehavior.Cascade), // Se um grupo é deletado, as associações são removidas
                    j => j.HasOne<ApplicationUser>().WithMany().HasForeignKey("UsuariosId").OnDelete(DeleteBehavior.Restrict), // <<< ALTERADO AQUI: Impede deletar usuário se estiver em grupos
                    j =>
                    {
                        j.HasKey("UsuariosId", "GruposId");
                        j.ToTable("GrupoUsuarios");
                    }
                );

            modelBuilder.Entity<GrupoMensagemBroadcast>()
                .HasKey(gmb => new { gmb.MensagemBroadcastId, gmb.GrupoId });

            modelBuilder.Entity<GrupoMensagemBroadcast>()
                .HasOne(gmb => gmb.MensagemBroadcast)
                .WithMany(mb => mb.GruposDestinatarios)
                .HasForeignKey(gmb => gmb.MensagemBroadcastId)
                .OnDelete(DeleteBehavior.Cascade); 
            modelBuilder.Entity<GrupoMensagemBroadcast>()
                .HasOne(gmb => gmb.Grupo)
                .WithMany()
                .HasForeignKey(gmb => gmb.GrupoId)
                .OnDelete(DeleteBehavior.Cascade); 
                 
            modelBuilder.Entity<UsuarioMensagemBroadcast>()
               .HasKey(umb => new { umb.MensagemBroadcastId, umb.UsuarioId });

            modelBuilder.Entity<UsuarioMensagemBroadcast>()
                .HasOne(umb => umb.MensagemBroadcast)
                .WithMany(mb => mb.DestinatariosUsuarios)
                .HasForeignKey(umb => umb.MensagemBroadcastId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<UsuarioMensagemBroadcast>()
                .HasOne(umb => umb.Usuario)
                .WithMany() 
                .HasForeignKey(umb => umb.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HorarioGrupo>()
        .HasIndex(hg => new { hg.GrupoId, hg.DiaDaSemana, hg.Hora })
        .IsUnique();

            modelBuilder.Entity<HorarioGrupo>()
                .HasOne(hg => hg.Grupo)
                .WithMany(g => g.HorariosDefinidos)
                .HasForeignKey(hg => hg.GrupoId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Grupo)
                .WithMany(g => g.Consultas) 
                .HasForeignKey(c => c.GrupoId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}