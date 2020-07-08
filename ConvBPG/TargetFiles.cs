using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvBPG
{
    class TargetFiles
    {
        public List<ConvInfo> ConvInfos = new List<ConvInfo>();


        public void AddTargetFiles(string path) {

            if ((Directory.Exists(path) == false)
                && (File.Exists(path) == false)) {
                
                return;
            }

            /* Add file path */
            var isDirectory = File.GetAttributes(path).HasFlag(FileAttributes.Directory);

            if (isDirectory == false) {
                AddFilePathToTargetFiles(path);
                return;
            }

            /* Add sub directory file path */
            string[] allFilePaths = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            foreach (string subPath in allFilePaths) {
                AddFilePathToTargetFiles(subPath);
            }

        }

        void AddFilePathToTargetFiles(string filePath) {

            if (IsTargetFile(filePath) == false) {
                return;
            }

            var info = new ConvInfo();
            
            info.TargetFilePath = filePath;
            info.UpdateFileSize();

            ConvInfos.Add(info);
        }

        bool IsTargetFile(string filePath) {

            var extension = Path.GetExtension(filePath);

            if (extension.Equals(".jpg")
                || extension.Equals(".png")) {

                return true;
            }

            return false;
        }

        

    }
}
