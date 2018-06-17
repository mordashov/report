using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
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
        private DataTable dtTotal { get; set; }
        private DataTable dtCompleted { get; set; }
        private DataTable dtPassTest { get; set; }
        private DataTable dtNotCompleted { get; set; }
        private DataTable dtNotStarted { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            QueryReport();
        }

        private void QueryReport()
        {
            MsAccess acs = new MsAccess();
            string connectionString = acs.MainConnectionString;
            string sql =
                $@"SELECT usr.usr_tn AS ТН, usr.usr_fln AS ФИО, Count(anw.anw_id) AS [Всего ответов], Sum(IIf([anw]![anw_true]=1,1,0)) AS Верно, IIf([Верно]=0,0,Round(([Верно]/[Всего ответов])*100,0)) AS Процент
                    FROM anw RIGHT JOIN (usr LEFT JOIN rez ON usr.usr_tn = rez.usr_tn) ON anw.anw_id = rez.anw_id
                    GROUP BY usr.usr_tn, usr.usr_fln
                    ORDER BY usr.usr_fln;
                    ";
             
            OleDbDataAdapter da = new OleDbDataAdapter(sql, connectionString);

            DataSet ds = new DataSet();

            try
            {
                da.Fill(ds, "t");
            }
            catch (Exception)
            {
                MessageBox.Show("Не могу найти базу данных!\nБаза Access должна быть расположена в одной папке с исполняемым файлом");
                Environment.Exit(0);
            }

            int total = ds.Tables["t"].Rows.Count;

            //Тестируемые сотрудники
            //DataTable dtTotal = new DataTable();
            dtTotal = ds.Tables["t"];

            textBoxTotal.Text = total.ToString() + " чел.";

            //Завершили тестирование
            //DataTable dtCompleted = new DataTable();
            string qString = "[Всего ответов] = '10'";
            dtCompleted = dtTotal.Select(qString).CopyToDataTable();

            int completed = dtCompleted.Rows.Count;
            textBoxCompleted.Text = completed.ToString() + " чел.";

            //Сдали тест
            //DataTable dtPassTest = new DataTable();
            qString = "[Верно] = '10'";
            try
            {
                dtPassTest = dtTotal.Select(qString).CopyToDataTable();
                int passTest = dtPassTest.Rows.Count;
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
                dtNotCompleted = dtTotal.Select(qString).CopyToDataTable();
                int notCompleted = dtNotCompleted.Rows.Count;
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
                dtNotStarted = dtTotal.Select(qString).CopyToDataTable();
                int notStarted = dtNotStarted.Rows.Count;
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
                dataGrigReport.DataContext = dtTotal.DefaultView;
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
                dataGrigReport.DataContext = dtCompleted.DefaultView;
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
                dataGrigReport.DataContext = dtPassTest.DefaultView;
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
                dataGrigReport.DataContext = dtNotCompleted.DefaultView;
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
                dataGrigReport.DataContext = dtNotStarted.DefaultView;
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
    }
}
