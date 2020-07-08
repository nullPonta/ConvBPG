using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConvBPG
{
    class ConvertToBPG
    {
        string result;


        public async Task<string> StartCommandAsync(ConvInfo convInfo) {
            //Processを非同期に実行
            using (Process process = GetProcess(convInfo)) {
                if (process == null) { return null;  }

                return await StartCommandAsync(process);
            }
        }

        private async Task<string> StartCommandAsync(Process process) {
            using (var ctoken = new CancellationTokenSource()) {
                bool started = false;

                process.Exited += (sender, args) => {
                    Console.WriteLine($"exited");
                    ctoken.Cancel();
                };

                process.OutputDataReceived += (sender, args) => {
                    if (!string.IsNullOrEmpty(args.Data)) {
                        Console.WriteLine($"stdout={args.Data}");
                        result += $"{args.Data}\n";
                    }
                };

                process.ErrorDataReceived += (sender, args) => {
                    if (!string.IsNullOrEmpty(args.Data)) {
                        Console.WriteLine($"stderr={args.Data}");
                        result += $"Error : {args.Data}\n";
                    }
                };

                //プロセスからの情報を受け取る変数の初期化
                result = "";
                await Task.Delay(0);

                try {
                    //プロセスの開始
                    started = process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                catch (Exception e) {
                    Console.WriteLine($"Exception={e}");
                }

                ctoken.Token.WaitHandle.WaitOne();
            }

            return result;
        }

        Process GetProcess(ConvInfo convInfo) {
            string bpgencPath = @"C:\_APP\_Image\bpg-0.9.8-win64\bpgenc.exe";

            if ((File.Exists(bpgencPath) == false)
                || (File.Exists(convInfo.TargetFilePath) == false)) { return null; }

            var p = GetProcess(bpgencPath);
            string arg = "\"" + convInfo.TargetFilePath + "\" -o \"" + convInfo.GetBPG_Path() + "\"";

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
