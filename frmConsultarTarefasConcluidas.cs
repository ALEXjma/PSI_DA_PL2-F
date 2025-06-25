using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Entity;
using iTasks.Data;
using iTasks.Models;

namespace iTasks
{
    public partial class frmConsultarTarefasConcluidas : Form
    {
        public frmConsultarTarefasConcluidas()
        {
            InitializeComponent();
            Load += frmConsultarTarefasConcluidas_Load;
            btFechar.Click += (s, e) => this.Close();
        }

        private void frmConsultarTarefasConcluidas_Load(object sender, EventArgs e)
        {
            using (var db = new iTasksDbContext())
            {
                if (SessionManager.CurrentUser is Gestor gestor)
                {
                    // Manager: Mostra todas as tarefas concluídas que ele gere
                    var tarefas = db.Tarefas
                        .Include(t => t.TipoTarefa)
                        .Include(t => t.Programador)
                        .Where(t => t.IdGestor == gestor.Id && t.EstadoAtual == CurrentStatus.Done)
                        .Select(t => new
                        {
                            Programador = t.Programador.Nome,
                            t.Descricao,
                            TipoTarefa = t.TipoTarefa.Nome,
                            t.OrdemExecucao,
                            t.StoryPoints,
                            t.DataPrevistaInicio,
                            t.DataPrevistaFim,
                            t.DataRealInicio,
                            t.DataRealFim,
                            DiasPrevistos = DbFunctions.DiffDays(t.DataPrevistaInicio, t.DataPrevistaFim),
                            DiasExecucao = t.DataRealFim != null && t.DataRealInicio != null ? DbFunctions.DiffDays(t.DataRealInicio, t.DataRealFim) : null
                        })
                        .ToList();
                    gvTarefasConcluidas.DataSource = tarefas;
                }
                else if (SessionManager.CurrentUser is Programador prog)
                {
                    // Programador: Mostra apenas as tarefas concluídas atribuídas a ele
                    var tarefas = db.Tarefas
                        .Include(t => t.TipoTarefa)
                        .Where(t => t.IdProgramador == prog.Id && t.EstadoAtual == CurrentStatus.Done)
                        .Select(t => new
                        {
                            t.Descricao,
                            TipoTarefa = t.TipoTarefa.Nome,
                            t.OrdemExecucao,
                            t.StoryPoints,
                            t.DataPrevistaInicio,
                            t.DataPrevistaFim,
                            t.DataRealInicio,
                            t.DataRealFim,
                            DiasExecucao = t.DataRealFim != null && t.DataRealInicio != null ? DbFunctions.DiffDays(t.DataRealInicio, t.DataRealFim) : null
                        })
                        .ToList();
                    gvTarefasConcluidas.DataSource = tarefas;
                }
            }
        }
    }
}
