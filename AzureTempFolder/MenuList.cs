using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AzureTempFolder
{
    public partial class MenuList : Form
    {
        private string subpastaSelecionada;
        private AzureBlobHelper blobHelper;

        public MenuList(string subpastaSelecionada)
        {
            InitializeComponent();

            this.subpastaSelecionada = subpastaSelecionada;

            blobHelper = new AzureBlobHelper(ConfigurationManager.AppSettings["AzureStorageConnectionString"]);            

            // Mostrar a barra de progresso
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Visible = true;

            // Chamar o método para preencher a DataGridView em segundo plano
            Task.Run(() => PreencherDataGridViewComDados(subpastaSelecionada));
        }

        private void PreencherDataGridViewComDados(string subpasta)
        {
            try
            {
                string folderPath = Path.Combine(ConfigurationManager.AppSettings["AzureTempFolderPath"], subpasta);
                string[] arquivos = Directory.GetFiles(folderPath);

                if(arquivos.Count() > 800)
                {
                    MessageBox.Show("Muitos arquivos encontrados. Serão exibidos apenas 800 na listagem!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    arquivos = arquivos.Take(800).ToArray();
                }             

                // Criar uma DataTable para armazenar os dados
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("Nome do Arquivo");
                dataTable.Columns.Add("Tamanho do Arquivo");
                dataTable.Columns.Add("Encontrado no Azure");
                dataTable.Columns.Add("Tamanho no Azure");
                dataTable.Columns.Add("Selecionar para Remover", typeof(bool));

                // Preencher a DataTable com dados dos arquivos
                foreach (string arquivo in arquivos)
                {
                    FileInfo fileInfo = new FileInfo(arquivo);

                    List<string> azureFileInformation = blobHelper.ObtainAzureInformation(subpasta, fileInfo.Name);
                    double size = fileInfo.Length / 1024.0;

                    dataTable.Rows.Add(fileInfo.Name, $"{size:N2} KB", azureFileInformation[0], azureFileInformation[1]);
                }

                // Verificar se o controle dataGridView1 está criado
                if (dataGridView1.IsHandleCreated)
                {
                    Invoke(new Action(() =>
                    {
                        dataGridView1.DataSource = dataTable;
                        dataGridView1.AutoGenerateColumns = false;

                        dataGridView1.Columns["Nome do Arquivo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        dataGridView1.Columns["Tamanho do Arquivo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                        progressBar1.Visible = false;
                    }));
                }
                else
                {
                    // Se o controle dataGridView1 ainda não estiver criado, tente novamente após um pequeno intervalo
                    Task.Delay(100).ContinueWith(_ => PreencherDataGridViewComDados(subpasta));
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool checkBox1State = checkBox1.Checked;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
               
                if (row.Cells["Selecionar para Remover"] is DataGridViewCheckBoxCell checkBoxCell)
                {
                    checkBoxCell.Value = checkBox1State;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool hasFile = false;
            bool fileWithProblem = false;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Selecionar para Remover"] is DataGridViewCheckBoxCell checkBoxCell)
                {
                    if (checkBoxCell.Value != null && checkBoxCell.Value != DBNull.Value && Convert.ToBoolean(checkBoxCell.Value))
                    {
                        if(row.Cells["Tamanho no Azure"].Value.ToString() == "ERROR" || row.Cells["Encontrado no Azure"].Value.ToString() == "Não")
                        {
                            fileWithProblem = true;
                            break;
                        }

                        hasFile = true;
                        break;
                    }                    
                }
            }

            if(fileWithProblem)
            {
                MessageBox.Show("Você selecionou arquivos com problemas, por motivo de segurança não é possível excluir.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (hasFile)
            {
                DialogResult result = MessageBox.Show("Você tem certeza? Todos os arquivos selecionados serão deletados.",
                                                      "Confirmação",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    ExcluirArquivosSelecionados();
                }
            }
            else
            {
                MessageBox.Show("Nenhum arquivo selecionado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ExcluirArquivosSelecionados()
        {
            try
            {
                int count = 0;

                // Percorrer todas as linhas da DataGridView
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["Selecionar para Remover"] is DataGridViewCheckBoxCell checkBoxCell && row.Cells["Nome do Arquivo"].Value != null)
                    {
                        if (checkBoxCell.Value != null && checkBoxCell.Value != DBNull.Value && Convert.ToBoolean(checkBoxCell.Value))
                        {
                            string nomeArquivo = row.Cells["Nome do Arquivo"].Value.ToString();
                            RemoverArquivo(nomeArquivo);
                            count++;
                        }                       
                    }
                }

                MessageBox.Show(count + " arquivo(s) deletado(s).", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Mostrar a barra de progresso
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.Visible = true;

                // Chamar o método para preencher a DataGridView em segundo plano
                Task.Run(() => PreencherDataGridViewComDados(subpastaSelecionada));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir os arquivos: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoverArquivo(string nomeArquivo)
        {
            try
            {
                string folderPath = Path.Combine(ConfigurationManager.AppSettings["AzureTempFolderPath"], subpastaSelecionada, nomeArquivo);
                File.Delete(folderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao remover o arquivo: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}