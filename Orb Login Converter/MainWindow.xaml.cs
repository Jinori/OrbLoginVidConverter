using System.IO;
using System.Windows;
using Microsoft.Win32;
using OpenCvSharp;
using SkiaSharp;
using DALib.Drawing;
using DALib.Utility;

namespace Orb_Login_Converter
{
    public partial class MainWindow
    {
        private string _videoPath;
        private string _framesFolder;
        private string _outputFolder;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                this.DragMove();
            }
        }

        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        // Handles the Load Video button click event
        private void LoadVideo_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.flv|All Files|*.*",
                Title = "Select a Video File"
            };

            if (openFileDialog.ShowDialog() != true) 
                return;
            
            _videoPath = openFileDialog.FileName;
            VideoPathTextBox.Text = _videoPath;
            Log($"Video loaded: {_videoPath}");
        }

        // Handles the Process Video button click event
        private void ProcessVideo_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();

            if (string.IsNullOrWhiteSpace(_videoPath))
            {
                Log("Please load a video file first.");
                return;
            }

            InitializeDirectories();

            Log("Processing started...");

            try
            {
                ProcessVideo();
                Log("Processing completed!");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        // Initializes the directories for frames and output
        private void InitializeDirectories()
        {
            _framesFolder = CreateDirectory("frames");
            _outputFolder = CreateDirectory("output_frames");
        }

        // Main video processing logic
        private void ProcessVideo()
        {
            ExtractFrames(_videoPath, _framesFolder);
            using var logo = LoadLogo();
            var mask = CreateCircularMask(logo.Width, logo.Height);
            ApplyMaskToFrames(_framesFolder, _outputFolder, mask, logo.Width, logo.Height);
            GenerateSpfAndPal(_outputFolder);
        }

        // Loads the embedded logo
        private SKBitmap LoadLogo()
        {
            var resourceUri = new Uri("pack://application:,,,/logo000.png", UriKind.Absolute);
            var streamInfo = Application.GetResourceStream(resourceUri)
                ?? throw new FileNotFoundException("Embedded logo file not found.");

            using var stream = streamInfo.Stream;
            return SKBitmap.Decode(stream);
        }

        // Logs a message to the TextBox
        private void Log(string message)
        {
            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        }

        // Creates a directory and ensures it exists
        private string CreateDirectory(string name)
        {
            var path = Path.Combine(Environment.CurrentDirectory, name);
            Directory.CreateDirectory(path);
            return path;
        }

        // Extracts frames from the video
        private void ExtractFrames(string videoPath, string framesFolder)
        {
            using var capture = new VideoCapture(videoPath);

            if (!capture.IsOpened())
                throw new Exception("Unable to open video file.");

            int totalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount);
            int interval = totalFrames / 20;

            for (int i = 0; i < 20; i++)
            {
                capture.Set(VideoCaptureProperties.PosFrames, i * interval);
                using var frame = new Mat();

                if (capture.Read(frame))
                {
                    var frameFilename = Path.Combine(framesFolder, $"frame_{i + 1:02d}.png");
                    frame.SaveImage(frameFilename);
                    Log($"Frame saved: {frameFilename}");
                }
            }
        }

        // Creates a circular mask
        private SKBitmap CreateCircularMask(int width, int height)
        {
            var mask = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            using var canvas = new SKCanvas(mask);
            canvas.Clear(SKColors.Transparent);

            var paint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            canvas.DrawOval(new SKRect(0, 0, width, height), paint);
            return mask;
        }

        // Applies the mask to the frames
        private void ApplyMaskToFrames(string framesFolder, string outputFolder, SKBitmap mask, int width, int height)
        {
            foreach (var filePath in Directory.GetFiles(framesFolder, "*.png"))
            {
                using var original = SKBitmap.Decode(filePath);
                using var resizedFrame = original.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
                using var maskedImage = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

                using var canvas = new SKCanvas(maskedImage);
                canvas.Clear(SKColors.Transparent);
                canvas.DrawBitmap(resizedFrame, 0, 0);

                using var maskPaint = new SKPaint();
                maskPaint.BlendMode = SKBlendMode.DstIn;
                maskPaint.Shader = SKShader.CreateBitmap(mask, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

                canvas.DrawRect(new SKRect(0, 0, width, height), maskPaint);

                var outputFilePath = Path.Combine(outputFolder, Path.GetFileName(filePath));
                SaveBitmapToFile(maskedImage, outputFilePath);
                Log($"Masked frame saved: {outputFilePath}");
            }
        }

        // Saves a bitmap to a file
        private void SaveBitmapToFile(SKBitmap bitmap, string filePath)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(filePath, data.ToArray());
        }

        // Generates SPF and PAL files
        private void GenerateSpfAndPal(string imagesDirectory)
        {
            var spfOutputPath = Path.Combine(Environment.CurrentDirectory, "_nslogo.spf");
            var palOutputPaths = Enumerable.Range(1, 6)
                                           .Select(i => Path.Combine(Environment.CurrentDirectory, $"_nsl0{i}.pal"))
                                           .ToArray();

            var images = LoadImagesFromDirectory(imagesDirectory);
            var spf = SpfFile.FromImages(QuantizerOptions.Default, images);

            spf.Save(spfOutputPath);

            foreach (var palPath in palOutputPaths)
            {
                spf.PrimaryColors?.Save(palPath);
                Log($"PAL file saved: {palPath}");
            }

            Log($"SPF file saved: {spfOutputPath}");
        }

        // Loads images from a directory
        private List<SKImage?> LoadImagesFromDirectory(string directory)
        {
            return Directory.GetFiles(directory, "*.png")
                            .Select(filePath =>
                            {
                                try
                                {
                                    using var data = SKData.Create(filePath);
                                    return SKImage.FromEncodedData(data);
                                }
                                catch (Exception ex)
                                {
                                    Log($"Error loading image {filePath}: {ex.Message}");
                                    return null;
                                }
                            })
                            .Where(image => image != null)
                            .ToList();
        }
    }
}
