using System;
using System.Data.SQLite;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Dapper;
using Microsoft.Extensions.Configuration;
using Document = SmartVault.Program.BusinessObjects.Document;

namespace SmartVault.Program
{
    partial class Program
    {
        private static IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            WriteEveryThirdFileToFile(args[0]);
            GetAllFileSizes();
        }

        private static void GetAllFileSizes()
        {
            string outputFile = "GetAllFileSizesResult.txt";
            using (var connection = new SQLiteConnection(string.Format(configuration?["ConnectionStrings:DefaultConnection"] ?? "", configuration?["DatabaseFileName"])))
            {
                var result = connection.Query<int>($"SELECT SUM(LENGTH) FROM DOCUMENT");
                CreateFile(outputFile, result?.ToString());
            }
        }

        private static void WriteEveryThirdFileToFile(string accountId, string outputFile = "result.txt", string textToSearch = "Smith Property")
        {
            var sb = new StringBuilder();

            using (var connection = new SQLiteConnection(string.Format(configuration?["ConnectionStrings:DefaultConnection"] ?? "", configuration?["DatabaseFileName"])))
            {

                var queryResult = connection.Query<Document>($"SELECT * FROM Document WHERE AccountID = {accountId};").Select(d => d.FilePath);

                for (int i = 0; i < queryResult?.Count() / 3; i = i + 3)
                {
                    sb.Append(CheckInFile(queryResult?.ElementAt(i), textToSearch));
                }

                CreateFile(outputFile, sb.ToString());
                Console.WriteLine($"The new file {outputFile} has been created");
            }
        }
        private static string CheckInFile(string path, string textToSearch)
        {
            IFileSystem _fileSystem;
            _fileSystem = new FileSystem();
            string fileContent = _fileSystem.File.ReadAllText(path);

            if (fileContent.Contains(textToSearch))
            {
                return fileContent;
            }
            else
            {
                return "";
            }
        }

        private static void CreateFile(string fileName, string content)
        {
            IFileSystem _fileSystem;
            string _directory = Path.GetFullPath(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\..\\");
            _fileSystem = new FileSystem();
            _fileSystem.File.WriteAllText(_directory + fileName, content);
        }
    }
}