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
        private const string dir = @"C:\git\test\"; // Директория локальной копии репозитория
        private const string dbName = "testdb";

        public static List<string> files = new List<string>(); // Список файлов гита
        public static List<string> objectsSql = new List<string>(); // Список объектов с сервера sql
        
        /*
         * Добавить:
         * 1. try-catch в блоке сравнения
         * 2. Ведение лога проверки в базе
         * 3. Разобраться в юнит-тестами
         */

        static void Main(string[] args)
        {
            DirSearch(dir);
            var line = "";
            var generalCounterLine = 0; // Общий счетчик строк в файле
            var goodCounterLine = 0; // Счетчик совпавших строк в файле
            var IndexOutOfRangeException = 1;
            var fileName = "";

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
                    var nameSqlObject = obj.ItemArray[0];
                    IndexOutOfRangeException = 0;
                    goodCounterLine = 0;
                    generalCounterLine = 0;
                    fileName = "";

                    SqlCommand sqlCommand = new SqlCommand("sys.sp_helptext", connection); // Получим листинг процедуры
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@objname", nameSqlObject);
                    sqlDataAdapter.SelectCommand = sqlCommand;
                    sqlDataAdapter.Fill(dsObjectText);

                    DataTable objectTexts = dsObjectText.Tables[0];
                    List<DataRow> objectTextList = new List<DataRow>();
                    objectTextList = objectTexts.AsEnumerable().ToList();

                    foreach (var f in files) // Бежим по всем файлам гита
                    {
                        fileName = Path.GetFileName(f);
                        if (fileName.ToLower() == nameSqlObject.ToString().ToLower() + ".sql")
                        { // Если нашли файл в гите, то начинаем построчное сравнивание
                            StreamReader streamReader = new StreamReader(f);
                            while ((line = streamReader.ReadLine()) != null) // line - строка из гита
                            {
                                try
                                {
                                    var lineGit = line.Replace("\t", "").Replace("\n", "").Replace("\r", "").Replace("dbo.", "").Replace("GO", "");
                                    var lineSql = objectTextList[generalCounterLine].ItemArray[0].ToString().Replace("\t", "").Replace("\n", "").Replace("\r", "").Replace("GO", "");

                                    if (lineGit == lineSql) // Проверим, что первая строка совпадает, добавить сравнение не по строкам, а со CREATE или ALTER
                                    {
                                        goodCounterLine++;
                                    }
                                    generalCounterLine++;   // Общее количество строк
                                }
                                catch (ArgumentOutOfRangeException e)
                                {
                                    IndexOutOfRangeException = 1;
                                }

                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("BAD: ");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(nameSqlObject);
                            Console.WriteLine("File not exists on Git");
                        }

                        if (goodCounterLine == generalCounterLine)
                        {
                            if (generalCounterLine != 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("GOOD: ");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine(f);
                                if (IndexOutOfRangeException == 1)
                                {
                                    Console.WriteLine("Number of rows not equals");
                                }
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("BAD: ");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(f);
                        }
                    }
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
