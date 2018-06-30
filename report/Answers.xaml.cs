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
    /// Логика взаимодействия для Answers.xaml
    /// </summary>
    public partial class Answers : Window
    {
        public Answers()
        {
            InitializeComponent();
        }

        private string _mainConnectionString;
        private string _qstId;
        private string _qstName;
        private string _fio;
        private string _tn;
        private string _rs;

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

        public string QstId
        {
            get => _qstId;
            set => _qstId = value;
        }

        public string QstName
        {
            get => _qstName;
            set => _qstName = value;
        }

        public string Tn
        {
            get => _tn;
            set => _tn = value;
        }

        public string Rs
        {
            get => _rs;
            set => _rs = value;
        }

        private void CreateTable()
        {

            LabelFio.Content = _fio;

            int font = 16; // Размер шрифта
            const int cl = 5; //Кол-во колонок
            string[] headName = new string[cl] { "Тип", "Ответ", "Верный", "Выбор", "Результат" };
            int[] wth = new int[cl] { 0, 500, 100, 100, 100 };
            int[] marginLeft = new int[cl] { 0, 40, 20, 0, 20 };
            TextBlock[] aHead = new TextBlock[cl];

            string sql = $"SELECT Count(*) FROM [dbo].[anw] WHERE [anw].[qst_id] = {_qstId};";
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
            sql = $@"SELECT 
                    [qst].[qst_tp]
                    ,[anw_nm]
                    ,[anw_true]
                    ,CASE [qst].[qst_tp] 
	                    WHEN 'text' 
	                    THEN 
	                    (SELECT [rez].[rez_vl] FROM [rez] WHERE [rez].[anw_id] = [anwMain].[anw_id] AND [rez].[usr_tn] = {_tn})
	                    ELSE
		                    (SELECT [rez].[anw_id] FROM [rez] WHERE [rez].[anw_id] = [anwMain].[anw_id] AND [rez].[usr_tn] = {_tn})
	                    END as answer
	                ,
	                CASE [qst].[qst_tp] 
	                WHEN 'text' 
	                THEN
		                CASE [anw_true]
		                WHEN (SELECT [rez].[rez_vl] FROM [rez] WHERE [rez].[anw_id] = [anwMain].[anw_id] AND [rez].[usr_tn] = {_tn})
		                THEN 1
		                ELSE 0
		                END
	                ELSE
		                CASE [anw_true]
		                WHEN 1
		                THEN
			                CASE [anwMain].[anw_id]
			                WHEN (SELECT [rez].[anw_id] FROM [rez] WHERE [rez].[anw_id] = [anwMain].[anw_id] AND [rez].[usr_tn] = {_tn})
			                THEN 1
			                ELSE 0
			                END
		                ELSE 0
		                END
	                END as result
                FROM [anw] as [anwMain]
                INNER JOIN [qst] ON [qst].[qst_id] = [anwMain].[qst_id]
                WHERE [qst].[qst_id] = {_qstId} ";
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

            string tp = null;
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

                    switch (cell)
                    {
                        case "text":
                            tp = "text";
                            break;
                        case "radio":
                            tp = "radio";
                            break;
                        case "check":
                            tp = "check";
                            break;
                    }

                    //tp = cell == "text" ? "text" : cell == "radio"?"radio": cell == "check"?"check":"";
                    //Меняю цвет StackPanel
                    if (cell == "0") stackPanelRow[j].Background = Brushes.AntiqueWhite;
                    else stackPanelRow[j].Background = Brushes.LightGreen;
                    if (i == 2 & tp != "text")
                    {
                        //Заменяю 1 и 0 Правильно/Не правильно
                        if (cell == "1") cell = "Да";
                        if (cell == "0") cell = "Нет";
                    }
                    if (i == 3 & !string.IsNullOrEmpty(cell) & tp != "text")
                    {
                        cell = "Да";
                    }
                    if (i == 4)
                    {
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
                    };

                    stackPanelRow[j].Children.Add(aHead[i]);
                    i++;
                }
                StackPanelAnswers.Children.Add(stackPanelRow[j]);
                j++;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateTable();
            TrueAnswers();
            Question();
        }

        private void TrueAnswers()
        {
            LabelTrueAnswers.Content = _rs;
            if (_rs == "Верно") LabelTrueAnswers.Background = Brushes.LightGreen;
            if (_rs == "Не верно") LabelTrueAnswers.Background = Brushes.AntiqueWhite;

        }

        private void Question()
        {

            TextBlockQuestion.Text = _qstName;
        }

    }
}
