using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace ConvBPG
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        TargetFiles targetFiles = new TargetFiles();

        SettingWindow settingWindow;
        

        public MainWindow() {
            InitializeComponent();

            dataGrid.ItemsSource = targetFiles.ConvInfos;

            /* Setting */
            settingWindow = new SettingWindow(SetExePath, SetDeleteOriginalFile, SetQuantizerValue);
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e) {

        }

        private void textBox_PreviewDragOver_1(object sender, DragEventArgs e) {

            if (e.Data.GetDataPresent(DataFormats.FileDrop, true)) {
                e.Effects = DragDropEffects.Copy;
            }
            else {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void textBox_PreviewDrop(object sender, DragEventArgs e) {

            var dropFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (dropFiles == null) return;

            foreach (string path in dropFiles) {
                targetFiles.AddTargetFiles(path);
            }

            dataGrid.Items.Refresh();
        }

        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) {

            switch (e.PropertyName) {
                case "TargetFilePath":
                    e.Column.Header = "Target File Path";
                    e.Column.DisplayIndex = 0;
                    break;
                case "FileSize":
                    e.Column.Header = "Original File Size";
                    e.Column.DisplayIndex = 1;
                    break;
                case "ConvFileSize":
                    e.Column.Header = "Converted File Size";
                    e.Column.DisplayIndex = 2;
                    break;
                case "Percentage":
                    e.Column.Header = "Percentage";
                    e.Column.DisplayIndex = 3;
                    break;
                case "Message":
                    e.Column.Header = "Message";
                    e.Column.DisplayIndex = 4;
                    break;
                default:
                    break;
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e) {

            if (startButton.Content.Equals("Start")) {

                startButton.Content = "Stop";
                clearButton.IsEnabled = false;

                ConvertToBPG_Parallel.RefreshCancellationTokenSource();

                var t = Task.Run(() => {
                    ConvertToBPG_Parallel.Convert(targetFiles, this);
                });
            }
            else {
                startButton.Content = "Start";
                startButton.IsEnabled = false;
                clearButton.IsEnabled = false;

                Debug.WriteLine($"button_Click : Stop");
                ConvertToBPG_Parallel.Cancel();
            }

        }

        private void clearButton_Click(object sender, RoutedEventArgs e) {

            targetFiles.ConvInfos.Clear();
            dataGrid.Items.Refresh();
        }

        private void settingButton_Click(object sender, RoutedEventArgs e) {

            settingWindow.Show();
        }

        public void ScrollToVerticalOffset(double offset) {

            if (dataGrid.Items.Count <= 0) { return; }

            var border = VisualTreeHelper.GetChild(dataGrid, 0) as Decorator;
            if (border == null) { return; }

            var scroll = border.Child as ScrollViewer;
            if (scroll != null) scroll.ScrollToVerticalOffset(offset);

        }

        void SetExePath(SettingWindow settingWindow, string bpgencExePath) {

            settingWindow.UpdateBpgencExePath(bpgencExePath);
            ConvertToBPG.bpgencPath = bpgencExePath;
        }

        void SetDeleteOriginalFile(SettingWindow settingWindow, bool isDeleteOriginalFile) {

            ConvertToBPG_Parallel.isDeleteOriginalFile = isDeleteOriginalFile;
            settingWindow.UpdateIsDeleteOriginalFile(isDeleteOriginalFile);
        }

        void SetQuantizerValue(SettingWindow settingWindow, int quantizerValue) {

            ConvertToBPG_Parallel.quantizerValue = quantizerValue;
            settingWindow.UpdateQuantizerValue(quantizerValue);
        }

    }

}
