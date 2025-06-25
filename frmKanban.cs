using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTasks.Models;
using iTasks.Data;
using System.Data.Entity;

namespace iTasks
{
    public partial class frmKanban : Form
    {
        public frmKanban()
        {
            InitializeComponent();
            Load += frmKanban_Load;
            FormClosed += frmKanban_FormClosed;
            btLogout.Click += btLogout_Click;
            btSetDoing.Click += btSetDoing_Click;
            btSetDone.Click += btSetDone_Click;
            btSetTodo.Click += btSetTodo_Click;
            btNova.Click += btNova_Click;
            btPrevisao.Click += btPrevisao_Click;
            // Detalhes das tarefas
            lstTodo.DoubleClick += lstTask_DoubleClick;
            lstDoing.DoubleClick += lstTask_DoubleClick;
            lstDone.DoubleClick += lstTask_DoubleClick;
            // Toolstrip menu items
            gerirUtilizadoresToolStripMenuItem.Click += gerirUtilizadoresToolStripMenuItem_Click;
            gerirTiposDeTarefasToolStripMenuItem.Click += gerirTiposDeTarefasToolStripMenuItem_Click;
            tarefasTerminadasToolStripMenuItem.Click += tarefasTerminadasToolStripMenuItem_Click;
            tarefasEmCursoToolStripMenuItem.Click += tarefasEmCursoToolStripMenuItem_Click;
            sairToolStripMenuItem.Click += sairToolStripMenuItem_Click;
        }

        private void frmKanban_Load(object sender, EventArgs e)
        {
            // Mostrar o nome do utilizador atual
            if (SessionManager.CurrentUser != null)
            {
                label1.Text = $"Bem vindo: {SessionManager.CurrentUser.Nome}";
            }
            else
            {
                label1.Text = "Bem vindo: <Desconhecido>";
            }

            // Adaptar o UI conforme o tipo de utilizador
            if (SessionManager.CurrentUser is Gestor) // Gestor
            {
                // Botões principais
                btSetDoing.Enabled = false;
                btSetDone.Enabled = false;
                btSetTodo.Enabled = false;
                btNova.Enabled = true;
                btPrevisao.Enabled = true;
                // Toolstrip menu items
                gerirUtilizadoresToolStripMenuItem.Enabled = true;
                gerirTiposDeTarefasToolStripMenuItem.Enabled = true;
                tarefasEmCursoToolStripMenuItem.Enabled = true;
                exportarParaCSVToolStripMenuItem.Enabled = true;
            }
            else if (SessionManager.CurrentUser is Programador) // Programador
            {
                // Botões principais
                btSetDoing.Enabled = true;
                btSetDone.Enabled = true;
                btSetTodo.Enabled = true;
                btNova.Enabled = false;
                btPrevisao.Enabled = false;
                // Toolstrip menu items
                gerirUtilizadoresToolStripMenuItem.Enabled = false;
                gerirTiposDeTarefasToolStripMenuItem.Enabled = false;
                tarefasEmCursoToolStripMenuItem.Enabled = false;
                exportarParaCSVToolStripMenuItem.Enabled = false;
            }
            LoadKanbanTasks();
        }

        private void LoadKanbanTasks()
        {
            using (var db = new iTasksDbContext())
            {
                var tarefas = db.Tarefas
                    .Include(t => t.Programador)
                    .Include(t => t.TipoTarefa)
                    .ToList();

                var todo = tarefas.Where(t => t.EstadoAtual == CurrentStatus.ToDo).ToList();
                var doing = tarefas.Where(t => t.EstadoAtual == CurrentStatus.Doing).ToList();
                var done = tarefas.Where(t => t.EstadoAtual == CurrentStatus.Done).ToList();

                lstTodo.DataSource = null;
                lstDoing.DataSource = null;
                lstDone.DataSource = null;

                lstTodo.DataSource = todo;
                lstDoing.DataSource = doing;
                lstDone.DataSource = done;

                lstTodo.DisplayMember = "Descricao";
                lstDoing.DisplayMember = "Descricao";
                lstDone.DisplayMember = "Descricao";

                lstTodo.ValueMember = "Id";
                lstDoing.ValueMember = "Id";
                lstDone.ValueMember = "Id";
            }
        }

        private void frmKanban_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void btLogout_Click(object sender, EventArgs e)
        {
            SessionManager.CurrentUser = null;
            frmLogin loginForm = new frmLogin();
            loginForm.Show();
            this.Hide();
        }

