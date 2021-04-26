using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ConvBPG
{

    public class ConvertToBPG_Parallel
    {
        static CancellationTokenSource cts;

        public static bool isDeleteOriginalFile;

        public static int quantizerValue;


        public static void RefreshCancellationTokenSource() {
            cts?.Dispose(); // Clean up old token source.
            cts = new CancellationTokenSource();
        }

        public static void Cancel() {
            cts.Cancel();
        }

        public static async void Convert(TargetFiles targetFiles, MainWindow mainWindow) {

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
                        cmdResult = await conv.StartCommandAsync(info, quantizerValue);

                        /* Update Converted Size */
                        info.UpdateConvedSize();

                        if (info.BpgencSuccess) {

                            if (isDeleteOriginalFile) {
                                /* Delete Target File */
                                info.DeleteTargetFile();
                            }
                            else {
                                info.SetConvertCompletedMessage();
                            }
                        }

                        return cmdResult + info.TargetFilePath;
                    }
                    finally {
                        Debug.WriteLine($"ConvertToBPG Completed: {info.TargetFilePath} : {cmdResult}");
                        sem.Release();

                        /* Refresh view */
                        mainWindow.Dispatcher.Invoke((Action)(() => {
                            mainWindow.dataGrid.Items.Refresh();
                            mainWindow.ScrollToVerticalOffset(offset);
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

                mainWindow.Dispatcher.Invoke((Action)(() => {
                    mainWindow.startButton.Content = "Start";
                    mainWindow.startButton.IsEnabled = true;
                    mainWindow.clearButton.IsEnabled = true;

                    MessageBox.Show($"Completed. {results.Length} Files.");
                }));

            }
            catch (Exception e) {
                Debug.WriteLine("ConvertToBPG Exception : " + e);
            }

        }

    }

}
