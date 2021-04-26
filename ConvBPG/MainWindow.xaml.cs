using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        CancellationTokenSource cts;


        public MainWindow() {
            InitializeComponent();

            dataGrid.ItemsSource = targetFiles.ConvInfos;

            /* Setting */
            settingWindow = new SettingWindow(SetExePath);
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
                    e.Column.Header = "File Size";
                    e.Column.DisplayIndex = 1;
                    break;
                case "ConvFileSize":
                    e.Column.Header = "Conv File Size";
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

                cts?.Dispose(); // Clean up old token source.
                cts = new CancellationTokenSource();

                var t = Task.Run(() => {
                    ConvertToBPG_Parallel();
                });
            }
            else {
                startButton.Content = "Start";
                startButton.IsEnabled = false;
                clearButton.IsEnabled = false;

                Debug.WriteLine($"button_Click : Stop");
                cts.Cancel();
            }

        }

        private void clearButton_Click(object sender, RoutedEventArgs e) {

            targetFiles.ConvInfos.Clear();
            dataGrid.Items.Refresh();
        }

        private void settingButton_Click(object sender, RoutedEventArgs e) {

            settingWindow.Show();
        }

        async void ConvertToBPG_Parallel() {

            var cpuCount = Environment.ProcessorCount;
            var sem = new SemaphoreSlim(cpuCount);
            int itemCount = 0;

            var convTasks = targetFiles.ConvInfos.Select(async info => {
                var token = cts.Token;

                itemCount++;
                double offset = itemCount - cpuCount;

                var t = await Task.Run(async () => {

                    string cmdResult = null;

                    try {
                        /* Wait and Cancel */
                        //Debug.WriteLine($"ConvertToBPG Run: {info.TargetFilePath}");
                        await sem.WaitAsync();
                        if (token.IsCancellationRequested) {
                            Debug.WriteLine($"ConvertToBPG Cancel: {info.TargetFilePath}");
                            cmdResult = "Canceled";
                            info.Message = cmdResult;
                            return cmdResult + info.TargetFilePath;
                        }
                        Debug.WriteLine($"ConvertToBPG Start: {info.TargetFilePath}");

                        /* Start Command */
                        var conv = new ConvertToBPG();
                        cmdResult = await conv.StartCommandAsync(info);

                        /* Update Converted Size */
                        info.UpdateConvedSize();

                        /* Delete Target File */
                        info.DeleteTargetFile();

                        return cmdResult + info.TargetFilePath;
                    }
                    finally {
                        Debug.WriteLine($"ConvertToBPG Completed: {info.TargetFilePath} : {cmdResult}");
                        sem.Release();

                        /* Refresh view */
                        this.Dispatcher.Invoke((Action)(() => {
                            dataGrid.Items.Refresh();
                            ScrollToVerticalOffset(offset);
                            Debug.WriteLine($"offset : {offset}");
                        }));
                    }
                }, token);

                return t;
            });

            try {
                string[] results = await Task.WhenAll(convTasks);

                foreach (var result in results) {
                    Debug.WriteLine($"ConvertToBPG result: {result}");
                }

                this.Dispatcher.Invoke((Action)(() => {
                    startButton.Content = "Start";
                    startButton.IsEnabled = true;
                    clearButton.IsEnabled = true;

                    MessageBox.Show($"Completed. {results.Length} Files.");
                }));

            }
            catch (Exception e) {
                Debug.WriteLine("ConvertToBPG Exception : " + e);
            }

        }

        void ScrollToVerticalOffset(double offset) {

            if (dataGrid.Items.Count <= 0) { return; }

            var border = VisualTreeHelper.GetChild(dataGrid, 0) as Decorator;
            if (border == null) { return; }

            var scroll = border.Child as ScrollViewer;
            if (scroll != null) scroll.ScrollToVerticalOffset(offset);

        }

        void SetExePath(SettingWindow settingWindow, string bpgencExePath) {

            settingWindow.SetBpgencExePath(bpgencExePath);
            ConvertToBPG.bpgencPath = bpgencExePath;
        }

    }

}
