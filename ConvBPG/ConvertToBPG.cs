using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace ConvBPG
{
    class ConvertToBPG
    {
        public static string bpgencPath;

        public string StandardOutputResult;
        public string StandardErrorResult;


        public async Task<string> StartCommandAsync(ConvInfo convInfo, int quantizerValue) {
            //Processを非同期に実行
            using (Process process = GetProcess(convInfo, quantizerValue)) {
                if (process == null) {
                    convInfo.BpgencSuccess = false;
                    convInfo.Message = "Could not start bpgenc !";
                    return "Error : GetProcess Failure !";
                }

                return await StartCommandAsync(process, convInfo);
            }
        }

        private async Task<string> StartCommandAsync(Process process, ConvInfo convInfo) {
            using (var cts = new CancellationTokenSource()) {
                bool started = false;

                process.Exited += (sender, args) => {
                    //Console.WriteLine($"exited");
                    while( string.IsNullOrEmpty(StandardOutputResult) ||
                        string.IsNullOrEmpty(StandardErrorResult) ) {

                        Task.Delay(0);
                    }

                    cts.Cancel();
                };

                process.OutputDataReceived += (sender, args) => {
                    StandardOutputResult += $"OutputDataReceived : {args.Data}";
                };

                process.ErrorDataReceived += (sender, args) => {
                    StandardErrorResult += $"ErrorDataReceived : {args.Data}";
                };

                //プロセスからの情報を受け取る変数の初期化
                StandardOutputResult = "";
                StandardErrorResult = "";
                await Task.Delay(0);

                try {
                    //プロセスの開始
                    started = process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    convInfo.BpgencSuccess = true;
                }
                catch (Exception e) {
                    Console.WriteLine($"Exception={e}");
                }

                cts.Token.WaitHandle.WaitOne();
            }

            return StandardOutputResult + StandardErrorResult;
        }

        Process GetProcess(ConvInfo convInfo, int quantizerValue) {

            if ((File.Exists(bpgencPath) == false)
                || (File.Exists(convInfo.TargetFilePath) == false)) {
                return null;
            }

            var p = GetProcess(bpgencPath);

            string arg = "\"" + convInfo.TargetFilePath + "\" -o \"" + convInfo.GetBPG_Path() + "\"";
            arg += " -q " + quantizerValue.ToString();

            Debug.WriteLine("arg : " + arg);

            p.StartInfo.Arguments = arg;

            return p;
        }

        Process GetProcess(string exePath) {

            Process p = new Process();

            p.StartInfo.FileName = exePath;

            //出力を読み取れるようにする
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = false;

            //ウィンドウを表示しないようにする
            p.StartInfo.CreateNoWindow = true;

            p.EnableRaisingEvents = true;

            return p;
        }

    }
}
