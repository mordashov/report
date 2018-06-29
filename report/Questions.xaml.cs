using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace report
{
    /// <summary>
    /// Логика взаимодействия для Questions.xaml
    /// </summary>
    public partial class Questions : Window
    {
        public string Tn { get; set; }

        public Questions()
        {
            InitializeComponent();
        }

        private void CreateTable()
        {
            MainWindow mainWindow = new MainWindow();
            string tn = Tn;
            int font = 16; // Размер шрифта
            const int cl = 4; //Кол-во колонок
            string[] headName = new string[cl] { "ТН", "ФИО", "Вопрос", "Результат" };
            int[] wth = new int[cl] { 0, 0, 600, 120 };
            int[] marginLeft = new int[cl] { 0, 0, 40, 20 };
            TextBlock[] aHead = new TextBlock[cl];

            string sql = "SELECT Count(*) FROM [dbo].[qst];";
            int count = 0;
            try
            {
                count = int.Parse(mainWindow.SingleResult(sql, mainWindow.MainConnectionString));
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
                    Width = wth[i]
                };
                stackPanelRow[0].Children.Add(aHead[i]);
            }
            StackPanelHead.Children.Add(stackPanelRow[0]);

            string connectionString = mainWindow.MainConnectionString;
            sql = $@"
                    SELECT 
                        [usr_tn]
                        ,[qst_id]
                        ,[qst_nm]
                        ,[Rezult]
                        FROM [dbo].[vwrep]
                        WHERE [usr_tn] = {Tn}
                        ORDER BY [qst_id]";
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
                        Width = wth[i]
                    };

                    stackPanelRow[j].Children.Add(aHead[i]);
                    i++;
                }
                StackPanelQestions.Children.Add(stackPanelRow[j]);
                j++;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateTable();

        }
    }
}
