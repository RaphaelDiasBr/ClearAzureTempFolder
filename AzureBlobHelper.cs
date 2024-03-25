using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;

namespace AzureTempFolder
{
    public class AzureBlobHelper
    {
        private string connectionString;

        public AzureBlobHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<string> ObtainAzureInformation(string containerName, string fileName)
        {
            List<string> informacoesAzure = new List<string>();

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

                if (blob.Exists())
                {
                    informacoesAzure.Add("Sim");

                    double size = blob.Properties.Length / 1024.0;

                    if (size > 0)
                        informacoesAzure.Add($"{size:N2} KB");
                    else
                        informacoesAzure.Add("ERROR");
                }
                else
                {
                    informacoesAzure.Add("Não");
                    informacoesAzure.Add(string.Empty);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter informações no Azure: {ex.Message}");
                informacoesAzure.Add("Erro");
                informacoesAzure.Add(string.Empty);
            }

            return informacoesAzure;
        }


        public List<string> GetFilesWhitProblem(string containerName)
        {
            List<string> problematicFiles = new List<string>();

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                foreach (IListBlobItem blobItem in container.ListBlobs(null, true, BlobListingDetails.None))
                {
                    if (blobItem is CloudBlockBlob blockBlob)
                    {
                        if (blockBlob.Properties.Length == 0)
                        {
                            problematicFiles.Add(blockBlob.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar no Azure: {ex.Message}");
            }

            return problematicFiles;
        }
    }
}