        private void btSetDoing_Click(object sender, EventArgs e)
        {
            // Move selected ToDo task to Doing (if allowed)
            if (!(SessionManager.CurrentUser is Programador prog))
            {
                MessageBox.Show("Apenas programadores podem executar tarefas.");
                return;
            }
            if (!(lstTodo.SelectedItem is Tarefa tarefa))
            {
                MessageBox.Show("Selecione uma tarefa na lista ToDo.");
                return;
            }
            if (tarefa.IdProgramador != prog.Id)
            {
                MessageBox.Show("Só pode executar as suas próprias tarefas.");
                return;
            }
            using (var db = new iTasksDbContext())
            {
                var tarefaDb = db.Tarefas.Find(tarefa.Id);
                if (tarefaDb.EstadoAtual != CurrentStatus.ToDo)
                {
                    MessageBox.Show("A tarefa não está no estado ToDo.");
                    return;
                }
                // OrdemExecucao: só pode mover a de menor ordem entre as suas ToDo
                var minOrdem = db.Tarefas.Where(t => t.IdProgramador == prog.Id && t.EstadoAtual == CurrentStatus.ToDo).Min(t => (int?)t.OrdemExecucao) ?? tarefaDb.OrdemExecucao;
                if (tarefaDb.OrdemExecucao != minOrdem)
                {
                    MessageBox.Show("Deve executar as tarefas pela ordem definida pelo gestor.");
                    return;
                }
                // Máximo 2 Doing
                int doingCount = db.Tarefas.Count(t => t.IdProgramador == prog.Id && t.EstadoAtual == CurrentStatus.Doing);
                if (doingCount >= 2)
                {
                    MessageBox.Show("Só pode ter 2 tarefas em execução (Doing) ao mesmo tempo.");
                    return;
                }
                tarefaDb.EstadoAtual = CurrentStatus.Doing;
                tarefaDb.DataRealInicio = DateTime.Now;
                db.SaveChanges();
            }
            LoadKanbanTasks();
        }

        private void btSetDone_Click(object sender, EventArgs e)
        {
            // Move selected Doing task to Done (if allowed)
            if (!(SessionManager.CurrentUser is Programador prog))
            {
                MessageBox.Show("Apenas programadores podem terminar tarefas.");
                return;
            }
            if (!(lstDoing.SelectedItem is Tarefa tarefa))
            {
                MessageBox.Show("Selecione uma tarefa na lista Doing.");
                return;
            }
            if (tarefa.IdProgramador != prog.Id)
            {
                MessageBox.Show("Só pode terminar as suas próprias tarefas.");
                return;
            }
            using (var db = new iTasksDbContext())
            {
                var tarefaDb = db.Tarefas.Find(tarefa.Id);
                if (tarefaDb.EstadoAtual != CurrentStatus.Doing)
                {
                    MessageBox.Show("A tarefa não está no estado Doing.");
                    return;
                }
                // OrdemExecucao: só pode terminar a de menor ordem entre as suas Doing
                var minOrdem = db.Tarefas.Where(t => t.IdProgramador == prog.Id && t.EstadoAtual == CurrentStatus.Doing).Min(t => (int?)t.OrdemExecucao) ?? tarefaDb.OrdemExecucao;
                if (tarefaDb.OrdemExecucao != minOrdem)
                {
                    MessageBox.Show("Deve terminar as tarefas pela ordem definida pelo gestor.");
                    return;
                }
                tarefaDb.EstadoAtual = CurrentStatus.Done;
                tarefaDb.DataRealFim = DateTime.Now;
                db.SaveChanges();
            }
            LoadKanbanTasks();
        }

        private void btSetTodo_Click(object sender, EventArgs e)
        {
            // Move selected Doing task back to ToDo (if allowed)
            if (!(SessionManager.CurrentUser is Programador prog))
            {
                MessageBox.Show("Apenas programadores podem reiniciar tarefas.");
                return;
            }
            if (!(lstDoing.SelectedItem is Tarefa tarefa))
            {
                MessageBox.Show("Selecione uma tarefa na lista Doing.");
                return;
            }
            if (tarefa.IdProgramador != prog.Id)
            {
                MessageBox.Show("Só pode reiniciar as suas próprias tarefas.");
                return;
            }
            using (var db = new iTasksDbContext())
            {
                var tarefaDb = db.Tarefas.Find(tarefa.Id);
                if (tarefaDb.EstadoAtual != CurrentStatus.Doing)
                {
                    MessageBox.Show("A tarefa não está no estado Doing.");
                    return;
                }
                tarefaDb.EstadoAtual = CurrentStatus.ToDo;
                tarefaDb.DataRealInicio = null;
                db.SaveChanges();
            }
            LoadKanbanTasks();
        }

