using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTasks.Data;
using iTasks.Models;

namespace iTasks
{
    public partial class frmGereTiposTarefas : Form
    {
        public frmGereTiposTarefas()
        {
            InitializeComponent();
            Load += frmGereTiposTarefas_Load;
            FormClosed += frmFormClosed;
            // Create
            btNovo.Click += btNovo_Click;
            // Read
            lstLista.SelectedIndexChanged += lstLista_SelectedIndexChanged;
            //Update
            btGravar.Click += btGravar_Click;
            // Delete
            btEliminar.Click += btEliminar_Click;
        }

        private void frmGereTiposTarefas_Load(object sender, EventArgs e)
        {
            LoadTiposTarefa();
        }

        private void frmFormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void LoadTiposTarefa()
        {
            using (var db = new iTasksDbContext())
            {
                var tipos = db.TiposTarefa.ToList();
                lstLista.DataSource = tipos;
                lstLista.DisplayMember = "Nome";
                lstLista.ValueMember = "Id";
            }
            txtId.Text = "";
            txtDesc.Text = "";
        }

        private void btNovo_Click(object sender, EventArgs e)
        {
            txtId.Text = "";
            txtDesc.Text = "";
        }

        private void lstLista_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstLista.SelectedItem is TipoTarefa tipo)
            {
                txtId.Text = tipo.Id.ToString();
                txtDesc.Text = tipo.Nome;
            }
        }

        private void btGravar_Click(object sender, EventArgs e)
        {
            using (var db = new iTasksDbContext())
            {
                int.TryParse(txtId.Text, out int id);
                TipoTarefa tipo = db.TiposTarefa.Find(id) ?? new TipoTarefa();
                tipo.Nome = txtDesc.Text.Trim();
                // Validar se já existe um tipo de tarefa com o mesmo nome
                bool exists = db.TiposTarefa.Any(t => t.Nome == tipo.Nome && t.Id != tipo.Id);
                if (exists)
                {
                    MessageBox.Show("Já existe um tipo de tarefa com este nome.");
                    return;
                }
                if (tipo.Id == 0) db.TiposTarefa.Add(tipo);
                db.SaveChanges();
                MessageBox.Show("Tipo de tarefa gravado com sucesso.");
                LoadTiposTarefa();
            }
        }

        private void btEliminar_Click(object sender, EventArgs e)
        {
            if (lstLista.SelectedItem is TipoTarefa tipo)
            {
                var confirm = MessageBox.Show($"Eliminar tipo de tarefa '{tipo.Nome}'?", "Confirmação", MessageBoxButtons.YesNo);
                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        using (var db = new iTasksDbContext())
                        {
                            var tipoDb = db.TiposTarefa.Find(tipo.Id);
                            if (tipoDb != null)
                            {
                                db.TiposTarefa.Remove(tipoDb);
                                db.SaveChanges();
                                MessageBox.Show("Tipo de tarefa eliminado com sucesso.");
                                LoadTiposTarefa();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao eliminar tipo de tarefa: " + ex.Message);
                    }
                }
            }
        }
    }
}
