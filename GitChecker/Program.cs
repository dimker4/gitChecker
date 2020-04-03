using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitChecker
{
    class Program
    {
        public static string dir = @"C:\git\alice-skills\python\buy-elephant\";
        public static List<string> files = new List<string>(); // Список файлов гита
        private const string connectionString = "";

        static void Main(string[] args)
        {
            Console.WriteLine("begin");
            DirSearch(dir);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }

            foreach (string f in files) // Бежим по всем файлам гита
            {
                Console.WriteLine(f);

                if (File.Exists(f))
                {
                    // Open the file to read from.
                    string gitFileText = File.ReadAllText(f);
                    Console.Out.WriteLine(gitFileText);
                }
            }

            Console.ReadKey();
        
}

        static void DirSearch(string sDir)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        files.Add(f);
                    }
                    DirSearch(d);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
    }
}
