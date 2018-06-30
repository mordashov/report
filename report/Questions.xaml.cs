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
        private string _mainConnectionString;
        private string _fio;
        private string _tn;

        public string MainConnectionString
        {
            get => _mainConnectionString;
            set => _mainConnectionString = value;
        }

        public string Fio
        {
            get => _fio;
            set => _fio = value;
        }

        public string Tn
        {
            get => _tn;
            set => _tn = value;
        }


        public Questions()
        {
            InitializeComponent();
        }

        private void CreateTable()
        {

            LabelFio.Content = _fio;

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
                count = int.Parse(MainWindow.SingleResult(sql, _mainConnectionString));
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
                    Padding = new Thickness(marginLeft[i], 0, 0, 0),
                    Width = wth[i]
                };
                stackPanelRow[0].Children.Add(aHead[i]);
            }
            StackPanelHead.Children.Add(stackPanelRow[0]);

            string connectionString = _mainConnectionString;
            sql = $@"
                    SELECT 
                        [usr_tn]
                        ,[qst_id]
                        ,[qst_nm]
                        ,[Rezult]
                        FROM [dbo].[vwrep]
                        WHERE [usr_tn] = {_tn}
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
                    Orientation = Orientation.Horizontal,
                    Background = Brushes.AntiqueWhite

                };

                int i = 0;
                foreach (DataColumn col in ds.Tables["t"].Columns)
                {
                    string cell = row[col].ToString();
                    //Меняю цвет StackPanel
                    if (cell == "0") stackPanelRow[j].Background = Brushes.AntiqueWhite;
                    else stackPanelRow[j].Background = Brushes.LightGreen;
                    //Заменяю 1 и 0 Правильно/Не правильно
                    if (cell == "1") cell = "Верно";
                    if (cell == "0") cell = "Не верно";

                    aHead[i] = new TextBlock()
                    {
                        Text = cell,
                        FontSize = font,
                        Padding = new Thickness(marginLeft[i], 20, 0, 0),
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
            TrueAnswers();
        }

        private void TrueAnswers()
        {
            string sql = $@"SELECT CONCAT( COUNT( [Rezult] ), '_',  SUM( [Rezult] ))
                            FROM[dbo].[vwrep]
                            WHERE[usr_tn] = {_tn}";
            string res = MainWindow.SingleResult(sql, _mainConnectionString);
            string total = res.Split('_')[0];
            string success = res.Split('_')[1];
            LabelTrueAnswers.Content = $"Правельных ответов {success} из {total}";
        }
    }
}
