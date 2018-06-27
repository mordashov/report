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

        //private string _mainConnectionString = @"Data Source=DURON\SQLEXPRESS;Initial Catalog=testing;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private string _mainConnectionString = @"Data Source=LENOVO\SQLEXPRESS;Initial Catalog=ufs;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //private string _mainConnectionString = @"Data Source=alauda\alauda;Initial Catalog=ufs;User ID=prozorova_os;Password=q1w2e3r4";

        public string MainConnectionString
        {
            get => _mainConnectionString;
            set => _mainConnectionString = value;
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
            CreateTable();
        }

        private void CreateTable()
        {
            int font = 16;
            string[] headName = new string[4]{"ТН","ФИО","Время","Результат"};
            int[] marginLeft = new [] {20, 20, 300, 20};
            TextBlock[] aHead = new TextBlock[4];
            
            StackPanel stackPanelRow = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            //for (int i = 0; i < aHead.Length; i++)
            //{
            //    aHead[i] = new TextBlock()
            //    {
            //        Text = headName[i],
            //        FontSize = font,
            //        FontWeight = FontWeights.Bold,
            //        Margin = new Thickness(marginLeft[i], 0, 0, 0),
            //    };
            //    stackPanelRow.Children.Add(aHead[i]);
            //}
            //stackPanelTable.Children.Add(stackPanelRow);

            string connectionString = MainConnectionString;
            string sql = @"
                        SELECT [usr_tn]
                            ,[usr_fln]
                            , SUM([Rezult])*100/(SELECT COUNT(*) FROM[dbo].[qst]) as pr
                        FROM[ufs].[dbo].[vwrep]
                        GROUP BY[usr_fln], [usr_tn]
                        ORDER BY[usr_fln], [usr_tn]";
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

            foreach (DataRow row in ds.Tables["t"].Rows)
            {
                int i = 0;
                foreach (DataColumn col in ds.Tables["t"].Columns)
                {
                    string cell = row[col].ToString();
                    aHead[i] = new TextBlock()
                    {
                        Text = cell,
                        FontSize = font,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(marginLeft[i], 0, 0, 0),
                    };
                    stackPanelRow.Children.Add(aHead[i]);
                    i++;
                }
                stackPanelTable.Children.Add(stackPanelRow);

            }
        }
    }

}
