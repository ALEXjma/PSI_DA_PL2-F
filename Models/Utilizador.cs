using System.Collections.Generic;

namespace iTasks.Models
{
    public class Utilizador
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Programador : Utilizador
    {
        public ExperienceLevel NivelExperiencia { get; set; }
        public int IdGestor { get; set; }
        public virtual Gestor Gestor { get; set; }
        public virtual ICollection<Tarefa> Tarefas { get; set; }
    }

    public class Gestor : Utilizador
    {
        public Department Departamento { get; set; }
        public bool GereUtilizadores { get; set; }
        public virtual ICollection<Programador> Programadores { get; set; }
        public virtual ICollection<Tarefa> Tarefas { get; set; }
    }
}