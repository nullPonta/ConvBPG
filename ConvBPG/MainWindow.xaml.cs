using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace ConvBPG
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        TargetFiles targetFiles = new TargetFiles();


        public MainWindow() {
            InitializeComponent();

            dataGrid.ItemsSource = targetFiles.ConvInfos;
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

        private void button_Click(object sender, RoutedEventArgs e) {

            button.IsEnabled = false;
            clearButton.IsEnabled = false;

            var t = Task.Run(() => {
                ConvertToBPG();
            });
        }

        private void clearButton_Click(object sender, RoutedEventArgs e) {

            targetFiles.ConvInfos.Clear();
            dataGrid.Items.Refresh();
        }

        async void ConvertToBPG() {

            var sem = new SemaphoreSlim(8); // 最大同時実行数
            var convTasks = targetFiles.ConvInfos.Select(async info => {

                var t = await Task.Run(async () => {

                    await sem.WaitAsync();
                    Debug.WriteLine($"ConvertToBPG Start: {info.TargetFilePath}");

                    try {
                        var conv = new ConvertToBPG();
                        var cmdResult = await conv.StartCommandAsync(info);

                        /* Update Converted Size */
                        info.UpdateConvedSize();

                        /* Delete Target File */
                        info.DeleteTargetFile();

                        this.Dispatcher.Invoke((Action)(() => {
                            dataGrid.Items.Refresh();
                            Debug.WriteLine($"ConvertToBPG Refresh: {info.TargetFilePath}");
                        }));

                        return cmdResult;
                    }
                    finally {
                        Debug.WriteLine($"ConvertToBPG Completed: {info.TargetFilePath}");
                        sem.Release();
                    }
                });

                return t;
            });

            try {
                string[] results = await Task.WhenAll(convTasks);

                foreach (var result in results) {
                    Debug.WriteLine($"ConvertToBPG result: {result}");
                }

                this.Dispatcher.Invoke((Action)(() => {
                    button.IsEnabled = true;
                    clearButton.IsEnabled = true;

                    MessageBox.Show($"Completed. {results.Length} Files.");
                }));

            }
            catch (Exception e) {
                Debug.WriteLine("ConvertToBPG Exception : " + e);
            }

        }

    }

}
