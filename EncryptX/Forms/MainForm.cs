using EncryptX.Services;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
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
            _aesService = new AesService();
            _logService = new LogService();
            _currentUser = username;

            this.Text = $"EncryptX Dashboard - Welcome, {_currentUser}!";

            // Design Adaptiv
            ConfigureInterfaceForUser();
        }

        private void ConfigureInterfaceForUser()
        {
            bool isAdmin = _currentUser.Equals("admin", StringComparison.OrdinalIgnoreCase);

            if (isAdmin)
            {
                // Admin: Vede tot
                btnDeleteUser.Visible = true;
            }
            else
            {
                // User: Nu vede Delete, si Refresh e centrat
                btnDeleteUser.Visible = false;

                int parentWidth = tabControl1.Width;
                int centerX = (parentWidth - btnRefresh.Width) / 2;
                btnRefresh.Location = new Point(centerX, btnRefresh.Location.Y);
            }
        }

        private bool IsPasswordSecure(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) { MessageBox.Show("Introdu o parola!"); return false; }
            if (password.Length < 6) { MessageBox.Show("Minim 6 caractere!"); return false; }
            if (!password.Any(char.IsUpper)) { MessageBox.Show("Minim o litera mare!"); return false; }
            if (!password.Any(ch => !char.IsLetterOrDigit(ch))) { MessageBox.Show("Minim un simbol special!"); return false; }
            return true;
        }

        // --- BUTOANE TEXT ---
        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text)) return;
            if (!IsPasswordSecure(txtPassword.Text)) return;
            try { txtOutput.Text = _aesService.EncryptText(txtInput.Text, txtPassword.Text); _logService.AddLog(_currentUser, "Text", "TEXT_ENCRYPT"); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text)) return;
            if (!IsPasswordSecure(txtPassword.Text)) return;
            try { txtOutput.Text = _aesService.DecryptText(txtInput.Text, txtPassword.Text); _logService.AddLog(_currentUser, "Text", "TEXT_DECRYPT"); }
            catch { MessageBox.Show("Parola gresita!"); }
        }

        // --- BUTOANE FISIERE ---
        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog()) if (ofd.ShowDialog() == DialogResult.OK) txtFilePath.Text = ofd.FileName;
        }

        private void btnEncryptFile_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtFilePath.Text)) return;
            if (!IsPasswordSecure(txtFilePass.Text)) return;
            try { _aesService.EncryptFile(txtFilePath.Text, txtFilePath.Text + ".enc", txtFilePass.Text); _logService.AddLog(_currentUser, Path.GetFileName(txtFilePath.Text), "FILE_ENCRYPT"); MessageBox.Show("Done!"); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnDecryptFile_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtFilePath.Text) || !txtFilePath.Text.EndsWith(".enc")) return;
            if (!IsPasswordSecure(txtFilePass.Text)) return;
            try
            {
                string outFile = txtFilePath.Text.Substring(0, txtFilePath.Text.Length - 4);
                if (File.Exists(outFile)) outFile += ".decrypted";
                _aesService.DecryptFile(txtFilePath.Text, outFile, txtFilePass.Text);
                _logService.AddLog(_currentUser, Path.GetFileName(txtFilePath.Text), "FILE_DECRYPT");
                MessageBox.Show("Done!");
            }
            catch { MessageBox.Show("Parola gresita!"); }
        }

        // --- ISTORIC ---
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            bool isAdmin = _currentUser.Equals("admin", StringComparison.OrdinalIgnoreCase);
            gridHistory.DataSource = _logService.GetLogs(_currentUser, isAdmin);
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            if (!_currentUser.Equals("admin", StringComparison.OrdinalIgnoreCase)) return;
            if (gridHistory.SelectedRows.Count == 0 && gridHistory.CurrentRow == null) return;

            var row = gridHistory.SelectedRows.Count > 0 ? gridHistory.SelectedRows[0] : gridHistory.CurrentRow;
            if (row == null) return;
            string user = row.Cells["Username"].Value.ToString();

            if (user.ToLower() == "admin") { MessageBox.Show("Nu poti sterge adminul!"); return; }

            if (MessageBox.Show($"Stergi pe {user}?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _logService.DeleteUserAndHistory(user);
                btnRefresh.PerformClick();
            }
        }

        // --- NAVIGATIE & CONTROL ---
        private void MainForm_Load(object sender, EventArgs e) { } 
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) => Application.Exit();
        private void button1_Click(object sender, EventArgs e) => Application.Exit();
        private void button2_Click(object sender, EventArgs e) => tabControl1.SelectedIndex = 0;
        private void button3_Click(object sender, EventArgs e) => tabControl1.SelectedIndex = 1;
        private void button4_Click(object sender, EventArgs e) { tabControl1.SelectedIndex = 2; btnRefresh.PerformClick(); }
    }
}