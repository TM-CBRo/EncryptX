using EncryptX.Services;
using System;
using System.IO;
using System.Linq; // pt verificarea parolei
using System.Security.Cryptography;
using System.Windows.Forms;

namespace EncryptX.Forms
{
    public partial class MainForm : Form
    {
        private readonly AesService _aesService;
        private readonly LogService _logService;
        private readonly string _currentUser;

        public MainForm(string username)
        {
            InitializeComponent();
            _aesService = new AesService(); // Pornim motorul AES
            _logService = new LogService(); // Pornim serviciul de log-uri
            _currentUser = username;        // Salvam cine a intrat

            this.Text = $"EncryptX Dashboard - Welcome, {_currentUser}!";
        }

        // =========================================================
        //  NOUA FUNCTIE DE VALIDARE AVANSATA
        // =========================================================
        private bool IsPasswordSecure(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Te rog introdu o parola!", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 1. Verificare Lungime (Minim 6)
            if (password.Length < 6)
            {
                MessageBox.Show("Parola este prea scurta!\nTrebuie sa aiba minim 6 caractere.", "Securitate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 2. Verificare Litera Mare (A-Z)
            if (!password.Any(char.IsUpper))
            {
                MessageBox.Show("Parola trebuie sa contina cel putin o litera mare (A-Z)!", "Securitate Slaba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 3. Verificare Simbol (Orice nu e litera sau cifra)
            // Aceasta linie cauta semne gen: ! @ # $ % ^ & * ( ) _ - + =
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                MessageBox.Show("Parola trebuie sa contina cel putin un simbol special (ex: @, #, !, $)", "Securitate Slaba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true; // Parola a trecut toate testele!
        }

        // =========================================================
        // TAB 2: TEXT (Logic pentru Text)
        // =========================================================

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            string text = txtInput.Text;
            string pass = txtPassword.Text;

            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Introdu textul!", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validare Parola
            if (!IsPasswordSecure(pass)) return;

            try
            {
                string result = _aesService.EncryptText(text, pass);
                txtOutput.Text = result;

                _logService.AddLog(_currentUser, "Text Message", "TEXT_ENCRYPT");

                MessageBox.Show("Criptare reusita!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            string textCriptat = txtInput.Text;
            string pass = txtPassword.Text;

            if (string.IsNullOrEmpty(textCriptat))
            {
                MessageBox.Show("Introdu textul criptat!", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validare Parola
            if (!IsPasswordSecure(pass)) return;

            try
            {
                string result = _aesService.DecryptText(textCriptat, pass);
                txtOutput.Text = result;

                _logService.AddLog(_currentUser, "Text Message", "TEXT_DECRYPT");

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

        // =========================================================
        // TAB 1: FISIERE (Logic pentru Fisiere)
        // =========================================================

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Alege un fisier";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                }
            }
        }

        private void btnEncryptFile_Click(object sender, EventArgs e)
        {
            string inputFile = txtFilePath.Text;
            string password = txtFilePass.Text;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                MessageBox.Show("Te rog alege un fisier valid!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validare Parola
            if (!IsPasswordSecure(password)) return;

            try
            {
                string outputFile = inputFile + ".enc";
                _aesService.EncryptFile(inputFile, outputFile, password);

                string fileNameOnly = Path.GetFileName(inputFile);
                _logService.AddLog(_currentUser, fileNameOnly, "FILE_ENCRYPT");

                MessageBox.Show($"Fisier criptat cu succes!\nSalvat ca: {outputFile}", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la criptare: " + ex.Message);
            }
        }

        private void btnDecryptFile_Click(object sender, EventArgs e)
        {
            string inputFile = txtFilePath.Text;
            string password = txtFilePass.Text;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                MessageBox.Show("Alege fisierul .enc!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!inputFile.EndsWith(".enc"))
            {
                MessageBox.Show("Te rog selecteaza un fisier care se termina in .enc", "Atentie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validare Parola
            if (!IsPasswordSecure(password)) return;

            try
            {
                string outputFile = inputFile.Substring(0, inputFile.Length - 4);

                if (File.Exists(outputFile))
                {
                    outputFile = outputFile + ".decrypted";
                }

                _aesService.DecryptFile(inputFile, outputFile, password);

                string fileNameOnly = Path.GetFileName(inputFile);
                _logService.AddLog(_currentUser, fileNameOnly, "FILE_DECRYPT");

                MessageBox.Show($"Fisier decriptat cu succes!\nSalvat ca: {outputFile}", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (CryptographicException)
            {
                MessageBox.Show("Parola gresita!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        // =========================================================
        // TAB 3: ISTORIC
        // =========================================================
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                var logs = _logService.GetLogs();
                gridHistory.DataSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la incarcarea istoricului: " + ex.Message);
            }
        }

        // =========================================================
        // Navigatie si Inchidere (Butoanele tale personalizate)
        // =========================================================

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e) // Butonul X
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e) // Tab Files
        {
            tabControl1.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e) // Tab Text
        {
            tabControl1.SelectedIndex = 1;
        }

        private void button4_Click(object sender, EventArgs e) // Tab History
        {
            tabControl1.SelectedIndex = 2;
        }
    }
}