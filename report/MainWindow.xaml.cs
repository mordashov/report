using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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

        private string _mainConnectionString = @"Data Source=DURON\SQLEXPRESS;Initial Catalog=testing;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //private string _mainConnectionString = @"Data Source=LENOVO\SQLEXPRESS;Initial Catalog=ufs;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //private string _mainConnectionString = @"Data Source=alauda\alauda;Initial Catalog=ufs;User ID=prozorova_os;Password=q1w2e3r4";

        public string MainConnectionString
        {
            get => _mainConnectionString;
            set => _mainConnectionString = value;
        }
        public object Theard { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            CreateTable();
            TotalSum();
        }

        private void CreateTable()
        {
            int font = 16; // Размер шрифта
            const int cl = 4; //Кол-во колонок
            string[] headName = new string[cl] {"ТН","ФИО","Время (мин.)","Результат (%)"};
            int[] wth = new int[cl] { 100, 300, 120, 120};
            int[] marginLeft = new int[cl] { 40, 20, 20, 20};
            TextBlock[] aHead = new TextBlock[cl];

            string sql = "SELECT Count(*) FROM [dbo].[usr];";
            int count = 0;
            try
            {
                count = int.Parse(SingleResult(sql, MainConnectionString));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(0);
            }

            StackPanel[] stackPanelRow = new StackPanel[count];

            //Формирую шапку таблицы
            stackPanelRow[0] = new StackPanel() { Orientation = Orientation.Horizontal };
            for (int i = 0; i < aHead.Length; i++)
            {
                aHead[i] = new TextBlock()
                {
                    Text = headName[i],
                    FontSize = font,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(marginLeft[i], 0, 0, 0),
                    Width = wth[i],
                };
                stackPanelRow[0].Children.Add(aHead[i]);
            }
            StackPanelHead.Children.Add(stackPanelRow[0]);

            string connectionString = MainConnectionString;
            sql = @"
                    SELECT [usr].[usr_tn]
                        ,[usr].[usr_fln]
	                    ,DATEDIFF(MI, MIN([usr].[usr_st]), MAX([usr].[usr_fn])) as [time]
                        , SUM([Rezult])*100/(SELECT COUNT(*) FROM[dbo].[qst]) as [pr]
                    FROM [dbo].[vwrep]
					RIGHT JOIN [dbo].[usr] ON [usr].[usr_tn] = [vwrep].[usr_tn]
                    GROUP BY [usr].[usr_fln], [usr].[usr_tn]
                    ORDER BY [usr].[usr_fln], [usr].[usr_tn]";
            SqlDataAdapter da = new SqlDataAdapter(sql, connectionString);
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds, "t");
            }
            catch (Exception)
            {
                MessageBox.Show("Не могу получить доступ к базе данных!");
                Environment.Exit(0);
            }

            int j = 0;
            foreach (DataRow row in ds.Tables["t"].Rows)
            {

                stackPanelRow[j] = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };

                int i = 0;
                foreach (DataColumn col in ds.Tables["t"].Columns)
                {
                    string cell = row[col].ToString();
                    aHead[i] = new TextBlock()
                    {
                        Text = cell,
                        FontSize = font,
                        Margin = new Thickness(marginLeft[i], 20, 0, 0),
                        Width = wth[i],
                        //Формирую имя ячейки, чтобы проще было найти
                        Name = "cl" + i + "_rw" + j.ToString()
                    };
                   
                    stackPanelRow[j].Children.Add(aHead[i]);
                    //Вешаю событие на ФИО
                    if (i == 1)
                    {
                        stackPanelRow[j].Children[i].MouseLeftButtonUp += new MouseButtonEventHandler(OpenQuestions);
                        stackPanelRow[j].Children[i].MouseEnter += new MouseEventHandler(CursorHand);
                        stackPanelRow[j].Children[i].MouseLeave += new MouseEventHandler(CursorArrow);
                    }

                    i++;
                }
                StackPanelTable.Children.Add(stackPanelRow[j]);
                j++;
            }
        }

        private void TotalSum()
        {
            string sql = "SELECT COUNT(*) FROM [dbo].[usr]";

            string t = SingleResult(sql, MainConnectionString);

            sql = "SELECT COUNT(*) FROM [dbo].[usr] WHERE [usr].[usr_login] is not null AND [usr].[usr_login] != ''";

            string s = SingleResult(sql, MainConnectionString);

            LabelTotal.Content = $"Прошли тест {s} из {t} чел.";

        }

        //Получение.отправка одиночого значения sql
        public static string SingleResult(string sql, string connectionString)
        {
            string result = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);

                try
                {
                    conn.Open();
                    result = cmd.ExecuteScalar().ToString();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    Console.WriteLine(ex.Message);
                }

                conn.Close();

            }

            return result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StackPanelHead.Children.Clear();
            StackPanelTable.Children.Clear();
            CreateTable();
            TotalSum();
        }

        private void OpenQuestions(object sender, EventArgs e)
        {
            string tn = ((TextBlock)sender).Name;
            int rw = int.Parse(tn.Split('_')[1].Replace("rw", "")); //строка
            int cl = int.Parse(tn.Split('_')[0].Replace("cl","")); //колонка
            StackPanel stackPanelRow = (StackPanel) StackPanelTable.Children[rw];
            tn = ((TextBlock) stackPanelRow.Children[0]).Text;
            string fio = ((TextBlock)stackPanelRow.Children[1]).Text;

            Questions qst = new Questions
            {
                Tn = tn,
                MainConnectionString = _mainConnectionString,
                Fio = fio
            };
            qst.ShowDialog();
            
        }

        public void CursorHand(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void CursorArrow(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }

}
