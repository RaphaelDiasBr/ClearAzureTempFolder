using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows.Forms;

namespace AzureTempFolder
{
    public partial class MenuDiagnostic : Form
    {
        private string subpastaSelecionada;
        private AzureBlobHelper blobHelper;

        public MenuDiagnostic(string subpastaSelecionada)
        {
            InitializeComponent();

            this.subpastaSelecionada = subpastaSelecionada;

            blobHelper = new AzureBlobHelper(ConfigurationManager.AppSettings["AzureStorageConnectionString"]);
            SetGridView(subpastaSelecionada);
        }

        private void SetGridView(string subpasta)
        {
            try
            {
                List<string> listArquivos = blobHelper.GetFilesWhitProblem(subpasta);

                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("Nome do Arquivo");
                dataTable.Columns.Add("Tamanho do Arquivo");

                if(listArquivos.Count > 0)
                {
                    MessageBox.Show("Foram encontrados arquivos com problemas", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    foreach (string arquivo in listArquivos)
                    {
                        dataTable.Rows.Add(arquivo, "0 KB");
                    }

                    dataGridView1.DataSource = dataTable;
                    dataGridView1.AutoGenerateColumns = true;

                    dataGridView1.Columns["Nome do Arquivo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dataGridView1.Columns["Tamanho do Arquivo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                else
                {
                    MessageBox.Show("Não foram encontrados arquivos com problemas", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao preencher a grid: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MenuForm menuForm = new MenuForm();
            this.Hide();
            menuForm.ShowDialog();

            this.Close();
        }
    }
}
