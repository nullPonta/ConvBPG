using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;


namespace ConvBPG
{

    public partial class SettingWindow : Window
    {
        public delegate void SetExePath(SettingWindow settingWindow, string path);

        SetExePath setExePathCallback;

        string bpgencExePath;


        public SettingWindow(SetExePath setExePathCallback) {
            InitializeComponent();

            this.setExePathCallback = setExePathCallback;

            /* Load */
            var bpgencExePath = Properties.Settings.Default.bpgencExePath;

            setExePathCallback(this, bpgencExePath);
        }

        public void SetBpgencExePath(string bpgencExePath) {
            this.bpgencExePath = bpgencExePath;
            exeTextBox.Text = bpgencExePath;

            Properties.Settings.Default.bpgencExePath = bpgencExePath;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;

            /* Save */
            Properties.Settings.Default.Save();

            this.Hide();
        }
    }
}
