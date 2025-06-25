using iTasks.Data;
using iTasks.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iTasks
{
    public partial class frmGereUtilizadores : Form
    {
        public frmGereUtilizadores()
        {
            InitializeComponent();
            Load += frmGereUtilizadores_Load;
            FormClosed += frmGereUtilizadores_FormClosed;
            // Create
            btNovoGestor.Click += btNovoGestor_Click;
            btNovoProg.Click += btNovoProg_Click;
            // Read
            lstListaGestores.SelectedIndexChanged += lstListaGestores_SelectedIndexChanged;
            lstListaProgramadores.SelectedIndexChanged += lstListaProgramadores_SelectedIndexChanged;
            // Update
            btGravarGestor.Click += btGravarGestor_Click;
            btGravarProg.Click += btGravarProg_Click;
            // Delete
            btEliminarGestor.Click += btEliminarGestor_Click;
            btEliminarProg.Click += btEliminarProg_Click;
        }

        private void frmGereUtilizadores_Load(object sender, EventArgs e)
        {
            cbDepartamento.DataSource = Enum.GetValues(typeof(Department));
            cbNivelProg.DataSource = Enum.GetValues(typeof(ExperienceLevel));
            LoadUsers();
        }

        private void frmGereUtilizadores_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void LoadUsers()
        {
            using (var db = new iTasksDbContext())
            {
                var gestores = db.Gestores.ToList();
                lstListaGestores.DataSource = gestores;
                lstListaGestores.DisplayMember = "Nome";
                lstListaGestores.ValueMember = "Id";
                cbGestorProg.DataSource = gestores;
                cbGestorProg.DisplayMember = "Nome";
                cbGestorProg.ValueMember = "Id";
                var programadores = db.Programadores.ToList();
                lstListaProgramadores.DataSource = programadores;
                lstListaProgramadores.DisplayMember = "Nome";
                lstListaProgramadores.ValueMember = "Id";
            }
        }

        private void btNovoGestor_Click(object sender, EventArgs e)
        {
            using (var db = new iTasksDbContext())
            {
                int id = db.Utilizadores.Any() ? db.Utilizadores.Max(x => x.Id) : 0;
                id++;
                txtIdGestor.Text = id.ToString();
                txtNomeGestor.Clear();
                txtUsernameGestor.Clear();
                txtPasswordGestor.Clear();
                cbDepartamento.SelectedIndex = 0;
                chkGereUtilizadores.Checked = false;
            }
        }

        private void btNovoProg_Click(object sender, EventArgs e)
        {
            using (var db = new iTasksDbContext())
            {
                int id = db.Utilizadores.Any() ? db.Utilizadores.Max(x => x.Id) : 0;
                id++;
                txtIdProg.Text = id.ToString();
                txtNomeProg.Clear();
                txtUsernameProg.Clear();
                txtPasswordProg.Clear();
                cbNivelProg.SelectedIndex = 0;
                cbGestorProg.SelectedIndex = 0;
            }
        }

        private void lstListaGestores_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (var db = new iTasksDbContext())
            {
                txtNomeGestor.Text = lstListaGestores.Text;
                Gestor gestor = new Gestor();
                gestor = db.Gestores.FirstOrDefault(c => c.Nome == txtNomeGestor.Text);
                txtIdGestor.Text = gestor.Id.ToString();
                txtUsernameGestor.Text = gestor.Username.ToString();
                txtPasswordGestor.Text = gestor.Password.ToString();
                cbDepartamento.Text = gestor.Departamento.ToString();
                chkGereUtilizadores.Checked = gestor.GereUtilizadores ? true : false;
            }
        }

        private void lstListaProgramadores_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (var db = new iTasksDbContext())
            {
                txtNomeProg.Text = lstListaProgramadores.Text;
                Programador programador = new Programador();
                programador = db.Programadores.FirstOrDefault(c => c.Nome == txtNomeProg.Text);
                txtIdProg.Text = programador.Id.ToString();
                txtNomeProg.Text = programador.Nome.ToString();
                txtUsernameProg.Text = programador.Username.ToString();
                txtPasswordProg.Text = programador.Password.ToString();
                cbNivelProg.Text = programador.NivelExperiencia.ToString();

                Gestor gestor = new Gestor();
                gestor = db.Gestores.FirstOrDefault(c => c.Id == programador.IdGestor);
                cbGestorProg.Text = gestor.Nome.ToString();
            }
        }

        private void btGravarGestor_Click(object sender, EventArgs e)
        {
            using (var db = new iTasksDbContext())
            {
                int.TryParse(txtIdGestor.Text, out int id);
                Gestor g = db.Gestores.Find(id) ?? new Gestor();
                g.Nome = txtNomeGestor.Text.Trim();
                g.Username = txtUsernameGestor.Text.Trim();
                g.Password = txtPasswordGestor.Text;
                g.Departamento = (Department)cbDepartamento.SelectedItem;
                g.GereUtilizadores = chkGereUtilizadores.Checked;
                // Verificar se já existe um utilizador com o mesmo username
                bool exists = db.Utilizadores.Any(u => u.Username == g.Username && u.Id != g.Id);
                if (exists)
                {
                    MessageBox.Show("O username já existe.");
                    return;
                }
                if (g.Id == 0) db.Gestores.Add(g);
                db.SaveChanges();
                MessageBox.Show("Gestor gravado com sucesso.");
                LoadUsers();
            }
        }

        private void btGravarProg_Click(object sender, EventArgs e)
        {
            using (var db = new iTasksDbContext())
            {
                int.TryParse(txtIdProg.Text, out int id);
                Programador p = db.Programadores.Find(id) ?? new Programador();
                p.Nome = txtNomeProg.Text.Trim();
                p.Username = txtUsernameProg.Text.Trim();
                p.Password = txtPasswordProg.Text;
                p.NivelExperiencia = (ExperienceLevel)cbNivelProg.SelectedItem;
                p.IdGestor = (int)cbGestorProg.SelectedValue;
                // Verificar se já existe um utilizador com o mesmo username
                bool exists = db.Utilizadores.Any(u => u.Username == p.Username && u.Id != p.Id);
                if (exists)
                {
                    MessageBox.Show("O username já existe.");
                    return;
                }
                if (p.Id == 0) db.Programadores.Add(p);
                db.SaveChanges();
                MessageBox.Show("Programador gravado com sucesso.");
                LoadUsers();
            }
        }

        private void btEliminarGestor_Click(object sender, EventArgs e)
        {
            if (lstListaGestores.SelectedItem is Gestor g)
            {
                var confirm = MessageBox.Show($"Eliminar gestor '{g.Nome}'?", "Confirmação", MessageBoxButtons.YesNo);
                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        using (var db = new iTasksDbContext())
                        {
                            var gestor = db.Gestores.Find(g.Id);
                            if (gestor != null)
                            {
                                db.Gestores.Remove(gestor);
                                db.SaveChanges();
                                MessageBox.Show("Gestor eliminado com sucesso.");
                                LoadUsers();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao eliminar gestor: " + ex.Message);
                    }
                }
            }
        }

        private void btEliminarProg_Click(object sender, EventArgs e)
        {
            if (lstListaProgramadores.SelectedItem is Programador p)
            {
                var confirm = MessageBox.Show($"Eliminar programador '{p.Nome}'?", "Confirmação", MessageBoxButtons.YesNo);
                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        using (var db = new iTasksDbContext())
                        {
                            var prog = db.Programadores.Find(p.Id);
                            if (prog != null)
                            {
                                db.Programadores.Remove(prog);
                                db.SaveChanges();
                                MessageBox.Show("Programador eliminado com sucesso.");
                                LoadUsers();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao eliminar programador: " + ex.Message);
                    }
                }
            }
        }
    }
}
