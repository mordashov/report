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

            string sql = "SELECT Count(*) FROM [sr].[qst];";
            int count = 0;
            try
            {
                count = int.Parse(MainWindow.SingleResult(sql, _mainConnectionString));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
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
                        FROM [sr].[vwrep]
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
                    if (i == 3)
                    {
                        //Меняю цвет StackPanel
                        if (cell == "0") stackPanelRow[j].Background = Brushes.AntiqueWhite;
                        else stackPanelRow[j].Background = Brushes.LightGreen;
                        //Заменяю 1 и 0 Правильно/Не правильно
                        if (cell == "1") cell = "Верно";
                        if (cell == "0") cell = "Не верно";
                    }

                    aHead[i] = new TextBlock()
                    {
                        Text = cell,
                        FontSize = font,
                        Padding = new Thickness(20, 10, 0, 10),
                        Width = wth[i],
                        TextWrapping = TextWrapping.WrapWithOverflow,
                        //Формирую имя ячейки, чтобы проще было найти
                        Name = "cl" + i + "_rw" + j.ToString()
                    };

                    stackPanelRow[j].Children.Add(aHead[i]);
                    //Вешаю событие на ФИО
                    if (i == 2)
                    {
                        stackPanelRow[j].Children[i].MouseLeftButtonUp += new MouseButtonEventHandler(OpenAnswers);
                        stackPanelRow[j].Children[i].MouseEnter += new MouseEventHandler(CursorHand);
                        stackPanelRow[j].Children[i].MouseLeave += new MouseEventHandler(CursorArrow);
                    }


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
                            FROM [sr].[vwrep]
                            WHERE [usr_tn] = {_tn}";
            string res = MainWindow.SingleResult(sql, _mainConnectionString);
            string total = res.Split('_')[0];
            string success = res.Split('_')[1];
            LabelTrueAnswers.Content = $"Правельных ответов {success} из {total}";
        }

        private void OpenAnswers(object sender, EventArgs e)
        {
            string tn = ((TextBlock)sender).Name;
            int rw = int.Parse(tn.Split('_')[1].Replace("rw", "")); //строка
            int cl = int.Parse(tn.Split('_')[0].Replace("cl", "")); //колонка
            StackPanel stackPanelRow = (StackPanel)StackPanelQestions.Children[rw];
            tn = ((TextBlock)stackPanelRow.Children[0]).Text;
            string qstId = ((TextBlock)stackPanelRow.Children[1]).Text;
            string qstNm = ((TextBlock)stackPanelRow.Children[2]).Text;
            string rs = ((TextBlock)stackPanelRow.Children[3]).Text;
            string fio = LabelFio.Content.ToString();

            Answers qst = new Answers
            {
                Tn = tn,
                MainConnectionString = _mainConnectionString,
                Fio = fio,
                QstId = qstId,
                QstName = qstNm,
                Rs = rs
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
    }
}
