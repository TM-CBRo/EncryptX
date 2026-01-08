using EncryptX.Services; // Conectam serviciul creat la Pasul 7
using System;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace EncryptX.Forms
{
    public partial class MainForm : Form
    {
        private readonly AesService _aesService;

        public MainForm()
        {
            InitializeComponent();
            _aesService = new AesService(); // Pornim motorul
        }

        // --- BUTONUL CRIPTARE ---
        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            string text = txtInput.Text;
            string pass = txtPassword.Text;

            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Introdu textul si parola!", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string result = _aesService.EncryptText(text, pass);
                txtOutput.Text = result;
                MessageBox.Show("Criptare reusita!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        // --- BUTONUL DECRIPTARE ---
        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            string textCriptat = txtInput.Text; // Luam textul criptat tot de sus
            string pass = txtPassword.Text;

            if (string.IsNullOrEmpty(textCriptat) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Introdu textul criptat si parola!", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string result = _aesService.DecryptText(textCriptat, pass);
                txtOutput.Text = result;
                MessageBox.Show("Decriptare reusita!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (CryptographicException)
            {
                MessageBox.Show("Parola gresita sau text corupt!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        // Cand se inchide fereastra principala, inchidem toata aplicatia
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}