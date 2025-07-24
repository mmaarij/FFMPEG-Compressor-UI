using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace FFMPEG_Compressor
{
    public partial class MainWindow : Window
    {
        string inputFile = "";
        string outputFolder = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseInput_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Video files|*.mp4;*.avi;*.mkv;*.mov"
            };
            if (dialog.ShowDialog() == true)
            {
                inputFile = dialog.FileName;
                txtInputPath.Text = inputFile;
            }
        }

        private void BrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                outputFolder = dialog.SelectedPath;
                txtOutputPath.Text = outputFolder;
            }
        }

        private async void Compress_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(outputFolder))
            {
                System.Windows.MessageBox.Show("Please select both input file and output folder.");
                return;
            }

            SetUIEnabled(false);
            string scaleFilter = GetScaleFilter();
            int crf = (int)sliderCrf.Value;
            string outputFile = Path.Combine(outputFolder, $"compressed_{Path.GetFileName(inputFile)}");

            txtResult.Text = "🔄 Compressing... please wait.\n";

            try
            {
                await Task.Run(() =>
                {
                    var process = new Process();
                    process.StartInfo.FileName = "ffmpeg.exe";
                    process.StartInfo.Arguments = $"-i \"{inputFile}\" -vf \"{scaleFilter}\" -c:v libx264 -preset fast -crf {crf} -y \"{outputFile}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;

                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            AppendToLog(e.Data);
                        }
                    };

                    process.OutputDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            AppendToLog(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    process.WaitForExit();
                });

                if (File.Exists(outputFile))
                {
                    long inputSize = new FileInfo(inputFile).Length / 1024 / 1024;
                    long outputSize = new FileInfo(outputFile).Length / 1024 / 1024;

                    AppendToLog($"\n✅ Compression complete!\nInput: {inputSize} MB\nOutput: {outputSize} MB\nCRF: {crf}\nScale: {scaleFilter}");
                }
                else
                {
                    AppendToLog("❌ Compression failed. Check ffmpeg.exe path and input file.");
                }
            }
            catch (Exception ex)
            {
                AppendToLog($"❌ Error: {ex.Message}");
            }
            finally
            {
                SetUIEnabled(true);
            }
        }



        private string GetScaleFilter()
        {
            if (rb34.IsChecked == true)
                return "scale=iw*3/4:ih*3/4";
            else if (rb12.IsChecked == true)
                return "scale=iw/2:ih/2";
            else if (rb14.IsChecked == true)
                return "scale=iw/4:ih/4";
            else
                return "scale=iw/2:ih/2"; // default fallback
        }

        private void SetUIEnabled(bool isEnabled)
        {
            btnBrowseInput.IsEnabled = isEnabled;
            btnBrowseOutput.IsEnabled = isEnabled;
            btnCompress.IsEnabled = isEnabled;
            sliderCrf.IsEnabled = isEnabled;
            rb34.IsEnabled = isEnabled;
            rb12.IsEnabled = isEnabled;
            rb14.IsEnabled = isEnabled;
        }

        private void AppendToLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtResult.Text += message + "\n";
                txtResult.ScrollToEnd();
            });
        }


    }
}
