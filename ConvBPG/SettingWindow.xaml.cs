using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;


namespace ConvBPG
{

    public partial class SettingWindow : Window
    {
        public delegate void SetExePath(SettingWindow settingWindow, string path);

        public delegate void SetDeleteOriginalFile(SettingWindow settingWindow, bool isDeleteOriginalFile);

        SetExePath setExePathCallback;

        SetDeleteOriginalFile setDeleteOriginalFileCallback;


        public SettingWindow(
            SetExePath setExePathCallback,
            SetDeleteOriginalFile setDeleteOriginalFileCallback) {
            
            InitializeComponent();

            this.setExePathCallback = setExePathCallback;
            this.setDeleteOriginalFileCallback = setDeleteOriginalFileCallback;

            /* Load from Properties */
            var bpgencExePath = Properties.Settings.Default.bpgencExePath;
            var isDeleteOriginalFile = Properties.Settings.Default.isDeleteOriginalFile;

            /* Set by Callback */
            setExePathCallback(this, bpgencExePath);
            setDeleteOriginalFileCallback(this, isDeleteOriginalFile);
        }

        public void UpdateBpgencExePath(string bpgencExePath) {
            exeTextBox.Text = bpgencExePath;

            Properties.Settings.Default.bpgencExePath = bpgencExePath;
        }

        public void UpdateIsDeleteOriginalFile(bool isDeleteOriginalFile) {
            isDeleteOriginalFileCheckBox.IsChecked = isDeleteOriginalFile;

            Properties.Settings.Default.isDeleteOriginalFile = isDeleteOriginalFile;
        }

        private void SelectBpgencExeButton_Click(object sender, RoutedEventArgs e) {

            var dialog = new OpenFileDialog();

            dialog.Filter = "exe file (*.exe)|*.exe";

            if (dialog.ShowDialog() == false) {
                return;
            }

            /* Set exe path */
            setExePathCallback(this, dialog.FileName);
        }

        private void isDeleteOriginalFileCheckBox_Checked(object sender, RoutedEventArgs e) {
            setDeleteOriginalFileCallback(this, isDeleteOriginalFileCheckBox.IsChecked.Value);
        }

        private void isDeleteOriginalFileCheckBox_Unchecked(object sender, RoutedEventArgs e) {
            setDeleteOriginalFileCallback(this, isDeleteOriginalFileCheckBox.IsChecked.Value);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;

            /* Save */
            Properties.Settings.Default.Save();

            this.Hide();
        }

    }
}
