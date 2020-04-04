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
                                                      AND schema_id = 1 -- dbo
                                                 ORDER BY name;";
        private const string dir = @"C:\git\alice-skills\python\buy-elephant\"; // Директория локальной копии репозитория
        private const string dbName = "testdb";

        public static List<string> files = new List<string>(); // Список файлов гита
        public static List<string> objectsSql = new List<string>(); // Список объектов с сервера sql
        

        static void Main(string[] args)
        {
            DirSearch(dir);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                connection.ChangeDatabase(dbName);
                DataSet ds = new DataSet(); // Датасет для имени объектов в базе
                DataSet dsObjectText = new DataSet(); // Датасет для определения объекта
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();

                SqlCommand command = new SqlCommand(getAllObjectsSql, connection);
                sqlDataAdapter.SelectCommand = command;
                sqlDataAdapter.Fill(ds);

                DataTable objects = ds.Tables[0];

                IEnumerable<DataRow> query =
                    from obj in objects.AsEnumerable()
                    select obj;

                foreach (var obj in query) // Бежим по всем объектам базы
                {
                    var name = obj.ItemArray[0];

                    SqlCommand sqlCommand = new SqlCommand("sys.sp_helptext", connection); // Получим 
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@objname", name);
                    sqlDataAdapter.SelectCommand = sqlCommand;
                    sqlDataAdapter.Fill(dsObjectText);
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
