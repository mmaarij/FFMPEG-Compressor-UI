using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace FFMPEG_Compressor
{
    public partial class MainWindow : Window
    {
        string inputFile = "";
        string outputFolder = "";

        private DispatcherTimer _positionTimer = new DispatcherTimer();
        private bool _isSeekSliderDragging = false;

        public MainWindow()
        {
            InitializeComponent();

            _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
            _positionTimer.Tick += PositionTimer_Tick;
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
                LoadPreview_Click(sender, e);
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
            string scaleArg = string.IsNullOrEmpty(scaleFilter) ? "" : $"-vf \"{scaleFilter}\"";
            int crf = (int)sliderCrf.Value;
            string outputFile = Path.Combine(outputFolder, $"compressed_{Path.GetFileName(inputFile)}");
            double start = rangeSlider.LowerValue;
            double end = rangeSlider.HigherValue;

            txtResult.Text = "🔄 Compressing... please wait.\n";

            try
            {
                await Task.Run(() =>
                {
                    var process = new Process();
                    process.StartInfo.FileName = "ffmpeg.exe";
                    process.StartInfo.Arguments = $"-i \"{inputFile}\" -ss {start} -to {end} {scaleArg} -c:v libx264 -preset fast -crf {crf} -y \"{outputFile}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;

                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            AppendToLog(e.Data);
                    };
                    process.OutputDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            AppendToLog(e.Data);
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
                    string scaleDisplay = string.IsNullOrEmpty(scaleFilter) ? "None (Original)" : scaleFilter;

                    AppendToLog(
                        $"\n✅ Compression complete!" +
                        $"\nInput Size: {inputSize} MB" +
                        $"\nOutput Size: {outputSize} MB" +
                        $"\nCRF: {crf}" +
                        $"\nScale: {scaleDisplay}" +
                        $"\nStart Time: {start:0.##}s" +
                        $"\nEnd Time: {end:0.##}s" +
                        $"\nDuration: {end-start:0.##}s"
                    );
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

        private void LoadPreview_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(txtInputPath.Text))
            {
                mediaPreview.Source = new Uri(txtInputPath.Text);
                mediaPreview.Position = TimeSpan.Zero;
                mediaPreview.LoadedBehavior = MediaState.Manual;
                mediaPreview.UnloadedBehavior = MediaState.Manual;

                mediaPreview.MediaOpened += (s, args) =>
                {
                    double totalSeconds = mediaPreview.NaturalDuration.TimeSpan.TotalSeconds;
                    rangeSlider.Maximum = totalSeconds;
                    rangeSlider.LowerValue = 0;
                    rangeSlider.HigherValue = totalSeconds;

                    seekSlider.Maximum = totalSeconds;
                    seekSlider.Value = 0;

                    _positionTimer.Start();
                };

                mediaPreview.Play();
                mediaPreview.Pause();
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPreview.Source != null)
            {
                mediaPreview.Play();
                _positionTimer.Start();
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPreview.Source != null)
            {
                mediaPreview.Pause();
                _positionTimer.Stop();
            }
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (mediaPreview.Source != null && mediaPreview.NaturalDuration.HasTimeSpan)
            {
                double current = mediaPreview.Position.TotalSeconds;
                if (!_isSeekSliderDragging)
                    seekSlider.Value = current;

                TimeSpan currentTime = mediaPreview.Position;
                TimeSpan totalTime = mediaPreview.NaturalDuration.TimeSpan;
                txtVideoTime.Text = $"{FormatTime(currentTime)} / {FormatTime(totalTime)}";

                if (current >= rangeSlider.HigherValue)
                {
                    mediaPreview.Pause();
                    _positionTimer.Stop();
                }
            }
        }


        private void SeekSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isSeekSliderDragging = true;
        }

        private void SeekSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isSeekSliderDragging = false;

            // Seek to the new position
            mediaPreview.Position = TimeSpan.FromSeconds(seekSlider.Value);

            if (mediaPreview.LoadedBehavior == MediaState.Manual && mediaPreview.Clock == null)
            {
                if (mediaPreview.CanPause)
                {
                    // Trick to force update: play a frame then pause
                    mediaPreview.Play();

                    Dispatcher.InvokeAsync(async () =>
                    {
                        await Task.Delay(100); // Let it render a frame
                        mediaPreview.Pause();
                    });
                }
            }
        }


        private void SetStart_Click(object sender, RoutedEventArgs e)
        {
            var currentPosition = seekSlider.Value;
            if (currentPosition < rangeSlider.HigherValue) // Ensure start is before end
            {
                rangeSlider.LowerValue = currentPosition;
            }
            else
            {
                System.Windows.MessageBox.Show("Start time must be less than end time.", "Invalid Range", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SetEnd_Click(object sender, RoutedEventArgs e)
        {
            var currentPosition = seekSlider.Value;
            if (currentPosition > rangeSlider.LowerValue) // Ensure end is after start
            {
                rangeSlider.HigherValue = currentPosition;
            }
            else
            {
                System.Windows.MessageBox.Show("End time must be greater than start time.", "Invalid Range", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return time.ToString(time.Hours > 0 ? @"h\:mm\:ss" : @"mm\:ss");
        }


        private string GetScaleFilter()
        {
            if (rbNone.IsChecked == true)
                return null; // No scaling
            else if (rb34.IsChecked == true)
                return "scale=iw*3/4:ih*3/4";
            else if (rb12.IsChecked == true)
                return "scale=iw/2:ih/2";
            else if (rb14.IsChecked == true)
                return "scale=iw/4:ih/4";
            else
                return null; // Default fallback
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
