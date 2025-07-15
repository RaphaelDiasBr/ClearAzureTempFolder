using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace AzureTempFolder
{
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
            FillDropDown();
        }

        private void FillDropDown()
        {
            try
            {
                comboBox1.Items.Clear();

                // Get Folders
                string[] subpastas = Directory.GetDirectories(ConfigurationManager.AppSettings["AzureTempFolderPath"]);

                foreach (string subpasta in subpastas)
                {
                    comboBox1.Items.Add(Path.GetFileName(subpasta));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter subpastas: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string subpastaSelecionada = comboBox1.SelectedItem as string;

            if (!string.IsNullOrEmpty(subpastaSelecionada))
            {
                MenuList menuList = new MenuList(subpastaSelecionada);
                this.Hide();
                menuList.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Selecione uma subpasta na lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string subpastaSelecionada = comboBox1.SelectedItem as string;

            if (!string.IsNullOrEmpty(subpastaSelecionada))
            {
                MenuDiagnostic menuList = new MenuDiagnostic(subpastaSelecionada);
                this.Hide();
                menuList.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Selecione uma subpasta na lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
