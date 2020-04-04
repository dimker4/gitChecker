using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitChecker
{
    class Program
    {
        private const string connectionString = "Data Source=DESKTOP-85CTC0B\\SQLEXPRESS;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private const string getAllObjectsSql = @"SELECT name 
                                                     FROM sys.all_objects
                                                    WHERE type in ('P', 'X', 'PC', 'FN', 'FT', 'IF') 
                                                 ORDER BY name;";
        private const string dir = @"C:\git\alice-skills\python\buy-elephant\"; // Директория локальной копии репозитория

        public static List<string> files = new List<string>(); // Список файлов гита
        public static List<string> objectsSql = new List<string>(); // Список объектов с сервера sql
        

        static void Main(string[] args)
        {
            DirSearch(dir);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet ds = new DataSet(); // Датасет для определения объекта
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();

                SqlCommand command = new SqlCommand(getAllObjectsSql, connection);
                command.Connection.Open();

                var a = command.ExecuteScalar();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        objectsSql.Add((string)reader["name"]); // Получим имя объекта

                        SqlCommand sqlCommand = new SqlCommand("sys.sp_helptext", connection); // Получим 
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@objname", (string)reader["name"]);
                        sqlDataAdapter.SelectCommand = sqlCommand;
                        sqlDataAdapter.Fill(ds);

                    }
                }
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
