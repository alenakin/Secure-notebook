using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SecureNotebook.Db;
using SecureNotebook.Services.Exceptions;

namespace SecureNotebook.Services
{
    public class FileService
    {
        private NotebookContext db;

        public FileService(NotebookContext db)
        {
            this.db = db;
        }

        public string GetText(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                throw new ArgumentNullException("File id can't be null or empty string");
            }

            var file = db.Files.Find(fileId) ?? throw new EntityNotFoundException($"File with id {fileId} was not found");

            string filePath = file.FilePath;
            string text = string.Empty;
            using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default))
            {
                text = sr.ReadToEnd();
            }

            return text;
        }
    }
}
