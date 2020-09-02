namespace TextToStringBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using static Helper;

    // todo optionally split long lines

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Stack<string> _undoStack = new Stack<string>();

        readonly ComboBoxItem _outputModeFluentExecuteSql = new ComboBoxItem("Fluent Execute SQL Command");
        readonly ComboBoxItem _outputModeExecuteSql = new ComboBoxItem("Execute SQL Command");
        readonly ComboBoxItem _outputModeFluentVariable = new ComboBoxItem("Fluent SB Variable");
        readonly ComboBoxItem _outputModeVariable = new ComboBoxItem("SB Variable");
        readonly ComboBoxItem _outputModeVerbatimExecuteSql = new ComboBoxItem("Verbatim Execute String");

        public MainWindow()
        {
            InitializeComponent();

            CbOutputMode.Items.Add(_outputModeExecuteSql);
            CbOutputMode.Items.Add(_outputModeFluentExecuteSql);
            CbOutputMode.Items.Add(_outputModeVariable);
            CbOutputMode.Items.Add(_outputModeFluentVariable);
            CbOutputMode.Items.Add(_outputModeVerbatimExecuteSql);

            CbOutputMode.SelectionChanged += ComboBoxSelectionChanged;

            CbOutputMode.SelectedItem = _outputModeExecuteSql;
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SPSBVariableName.IsEnabled = ((ComboBox) sender).SelectedItem != _outputModeFluentExecuteSql;
        }

        private void BtnToStringBuilder_Click(object sender, RoutedEventArgs e)
        {
            var sbName = TbStringBuilder.Text;

            if (string.IsNullOrEmpty(sbName))
            {
                sbName = "sql";
            }

            string data = TextBox.Text;
            _undoStack.Push(data);

            if (CbOutputMode.SelectedItem == _outputModeFluentExecuteSql)
            {
                _undoStack.Push(data);
                data = CreateFluentExecuteSqlCommand(data);
                TextBox.Text = data;
            }

            if (CbOutputMode.SelectedItem == _outputModeExecuteSql)
            {
                _undoStack.Push(data);
                data = CreateExecuteSqlCommand(data, sbName);
                TextBox.Text = data;
            }

            if (CbOutputMode.SelectedItem == _outputModeFluentVariable)
            {
                _undoStack.Push(data);
                data = CreateFluentStringBuilderVariable(data, sbName);
                TextBox.Text = data;
            }

            if (CbOutputMode.SelectedItem == _outputModeVariable)
            {
                _undoStack.Push(data);
                data = CreateStringBuilderVariable(data, sbName);
                TextBox.Text = data;
            }

            if (CbOutputMode.SelectedItem == _outputModeVerbatimExecuteSql)
            {
                _undoStack.Push(data);
                data = CreateVerbatimStringExecuteSQLVariable(data, sbName);
                TextBox.Text = data;
            }
        }

        private void BtnExtractQuoted_Click(object sender, RoutedEventArgs e)
        {
            string data = TextBox.Text;

            _undoStack.Push(data);
            data = UnwrapAppends(data);
            TextBox.Text = data;
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
            {
                TextBox.Text = _undoStack.Pop();
            }
        }

        private string CreateFluentStringBuilderVariable(string data, string stringBuilderName)
        {
            string lines = CreateStringBuilderAppendLines(data, "    ");

            var sb = new StringBuilder()
                .Append($"var {stringBuilderName} = new StringBuilder()")
                .Append(lines?.Length > 0 ? Environment.NewLine : string.Empty)
                .Append(lines ?? string.Empty)
                .AppendLine(";");

            return sb.ToString();
        }

        private string CreateFluentExecuteSqlCommand(string data)
        {
            string lines = CreateStringBuilderAppendLines(data, "    ");

            var sb = new StringBuilder()
                .Append("Execute.Sql(new StringBuilder()")
                .Append(lines?.Length > 0 ? Environment.NewLine : string.Empty)
                .Append(lines ?? string.Empty)
                .Append(lines?.Length > 0 ? Environment.NewLine : string.Empty)
                .Append(lines?.Length > 0 ? "    " : string.Empty)
                .AppendLine(".ToString());");

            return sb.ToString();
        }

        private string CreateStringBuilderVariable(string data, string stringBuilderName)
        {
            string lines = CreateStringBuilderAppendLines(data, stringBuilderName, ";");

            var sb = new StringBuilder()
                .Append($"var {stringBuilderName} = new StringBuilder();")
                .Append(lines?.Length > 0 ? Environment.NewLine : string.Empty)
                .Append(lines ?? string.Empty)
                .AppendLine(lines?.Length > 0 ? string.Empty : ";");
            return sb.ToString();
        }

        private string CreateExecuteSqlCommand(string data, string stringBuilderName)
        {
            string lines = CreateStringBuilderAppendLines(data, stringBuilderName, ";");

            var sb = new StringBuilder()
                .Append($"var {stringBuilderName} = new StringBuilder();")
                .Append(lines?.Length > 0 ? Environment.NewLine : string.Empty)
                .Append(lines ?? string.Empty)
                .AppendLine(lines?.Length > 0 ? string.Empty : ";")
                .AppendLine($"Execute.Sql({stringBuilderName}.ToString());");
            return sb.ToString();
        }

        private string CreateVerbatimStringExecuteSQLVariable(string data, string stringBuilderName)
        {
            string lines = CreateVerbatimStringData(data);

            var sb = new StringBuilder()
                .Append($"const string {stringBuilderName} = @\"")
                .Append(lines?.Length > 0 ? Environment.NewLine : string.Empty)
                .Append(lines ?? string.Empty)
                //.Append(lines?.Length > 0 ? Environment.NewLine : string.Empty)
                .AppendLine("\";")
                .AppendLine()
                .AppendLine($"Execute.Sql({stringBuilderName});");

            return sb.ToString();
        }

        private class ComboBoxItem
        {
            public ComboBoxItem(string title)
            {
                Title = title;
            }

            public string Title { get; set; }

            public override string ToString()
            {
                return Title;
            }
        }
    }
}
