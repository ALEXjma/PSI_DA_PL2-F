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
using iTasks.Auth;

namespace iTasks
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
            FormClosed += frmLoginFormClosed;
            btLogin.Click += btLogin_Click;
            registroToolStripMenuItem.Click += registroToolStripMenuItem_Click;
        }

        private void frmLoginFormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void btLogin_Click(object sender, EventArgs e)
        {
            var userService = new AuthManager();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            try
            {
                // Admin check
                if (username == "admin" && password == "admin")
                {
                    // Criar utilizador admin
                    Utilizador adminUser = new Utilizador
                    {
                        Id = 999,
                        Nome = "Administrador",
                        Username = "admin",
                        Password = "admin"
                    };
                    // Guardar utilizador na sessão
                    SessionManager.CurrentUser = adminUser;
                    // Abrir o formulário de gestão de utilizadores
                    frmGereUtilizadores frm = new frmGereUtilizadores();
                    frm.FormClosed += (s, args) => { this.Show(); };
                    this.Hide();
                    frm.Show();
                    return;
                }

                // Autenticar utilizador
                Utilizador user = userService.Authenticate(username, password);
                if (user == null)
                {
                    MessageBox.Show("Nome de utilizador ou password inválido.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Guardar utilizador na sessão
                SessionManager.CurrentUser = user;
                // Abrir o formulário kanban
                frmKanban kanbanForm = new frmKanban();
                kanbanForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void registroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Para fins de teste é possivel usar as credenciais admin admin para aceder à interface de gestão de utilizadores.", "Registro", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
