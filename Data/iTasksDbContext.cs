using System.Data.Entity;
using iTasks.Models;

namespace iTasks.Data
{
    public class iTasksDbContext : DbContext
    {
        public iTasksDbContext() : base("name=iTasksDbContext")
        {
        }

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Programador> Programadores { get; set; }
        public DbSet<Gestor> Gestores { get; set; }
        public DbSet<Tarefa> Tarefas { get; set; }
        public DbSet<TipoTarefa> TiposTarefa { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configurar relações e heranças
            modelBuilder.Entity<Programador>()
                .HasRequired(p => p.Gestor)
                .WithMany(g => g.Programadores)
                .HasForeignKey(p => p.IdGestor)
                .WillCascadeOnDelete(false);
            
            modelBuilder.Entity<Tarefa>()
                .HasRequired(t => t.Programador)
                .WithMany(p => p.Tarefas)
                .HasForeignKey(t => t.IdProgramador)
                .WillCascadeOnDelete(false);
            
            modelBuilder.Entity<Tarefa>()
                .HasRequired(t => t.Gestor)
                .WithMany(g => g.Tarefas)
                .HasForeignKey(t => t.IdGestor)
                .WillCascadeOnDelete(false);
            
            modelBuilder.Entity<Tarefa>()
                .HasRequired(t => t.TipoTarefa)
                .WithMany()
                .HasForeignKey(t => t.IdTipoTarefa)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Utilizador>()
                .Map<Programador>(m => m.Requires("Discriminator").HasValue("Programador"))
                .Map<Gestor>(m => m.Requires("Discriminator").HasValue("Gestor"));

            base.OnModelCreating(modelBuilder);
        }
    }
}