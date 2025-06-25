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
    public partial class frmConsultaTarefasEmCurso : Form
    {
        public frmConsultaTarefasEmCurso()
        {
            InitializeComponent();
            Load += frmConsultaTarefasEmCurso_Load;
            btFechar.Click += (s, e) => this.Close();
        }

        private void frmConsultaTarefasEmCurso_Load(object sender, EventArgs e)
        {
            // Permitir acesso apenas se o utilizador for um Gestor
            if (!(SessionManager.CurrentUser is Gestor gestor))
            {
                MessageBox.Show("Apenas gestores podem aceder a esta listagem.");
                Close();
                return;
            }
            using (var db = new iTasksDbContext())
            {
                var tarefas = db.Tarefas
                    .Include(t => t.Programador)
                    .Where(t => t.IdGestor == gestor.Id && t.EstadoAtual != CurrentStatus.Done)
                    .Select(t => new
                    {
                        Programador = t.Programador.Nome,
                        t.Descricao,
                        TipoTarefa = t.TipoTarefa.Nome,
                        t.OrdemExecucao,
                        t.StoryPoints,
                        t.EstadoAtual,
                        t.DataPrevistaInicio,
                        t.DataPrevistaFim,
                        t.DataRealInicio,
                        DiasRestantes = DbFunctions.DiffDays(DbFunctions.TruncateTime(DateTime.Now), t.DataPrevistaFim),
                        Atraso = DbFunctions.DiffDays(t.DataPrevistaFim, DbFunctions.TruncateTime(DateTime.Now)) > 0 ? DbFunctions.DiffDays(t.DataPrevistaFim, DbFunctions.TruncateTime(DateTime.Now)) : 0
                    })
                    .OrderBy(t => t.EstadoAtual)
                    .ThenBy(t => t.Programador)
                    .ToList();
                gvTarefasEmCurso.DataSource = tarefas;
            }
        }
    }
}
