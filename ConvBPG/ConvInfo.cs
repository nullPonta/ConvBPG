

using System.IO;

namespace ConvBPG
{
    public class ConvInfo
    {
        public string TargetFilePath { get; set; }

        public string FileSize { get; set; }

        public string ConvFileSize { get; set; }

        public string Percentage { get; set; }

        public string Message { get; set; }


        public string GetBPG_Path() {

            string path = Path.GetDirectoryName(TargetFilePath) + @"\";
            path += Path.GetFileNameWithoutExtension(TargetFilePath) + ".bpg";

            return path;
        }

        public void UpdateFileSize() {
            FileInfo fileInfo = new FileInfo(TargetFilePath);
            FileSize = GetFormatSizeString(fileInfo.Length);
        }

        public void UpdateConvedSize() {

            if ((File.Exists(TargetFilePath) == false)
                || (File.Exists(GetBPG_Path()) == false)) {

                ConvFileSize = "Error : Missing File !";
                return;
            }

            long orgSize = new FileInfo(TargetFilePath).Length;
            long bpgSize = new FileInfo(GetBPG_Path()).Length;

            ConvFileSize = GetFormatSizeString(bpgSize);
            Percentage = (((float)bpgSize / (float)orgSize) * 100.0f).ToString("##0.#") + " %";
        }

        string GetFormatSizeString(float size) {
            string[] suffix = { "", "K", "M", "G", "T", "P", "E", "Z", "Y" };
            int p = 1024;
            int index = 0;

            while (size >= p) {
                size /= p;
                index++;
            }

            string specifier = "#,##0.#";

            return string.Format(
                "{0}{1}B",
                size.ToString(specifier),
                index < suffix.Length ? suffix[index] : "-");
        }

        public void DeleteTargetFile() {

            var bpgPath = GetBPG_Path();

            if ((File.Exists(TargetFilePath) == false)
                || (File.Exists(bpgPath) == false)) {

                Message = "Error : Missing File !";
                return;
            }

            if (new FileInfo(bpgPath).Length <= 1024) {

                File.Delete(bpgPath);
                Message = "Error : File Size Illegality !";

                return;
            }

            File.Delete(TargetFilePath);

            Message = "Completed.";
        }

    }

}
