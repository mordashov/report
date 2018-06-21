using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace report
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string _mainConnectionString;
        private string _mainBasePath;

        public string MainConnectionString
        {
            get => _mainConnectionString;
            set => _mainConnectionString = value;
        }

        public string MainBasePath
        {
            get => _mainBasePath;
            set => _mainBasePath = value;
        }


        private DataTable DtTotal { get; set; }
        private DataTable DtMain { get; set; }
        private DataTable DtCompleted { get; set; }
        private DataTable DtPassTest { get; set; }
        private DataTable DtNotCompleted { get; set; }
        private DataTable DtNotStarted { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SetBasePath();
            GenerateResultArray();
            
            //QueryReport();
        }

        private void GenerateResultArray()
        {
            string sql;

            //Тестируемые сотрудники
            sql = "SELECT usr.usr_tn as ТН, usr.usr_fln as ФИО FROM usr";
            DtTotal = DtResult(sql, MainConnectionString);
            textBoxTotal.Text = DtTotal.Rows.Count + " чел.";


            //Таблица результатов
            sql =
                @"SELECT 
                    rez.usr_tn AS ТН,
                    usr.usr_fln AS ФИО,
                    qst.qst_id, 
                    qst.qst_nm, 
                    qst.qst_tp, 
                    IIf([qst]![qst_tp]=""radio"",IIf([anw_anw]=1,1,0),0) AS Radio, 
                    IIf([qst]![qst_tp]=""check"",IIf([anw_anw]=[rez_anw],1,0),0) AS [Check], 
                    Sum(anw.anw_true) AS anw_anw, Count(anw.anw_true) AS rez_anw, 
                    IIf([qst]![qst_tp]=""text"",IIf([sum_text]=[count_anw],1,0),0) AS [Text], Sum(IIf([anw]![anw_true]=[rez]![rez_vl],1,0)) AS sum_text, 
                    Count(anw.anw_id) AS count_anw, 
                    IIf(
                        [qst]![qst_tp]=""radio"",
                            IIf([anw_anw]=1,1,0),
                                IIf([qst]![qst_tp]=""check"",
                                    IIf([anw_anw]=[rez_anw],1,0),
                                            IIf([qst]![qst_tp]=""text"",
                                                    IIf([sum_text]=[count_anw],1,0),0))) AS Rezult
                FROM usr INNER JOIN (qst INNER JOIN (anw INNER JOIN rez ON anw.anw_id = rez.anw_id) ON qst.qst_id = anw.qst_id) ON usr.usr_tn = rez.usr_tn
                GROUP BY rez.usr_tn, usr.usr_fln, qst.qst_id, qst.qst_nm, qst.qst_tp;";
            DtMain = DtResult(sql, MainConnectionString);

            //Завершили тестирование
            DtCompleted = DtMain.AsEnumerable()
                .GroupBy(r => new { Col1 = r["ТН"], Col2 = r["ФИО"] })
                .Select(g => g.OrderBy(r => r["qst_id"]).First())
                .CopyToDataTable();
        }

        //ExecuteScalar
        private string SingleResult(string sql, string connectionString)
        {
            string testingStaff = "#Ошибка";

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                OleDbCommand cmd = new OleDbCommand(sql, conn);

                try
                {
                    conn.Open();
                    testingStaff = cmd.ExecuteScalar().ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            return testingStaff;
        }

        //DataTable
        private DataTable DtResult(string sql, string connectionString)
        {
            DataTable dtResult = null;

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                DataSet ds = new DataSet();

                try
                {
                    conn.Open();
                    da.Fill(ds, "t");
                    dtResult = ds.Tables["t"];
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Environment.Exit(0);
                }
            }

             
            return dtResult;
        }

        private void QueryReport()
        {

            string sql = @"SELECT Count(qst.qst_id) AS [Count-qst_id] FROM qst";
            //Кол-во вопросов
            string countQuestions = SingleResult(sql, MainConnectionString);

            string connectionString = MainConnectionString;
            sql =
                @"SELECT 
                    rez.usr_tn, 
                    qst.qst_id, 
                    qst.qst_nm, 
                    qst.qst_tp, 
                    IIf([qst]![qst_tp]=""radio"",IIf([anw_anw]=1,1,0),0) AS Radio, 
                    IIf([qst]![qst_tp]=""check"",IIf([anw_anw]=[rez_anw],1,0),0) AS [Check], 
                    Sum(anw.anw_true) AS anw_anw, Count(anw.anw_true) AS rez_anw, 
                    IIf([qst]![qst_tp]=""text"",IIf([sum_text]=[count_anw],1,0),0) AS [Text], Sum(IIf([anw]![anw_true]=[rez]![rez_vl],1,0)) AS sum_text, 
                    Count(anw.anw_id) AS count_anw, 
                    IIf(
                        [qst]![qst_tp]=""radio"",
                            IIf([anw_anw]=1,1,0),
                                IIf([qst]![qst_tp]=""check"",
                                    IIf([anw_anw]=[rez_anw],1,0),
                                            IIf([qst]![qst_tp]=""text"",
                                                    IIf([sum_text]=[count_anw],1,0),0))) AS Rezult
                FROM qst INNER JOIN(anw INNER JOIN rez ON anw.anw_id = rez.anw_id) ON qst.qst_id = anw.qst_id
                GROUP BY rez.usr_tn, qst.qst_id, qst.qst_nm, qst.qst_tp;";

            OleDbDataAdapter da = new OleDbDataAdapter(sql, connectionString);

            DataSet ds = new DataSet();

            try
            {
                da.Fill(ds, "t");
            }
            catch (Exception)
            {
                MessageBox.Show("Не могу найти базу данных!");
                Environment.Exit(0);
            }

            int total = ds.Tables["t"].Rows.Count;

            //Тестируемые сотрудники
            DtTotal = ds.Tables["t"];
            textBoxTotal.Text = total.ToString() + " чел.";

            //Завершили тестирование
            string qString = "[Всего ответов] = '10'";
            DtCompleted = DtTotal.Select(qString).CopyToDataTable();

            int completed = DtCompleted.Rows.Count;
            textBoxCompleted.Text = completed.ToString() + " чел.";

            //Сдали тест
            qString = "[Верно] = '10'";
            try
            {
                DtPassTest = DtTotal.Select(qString).CopyToDataTable();
                int passTest = DtPassTest.Rows.Count;
                textBoxPassTest.Text = passTest.ToString() + " чел.";
            }
            catch (Exception)
            {
                textBoxPassTest.Text = "0 чел.";
            }

            //Не завершили тестирование
            //DataTable dtNotCompleted = new DataTable();
            qString = "[Всего ответов] < '10' AND [Всего ответов] > 0";
            try
            {
                DtNotCompleted = DtTotal.Select(qString).CopyToDataTable();
                int notCompleted = DtNotCompleted.Rows.Count;
                textBoxNotCompleted.Text = notCompleted.ToString() + " чел.";
            }
            catch (Exception)
            {
                textBoxNotCompleted.Text = "0 чел.";
            }

            //Не приступали
            //DataTable dtNotStarted = new DataTable();
            qString = "[Всего ответов] = '0'";
            try
            {
                DtNotStarted = DtTotal.Select(qString).CopyToDataTable();
                int notStarted = DtNotStarted.Rows.Count;
                textBoxNotStarted.Text = notStarted.ToString() + " чел.";
            }
            catch (Exception)
            {
                textBoxNotCompleted.Text = "0 чел.";
            }
        }

        private void labelTotal_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                dataGrigReport.DataContext = DtTotal.DefaultView;
                dataGrigReport.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {

                dataGrigReport.Visibility = Visibility.Hidden;
            }
        }

        private void labelCompleted_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                dataGrigReport.DataContext = DtCompleted.DefaultView;
                dataGrigReport.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {

                dataGrigReport.Visibility = Visibility.Hidden;
            }
        }

        private void labelPassTest_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                dataGrigReport.DataContext = DtPassTest.DefaultView;
                dataGrigReport.Visibility = Visibility.Visible;

            }
            catch (Exception)
            {
                dataGrigReport.Visibility = Visibility.Hidden;
            }
            
        }

        private void labelNotCompleted_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                dataGrigReport.DataContext = DtNotCompleted.DefaultView;
                dataGrigReport.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {

                dataGrigReport.Visibility = Visibility.Hidden;
            }
        }

        private void labelNotStarted_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                dataGrigReport.DataContext = DtNotStarted.DefaultView;
                dataGrigReport.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {

                dataGrigReport.Visibility = Visibility.Hidden;
            }
        }

        private void Label_MouseMove(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
            ((Label)sender).Foreground = Brushes.LimeGreen;
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            ((Label)sender).Foreground = Brushes.Black;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            QueryReport();
            dataGrigReport.Visibility = Visibility.Hidden;
        }

        //Проверка наличия пути к базе
        private void SetBasePath()
        {

            string baseDirectory = Environment.CurrentDirectory;
            string configFile = baseDirectory + @"\config.txt";
            if (File.Exists(configFile))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(configFile);
                string line = file.ReadLine();
                MainBasePath = line;
                MainConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + line + ";Persist Security Info=True;Jet OLEDB:Database Password=lenovo";
                // + ";Persist Security Info=True;Jet OLEDB:Database Password=lenovo"
            }
            else
            {
                MessageBox.Show("Конфигурационный файл не найден!\n" +
                                "Создайте файл config.txt в папке с программой и " +
                                "укажите в нем путь к базе данных");
                Environment.Exit(0);
            }
        }
    }
}
