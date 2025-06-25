using System;

namespace iTasks.Models
{
    public class TipoTarefa
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }

    public class Tarefa
    {
        public int Id { get; set; }
        public int IdGestor { get; set; }
        public virtual Gestor Gestor { get; set; }
        public int IdProgramador { get; set; }
        public virtual Programador Programador { get; set; }
        public int OrdemExecucao { get; set; }
        public string Descricao { get; set; }
        public DateTime DataPrevistaInicio { get; set; }
        public DateTime DataPrevistaFim { get; set; }
        public int IdTipoTarefa { get; set; }
        public virtual TipoTarefa TipoTarefa { get; set; }
        public int StoryPoints { get; set; }
        public DateTime? DataRealInicio { get; set; }
        public DateTime? DataRealFim { get; set; }
        public DateTime DataCriacao { get; set; }
        public CurrentStatus EstadoAtual { get; set; }
    }
}