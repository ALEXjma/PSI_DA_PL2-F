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
    public partial class frmDetalhesTarefa : Form
    {
        private int? tarefaId;
        private bool isReadOnly;
        public frmDetalhesTarefa() : this(null) { }
        public frmDetalhesTarefa(int? tarefaId)
        {
            InitializeComponent();
            this.tarefaId = tarefaId;
            Load += frmDetalhesTarefa_Load;
            btGravar.Click += btGravar_Click;
            btEliminar.Click += btEliminar_Click;
            btFechar.Click += (s, e) => this.Close();
            // Permitir fechar o formulário com a tecla ESC
            this.KeyPreview = true;
            KeyDown += frmDetalhesTarefa_KeyDown;
        }

        private void frmDetalhesTarefa_Load(object sender, EventArgs e)
        {
            // Definir modo de apenas leitura se o utilizador não for gestor
            isReadOnly = !(SessionManager.CurrentUser is Gestor);
            using (var db = new iTasksDbContext())
            {
                // Load Programadores
                var allProgs = db.Programadores.ToList();
                List<Programador> progsToShow;
                if (!isReadOnly && SessionManager.CurrentUser is Gestor gestor)
                {
                    progsToShow = allProgs.Where(p => p.IdGestor == gestor.Id).ToList();
                }
                else
                {
                    progsToShow = allProgs;
                }
                cbProgramador.DataSource = progsToShow;
                cbProgramador.DisplayMember = "Nome";
                cbProgramador.ValueMember = "Id";
                // Load Tipos de Tarefa
                cbTipoTarefa.DataSource = db.TiposTarefa.ToList();
                cbTipoTarefa.DisplayMember = "Nome";
                cbTipoTarefa.ValueMember = "Id";

                if (tarefaId.HasValue)
                {
                    // Editar tarefa existente
                    var tarefa = db.Tarefas.Include(t => t.Programador).Include(t => t.TipoTarefa).FirstOrDefault(t => t.Id == tarefaId.Value);
                    if (tarefa == null)
                    {
                        MessageBox.Show("Tarefa não encontrada.");
                        Close();
                        return;
                    }
                    if (!isReadOnly && SessionManager.CurrentUser is Gestor _gestor && tarefa.IdGestor != _gestor.Id) {
                        // Definir como apenas leitura se a tarefa não pertence ao gestor atual
                        isReadOnly = true;
                        cbProgramador.DataSource = allProgs;
                    }
                    txtId.Text = tarefa.Id.ToString();
                    txtDesc.Text = tarefa.Descricao;
                    cbProgramador.SelectedValue = tarefa.IdProgramador;
                    txtOrdem.Text = tarefa.OrdemExecucao.ToString();
                    cbTipoTarefa.SelectedValue = tarefa.IdTipoTarefa;
                    dtInicio.Value = tarefa.DataPrevistaInicio;
                    dtFim.Value = tarefa.DataPrevistaFim;
                    txtStoryPoints.Text = tarefa.StoryPoints.ToString();
                    txtEstado.Text = tarefa.EstadoAtual.ToString();
                    txtDataRealini.Text = tarefa.DataRealInicio?.ToString("g") ?? "";
                    txtdataRealFim.Text = tarefa.DataRealFim?.ToString("g") ?? "";
                    txtDataCriacao.Text = tarefa.DataCriacao.ToString("g");
                }
                else
                {
                    // Criar nova tarefa
                    txtId.Text = "";
                    txtDesc.Text = "";
                    txtOrdem.Text = "";
                    txtStoryPoints.Text = "";
                    txtEstado.Text = "ToDo";
                    txtDataRealini.Text = "";
                    txtdataRealFim.Text = "";
                    txtDataCriacao.Text = DateTime.Now.ToString("g");
                    if (cbProgramador.Items.Count > 0) cbProgramador.SelectedIndex = 0;
                    if (cbTipoTarefa.Items.Count > 0) cbTipoTarefa.SelectedIndex = 0;
                    dtInicio.Value = DateTime.Now;
                    dtFim.Value = DateTime.Now.AddDays(1);
                }
            }
            SetReadOnlyMode(isReadOnly);
        }

        private void SetReadOnlyMode(bool readOnly)
        {
            txtDesc.ReadOnly = readOnly;
            cbProgramador.Enabled = !readOnly;
            txtOrdem.ReadOnly = readOnly;
            cbTipoTarefa.Enabled = !readOnly;
            dtInicio.Enabled = !readOnly;
            dtFim.Enabled = !readOnly;
            txtStoryPoints.ReadOnly = readOnly;
            btGravar.Enabled = !readOnly;
            btEliminar.Enabled = !readOnly;
        }

        private void btGravar_Click(object sender, EventArgs e)
        {
            if (!(SessionManager.CurrentUser is Gestor gestor))
            {
                MessageBox.Show("Apenas gestores podem gravar tarefas.");
                return;
            }
            using (var db = new iTasksDbContext())
            {
                Tarefa tarefa;
                if (tarefaId.HasValue)
                {
                    tarefa = db.Tarefas.Find(tarefaId.Value);
                    if (tarefa == null)
                    {
                        MessageBox.Show("Tarefa não encontrada.");
                        return;
                    }
                }
                else
                {
                    tarefa = new Tarefa();
                    tarefa.EstadoAtual = CurrentStatus.ToDo;
                    tarefa.DataCriacao = DateTime.Now;
                    tarefa.IdGestor = gestor.Id;
                }
                tarefa.Descricao = txtDesc.Text.Trim();
                tarefa.IdProgramador = (int)cbProgramador.SelectedValue;
                int ordem;
                if (int.TryParse(txtOrdem.Text, out ordem) && ordem > 0)
                {
                    tarefa.OrdemExecucao = ordem;
                }
                else
                {
                    // Usar a proxima ordem de execução disponível para o programador selecionado por padrão
                    int progId = (int)cbProgramador.SelectedValue;
                    int maxOrdem = db.Tarefas.Where(t => t.IdProgramador == progId).Select(t => (int?)t.OrdemExecucao).Max() ?? 0;
                    tarefa.OrdemExecucao = maxOrdem + 1;
                }
                tarefa.IdTipoTarefa = (int)cbTipoTarefa.SelectedValue;
                tarefa.DataPrevistaInicio = dtInicio.Value;
                tarefa.DataPrevistaFim = dtFim.Value;
                tarefa.StoryPoints = int.TryParse(txtStoryPoints.Text, out int sp) ? sp : 1;
                // Validar se a ordem de execução já existe para o programador selecionado
                bool ordemExists = db.Tarefas.Any(t => t.IdProgramador == tarefa.IdProgramador && t.OrdemExecucao == tarefa.OrdemExecucao && t.Id != tarefa.Id);
                if (ordemExists)
                {
                    MessageBox.Show("Já existe uma tarefa com esta ordem para o programador selecionado.");
                    return;
                }
                if (!tarefaId.HasValue)
                    db.Tarefas.Add(tarefa);
                db.SaveChanges();
                MessageBox.Show("Tarefa gravada com sucesso.");
                this.Close();
            }
        }

        private void btEliminar_Click(object sender, EventArgs e)
        {
            if (!(SessionManager.CurrentUser is Gestor gestor))
            {
                MessageBox.Show("Apenas gestores podem eliminar tarefas.");
                return;
            }
            if (!tarefaId.HasValue)
            {
                MessageBox.Show("Nenhuma tarefa selecionada para eliminar.");
                return;
            }
            using (var db = new iTasksDbContext())
            {
                var tarefa = db.Tarefas.Find(tarefaId.Value);
                if (tarefa == null)
                {
                    MessageBox.Show("Tarefa não encontrada.");
                    return;
                }
                db.Tarefas.Remove(tarefa);
                db.SaveChanges();
                MessageBox.Show("Tarefa eliminada com sucesso.");
                this.Close();
            }
        }

        private void frmDetalhesTarefa_KeyDown(object sender, KeyEventArgs e)
        {
            // Permitir fechar o formulário com a tecla ESC
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
