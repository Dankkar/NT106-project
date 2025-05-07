using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharingServer
{
    public static class DatabaseHelper
    {
        public static string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        private static string dbPath = Path.Combine(projectRoot, "test.db");
        public static readonly string connectionString = $"Data Source={dbPath};Version=3;";
    }
}