        private void btNova_Click(object sender, EventArgs e)
        {
            if (!(SessionManager.CurrentUser is Gestor))
            {
                MessageBox.Show("Apenas gestores podem criar tarefas.");
                return;
            }
            frmDetalhesTarefa frm = new frmDetalhesTarefa();
            frm.FormClosed += (s, args) => { LoadKanbanTasks(); this.Show(); };
            frm.ShowDialog();
        }

        private void lstTask_DoubleClick(object sender, EventArgs e)
        {
            ListBox list = sender as ListBox;
            if (list?.SelectedItem is Tarefa tarefa)
            {
                frmDetalhesTarefa frm = new frmDetalhesTarefa(tarefa.Id);
                frm.FormClosed += (s, args) => { LoadKanbanTasks(); this.Show(); };
                frm.ShowDialog();
            }
        }

        private void gerirUtilizadoresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmGereUtilizadores frm = new frmGereUtilizadores();
            frm.FormClosed += (s, args) => { this.Show(); };
            this.Hide();
            frm.Show();
        }

        private void gerirTiposDeTarefasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmGereTiposTarefas frm = new frmGereTiposTarefas();
            frm.FormClosed += (s, args) => { this.Show(); };
            this.Hide();
            frm.Show();
        }

        private void tarefasTerminadasToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            frmConsultarTarefasConcluidas frm = new frmConsultarTarefasConcluidas();
            frm.FormClosed += (s, args) => { this.Show(); };
            this.Hide();
            frm.Show();
        }

        private void tarefasEmCursoToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            frmConsultaTarefasEmCurso frm = new frmConsultaTarefasEmCurso();
            frm.FormClosed += (s, args) => { this.Show(); };
            this.Hide();
            frm.Show();
        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show("Tem a certeza que deseja sair do programa?", "Sair", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btPrevisao_Click(object sender, EventArgs e)
        {
            if (!(SessionManager.CurrentUser is Gestor gestor))
            {
                MessageBox.Show("Apenas gestores podem ver a previsão de conclusão.");
                return;
            }
            using (var db = new iTasksDbContext())
            {
                // ToDo tasks para este manager
                var todos = db.Tarefas.Where(t => t.IdGestor == gestor.Id && t.EstadoAtual == CurrentStatus.ToDo).ToList();
                // Done tasks para este manager
                var done = db.Tarefas.Where(t => t.IdGestor == gestor.Id && t.EstadoAtual == CurrentStatus.Done && t.DataRealInicio != null && t.DataRealFim != null).ToList();
                if (!todos.Any())
                {
                    MessageBox.Show("Não existem tarefas ToDo para previsão.");
                    return;
                }
                if (!done.Any())
                {
                    MessageBox.Show("Não existem tarefas concluídas para calcular médias.");
                    return;
                }
                // Dicionário: StoryPoints -> Lista de tempos de execução (em horas)
                var spToTimes = done
                    .GroupBy(t => t.StoryPoints)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(t => (t.DataRealFim.Value - t.DataRealInicio.Value).TotalHours).ToList()
                    );
                double totalEstimatedHours = 0;
                foreach (var tarefa in todos)
                {
                    int sp = tarefa.StoryPoints;
                    double avgTime;
                    if (spToTimes.ContainsKey(sp))
                    {
                        avgTime = spToTimes[sp].Average();
                    }
                    else
                    {
                        // Encontrar o StoryPoint mais próximo com lista de tempos associada
                        int? closest = spToTimes.Keys.OrderBy(x => Math.Abs(x - sp)).FirstOrDefault();
                        avgTime = closest.HasValue ? spToTimes[closest.Value].Average() : 0;
                    }
                    totalEstimatedHours += avgTime;
                }
                int totalEstimatedDays = totalEstimatedHours > 0.1 ? (int)Math.Ceiling(totalEstimatedHours / 24.0) : 0;
                MessageBox.Show($"Tempo estimado para concluir todas as tarefas ToDo: {totalEstimatedHours:F1} horas (~{totalEstimatedDays} dias)", "Previsão de Conclusão", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
