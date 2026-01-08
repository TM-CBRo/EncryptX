//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace EncryptX.Forms
//{
//    public partial class LoginForm : Form
//    {
//        public LoginForm()
//        {
//            InitializeComponent();
//        }

//        private void LoginForm_Load(object sender, EventArgs e)
//        {

//        }

//        private void btnRegister_Click(object sender, EventArgs e)
//        {

//        }

//        private void btnLogin_Click(object sender, EventArgs e)
//        {

//        }
//    }
//}

using System;
using System.Windows.Forms;
using EncryptX.Services; // Ca sa vedem AuthService

namespace EncryptX.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;

        public LoginForm()
        {
            InitializeComponent();
            _authService = new AuthService(); // Pornim serviciul
        }

        // --- BUTONUL DE INREGISTRARE ---
        private void btnRegister_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Te rog completeaza ambele campuri!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool success = _authService.RegisterUser(user, pass);
                if (success)
                {
                    MessageBox.Show("Cont creat cu succes! Acum te poti loga.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Acest utilizator exista deja!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la baza de date: " + ex.Message);
            }
        }

        // --- BUTONUL DE LOGIN ---
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Introdu user si parola!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_authService.LoginUser(user, pass))
            {
                // LOGIN REUSIT!
                // 1. Ascundem fereastra de login
                this.Hide();

                // 2. Deschidem fereastra principala
                MainForm main = new MainForm(user);
                main.ShowDialog(); // Asteptam aici pana se inchide Main

                // 3. Cand se inchide Main, inchidem de tot aplicatia
                this.Close();
            }
            else
            {
                MessageBox.Show("Utilizator sau parola incorecta!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}