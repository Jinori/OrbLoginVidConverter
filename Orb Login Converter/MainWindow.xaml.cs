using System.IO;
using System.Windows;
using Microsoft.Win32;
using OpenCvSharp;
using SkiaSharp;
using DALib.Drawing;
using DALib.Utility;
using System; // For Action
using System.Collections.Generic; // Added for List
using System.Linq; // Added for Enumerable.Range and .Select

namespace Orb_Login_Converter
{
    /// <summary>
    /// Defines a simple interface for logging messages.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Log(string message);
    }

    /// <summary>
    /// An implementation of <see cref="ILogger"/> that writes messages
    /// to a target <see cref="Action{String}"/>, typically used to log to a UI element like a TextBox.
    /// </summary>
    public class TextBoxLogger : ILogger
    {
        private readonly Action<string> _logAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBoxLogger"/> class.
        /// </summary>
        /// <param name="logAction">The action to perform when a message is logged. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logAction"/> is null.</exception>
        public TextBoxLogger(Action<string> logAction)
        {
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
        }

        /// <summary>
        /// Logs a message by invoking the configured log action.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Log(string message)
        {
            _logAction(message);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Handles user interactions for video selection, processing, and application window controls.
    /// </summary>
    public partial class MainWindow
    {
        private const string VideoFileFilter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.flv|All Files|*.*";
        private const string SelectVideoTitle = "Select a Video File";

        private string _videoPath; // Stores the path to the selected video file.
        // private string _framesFolder; // Logic moved to VideoProcessor
        // private string _outputFolder; // Logic moved to VideoProcessor
        private readonly VideoProcessor _videoProcessor; // Handles the core video processing tasks.

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Sets up component initialization and the video processor instance.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _videoProcessor = new VideoProcessor(new TextBoxLogger(Log)); 
        }
        
        /// <summary>
        /// Handles the MouseDown event for the title bar to enable window dragging.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.</param>
        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                this.DragMove(); // Allows the window to be dragged.
            }
        }

        
        /// <summary>
        /// Handles the Click event for the Minimize button.
        /// Minimizes the application window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Handles the Click event for the Maximize button.
        /// Maximizes or restores the application window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggles between Maximized and Normal window states.
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        /// <summary>
        /// Handles the Click event for the Close button.
        /// Closes the application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        /// <summary>
        /// Handles the Click event for the "Load Video" button.
        /// Opens a file dialog for the user to select a video file.
        /// Updates the UI with the selected video path and logs the action.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void LoadVideo_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = VideoFileFilter,
                Title = SelectVideoTitle
            };

            if (openFileDialog.ShowDialog() != true) 
                return;
            
            _videoPath = openFileDialog.FileName;
            VideoPathTextBox.Text = _videoPath;
            Log($"Video loaded: {_videoPath}");
        }

        /// <summary>
        /// Handles the Click event for the "Process Video" button.
        /// Initiates the video processing workflow using the <see cref="VideoProcessor"/>.
        /// Clears the log, checks for a valid video path, and handles potential exceptions.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void ProcessVideo_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();

            if (string.IsNullOrWhiteSpace(_videoPath))
            {
                Log("Please load a video file first.");
                return;
            }

            // InitializeDirectories(); // Moved to VideoProcessor
            // Log("Processing started..."); // Moved to VideoProcessor

            try
            {
                // ProcessVideo(); // Old call
                _videoProcessor.ProcessVideo(_videoPath); // New call
                // Log("Processing completed!"); // Logging is now handled by VideoProcessor
            }
            catch (Exception ex) // General exception catch for the UI
            {
                Log($"An error occurred during video processing: {ex.Message}");
            }
        }

        // Initializes the directories for frames and output - MOVED to VideoProcessor
        // private void InitializeDirectories() { ... }

        // Main video processing logic - MOVED to VideoProcessor
        // private void ProcessVideo() { ... }

        // Loads the embedded logo - MOVED to VideoProcessor
        // private SKBitmap LoadLogo() { ... }

        /// <summary>
        /// Logs a message to the application's LogTextBox.
        /// Appends a new line after the message and scrolls to the end of the TextBox.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private void Log(string message)
        {
            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        }

        // Creates a directory and ensures it exists - MOVED to VideoProcessor
        // private string CreateDirectory(string name) { ... }

        // Extracts frames from the video - MOVED to VideoProcessor
        // private void ExtractFrames(string videoPath, string framesFolder) { ... }

        // Creates a circular mask - MOVED to VideoProcessor
        // private SKBitmap CreateCircularMask(int width, int height) { ... }

        // Applies the mask to the frames - MOVED to VideoProcessor
        // private void ApplyMaskToFrames(string framesFolder, string outputFolder, SKBitmap mask, int width, int height) { ... }

        // Saves a bitmap to a file - MOVED to VideoProcessor
        // private void SaveBitmapToFile(SKBitmap bitmap, string filePath) { ... }

        // Generates SPF and PAL files - MOVED to VideoProcessor
        // private void GenerateSpfAndPal(string imagesDirectory) { ... }

        // Loads images from a directory - MOVED to VideoProcessor
        // private List<SKImage?> LoadImagesFromDirectory(string directory) { ... }
    }

    /// <summary>
    /// Handles the core video processing logic, including frame extraction, image manipulation,
    /// and generation of specific file formats (SPF, PAL).
    /// </summary>
    public class VideoProcessor
    {
        // Constants for directory names, file paths, and processing parameters.
        private const string FramesDirectoryName = "frames";
        private const string OutputFramesDirectoryName = "output_frames";
        private const string LogoResourcePath = "pack://application:,,,/logo000.png";
        private const int FramesToExtractCount = 20;
        private const string SpfOutputFileName = "_nslogo.spf";
        private const string PalOutputBaseName = "_nsl0";
        private const string PalOutputFileExtension = ".pal";
        private const string PngFileExtension = ".png";
        private const string PngSearchPattern = "*.png"; // Derived from PngFileExtension
        private const int PngEncodingQuality = 100;

        private readonly ILogger _logger; // Use ILogger interface
        private string _framesFolder; // Path to the directory where extracted frames are stored.
        private string _outputFolder; // Path to the directory where processed frames (output) are stored.

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoProcessor"/> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> instance for logging messages. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if logger is null.</exception>
        public VideoProcessor(ILogger logger) // Changed to ILogger
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Orchestrates the entire video processing workflow.
        /// This includes initializing directories, extracting frames, loading resources,
        /// applying masks, and generating output files.
        /// </summary>
        /// <param name="videoPath">The file path of the video to process.</param>
        public void ProcessVideo(string videoPath)
        {
            if (string.IsNullOrWhiteSpace(videoPath))
            {
                _logger.Log("Video path cannot be empty in VideoProcessor."); // Use _logger.Log
                return;
            }

            // Step 1: Setup directories for temporary frames and final output.
            this.InitializeDirectories(); 
            _logger.Log("Processing started in VideoProcessor..."); // Use _logger.Log

            try
            {
                // Step 2: Extract frames from the input video.
                this.ExtractFrames(videoPath, this._framesFolder);

                // Step 3: Load the logo image that will be used as part of the mask/overlay.
                using var logo = this.LoadLogo(); 
                if (logo == null)
                {
                    _logger.Log("Logo loading failed, aborting video processing in VideoProcessor."); // Use _logger.Log
                    return; // Critical resource missing.
                }

                // Validate logo dimensions before proceeding.
                if (logo.Width <= 0 || logo.Height <= 0)
                {
                    _logger.Log($"Invalid logo dimensions ({logo.Width}x{logo.Height}). Aborting video processing."); // Use _logger.Log
                    return;
                }

                // Step 4: Create a circular mask based on the logo's dimensions.
                var mask = this.CreateCircularMask(logo.Width, logo.Height);
                if (mask == null)
                {
                    _logger.Log("Mask creation failed, aborting video processing in VideoProcessor."); // Use _logger.Log
                    return; // Mask is essential for the next step.
                }

                // Step 5: Apply the created mask to each extracted frame.
                // The frames are resized to the logo's dimensions before masking.
                this.ApplyMaskToFrames(this._framesFolder, this._outputFolder, mask, logo.Width, logo.Height);
                
                // Step 6: Generate SPF and PAL files from the masked frames.
                this.GenerateSpfAndPal(this._outputFolder);
                _logger.Log("Processing completed successfully in VideoProcessor!"); // Use _logger.Log
            }
            catch (Exception ex)
            {
                // Catch-all for unexpected errors during the process.
                _logger.Log($"Error in VideoProcessor.ProcessVideo: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}"); // Use _logger.Log
            }
        }

        /// <summary>
        /// Initializes the directories required for storing intermediate frames and final output frames.
        /// It creates 'frames' and 'output_frames' directories in the current execution path.
        /// </summary>
        private void InitializeDirectories()
        {
            this._framesFolder = this.CreateDirectory(FramesDirectoryName);
            this._outputFolder = this.CreateDirectory(OutputFramesDirectoryName);
            _logger.Log($"Frames folder set to: {this._framesFolder}"); // Use _logger.Log
            _logger.Log($"Output folder set to: {this._outputFolder}"); // Use _logger.Log
        }

        /// <summary>
        /// Creates a directory at the specified path if it does not already exist.
        /// </summary>
        /// <param name="name">The name of the directory to create, relative to the current execution directory.</param>
        /// <returns>The full path of the created or existing directory.</returns>
        /// <exception cref="Exception">Throws an exception if directory creation fails, which is then caught by the caller.</exception>
        private string CreateDirectory(string name)
        {
            var path = Path.Combine(Environment.CurrentDirectory, name);
            try
            {
                Directory.CreateDirectory(path);
                _logger.Log($"Directory created/ensured: {path}"); // Use _logger.Log
            }
            catch (Exception ex)
            {
                _logger.Log($"Error creating directory {path}: {ex.Message}"); // Use _logger.Log
                // Wrap and re-throw as a custom exception for more specific error handling.
                throw new FileOperationException($"Failed to create directory {path}: {ex.Message}", ex); 
            }
            return path;
        }
        
        /// <summary>
        /// Loads the embedded logo image (logo000.png) from application resources.
        /// </summary>
        /// <returns>An <see cref="SKBitmap"/> object of the loaded logo, or null if loading fails.</returns>
        /// <remarks>Throws FileNotFoundException if the resource URI is invalid or the resource is not found, though current implementation returns null after logging.</remarks>
        private SKBitmap LoadLogo()
        {
            _logger.Log($"Attempting to load logo resource ({Path.GetFileName(LogoResourcePath)})."); // Use _logger.Log
            var resourceUri = new Uri(LogoResourcePath, UriKind.Absolute);
            var streamInfo = Application.GetResourceStream(resourceUri); // WPF specific call to get embedded resource.

            if (streamInfo == null)
            {
                // Log the error. For robustness in this specific workflow, we return null.
                // If this were a library, throwing new FileNotFoundException($"Embedded logo file not found at URI: {LogoResourcePath}") would be more conventional.
                _logger.Log($"CRITICAL: Embedded logo file '{Path.GetFileName(LogoResourcePath)}' not found at {LogoResourcePath}. Ensure the image Build Action is 'Resource' and the path is correct."); // Use _logger.Log
                return null; 
            }

            using var stream = streamInfo.Stream;
            var logo = SKBitmap.Decode(stream);
            if (logo == null)
            {
                _logger.Log("CRITICAL: Failed to decode logo image. Ensure it's a valid PNG/image format and not corrupted."); // Use _logger.Log
                return null;
            }
            _logger.Log($"Logo loaded successfully. Dimensions: {logo.Width}x{logo.Height}"); // Use _logger.Log
            return logo;
        }

        /// <summary>
        /// Extracts a specified number of frames (<see cref="FramesToExtractCount"/>) from the input video
        /// at approximately equal intervals and saves them as PNG files in the specified folder.
        /// </summary>
        /// <param name="videoPath">The file path of the video from which to extract frames.</param>
        /// <param name="framesFolderPath">The path to the directory where extracted frames will be saved.</param>
        /// <exception cref="Exception">Throws an exception if the video file cannot be opened.</exception>
        private void ExtractFrames(string videoPath, string framesFolderPath)
        {
            _logger.Log($"Starting frame extraction for video: {videoPath} into {framesFolderPath}"); // Use _logger.Log
            using var capture = new VideoCapture(videoPath);

            if (!capture.IsOpened())
            {
                // Log the error before throwing a specific exception.
                _logger.Log($"FrameExtractionError: Unable to open video file: {videoPath}"); // Use _logger.Log
                throw new FrameExtractionException($"Unable to open video file: {videoPath}");
            }

            int totalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount);
            _logger.Log($"Video contains {totalFrames} total frames."); // Use _logger.Log

            if (totalFrames <= 0)
            {
                _logger.Log("Video file contains no frames or frame count could not be retrieved. Stopping frame extraction."); // Use _logger.Log
                return; 
            }

            // Process a maximum of FramesToExtractCount frames.
            int framesToExtract = Math.Min(FramesToExtractCount, totalFrames);
            // Calculate interval to pick frames spread across the video.
            // This ensures that frames are sampled throughout the video's duration.
            // Math.Max(1, ...) ensures the interval is at least 1 to prevent issues with very short videos or few frames.
            int interval = (totalFrames > framesToExtract) ? Math.Max(1, totalFrames / framesToExtract) : 1;
            
            _logger.Log($"Attempting to extract {framesToExtract} frames (max: {FramesToExtractCount}) with an interval of {interval}."); // Use _logger.Log

            for (int i = 0; i < framesToExtract; i++)
            {
                long framePosition = (long)i * interval;
                 // Ensure framePosition is within the valid range of the video's frames.
                if (framePosition >= totalFrames && totalFrames > 0) { 
                    framePosition = totalFrames - 1; // Adjust to get the very last frame if calculated position is out of bounds.
                }
                if (framePosition < 0) framePosition = 0; // Safety check, though interval logic should prevent this.

                capture.Set(VideoCaptureProperties.PosFrames, framePosition); // Seek to the calculated frame position.
                using var frameMat = new Mat(); // OpenCV Mat object to hold the frame data.

                if (capture.Read(frameMat) && !frameMat.Empty()) // Read the frame and ensure it's not empty.
                {
                    var frameFilename = Path.Combine(framesFolderPath, $"frame_{i + 1:02d}{PngFileExtension}");
                    try
                    {
                        frameMat.SaveImage(frameFilename);
                        _logger.Log($"Frame saved: {frameFilename} (from position {framePosition})"); // Use _logger.Log
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Failed to save frame {frameFilename}: {ex.Message}"); // Use _logger.Log
                    }
                }
                else
                {
                    _logger.Log($"Failed to read frame at position {framePosition} or frame was empty. Total frames: {totalFrames}."); // Use _logger.Log
                    // If reading fails near the end, break to avoid multiple similar errors.
                    if (totalFrames > 0 && framePosition >= totalFrames - interval && i < framesToExtract -1) 
                    {
                        _logger.Log("Stopping frame extraction due to read failure that might indicate end of video or issue."); // Use _logger.Log
                        break; 
                    }
                }
            }
            _logger.Log("Frame extraction finished."); // Use _logger.Log
        }

        /// <summary>
        /// Creates a circular SKBitmap mask with the specified dimensions.
        /// The mask is white within the circle and transparent outside.
        /// </summary>
        /// <param name="width">The width of the mask to create.</param>
        /// <param name="height">The height of the mask to create.</param>
        /// <returns>An <see cref="SKBitmap"/> representing the circular mask, or null if dimensions are invalid.</returns>
        private SKBitmap CreateCircularMask(int width, int height)
        {
            if (width <= 0 || height <= 0) 
            {
                _logger.Log($"Invalid dimensions ({width}x{height}) for circular mask. Cannot create mask."); // Use _logger.Log
                return null;
            }
            // Create a new bitmap for the mask with Rgba8888 color type and premultiplied alpha for transparency.
            var mask = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            using var canvas = new SKCanvas(mask);
            canvas.Clear(SKColors.Transparent); // Start with a fully transparent canvas.

            // Define the paint for drawing the white oval (the circular part of the mask).
            var paint = new SKPaint
            {
                Color = SKColors.White, // The opaque part of the mask will be white.
                IsAntialias = true,     // Smooth edges for the circle.
                Style = SKPaintStyle.Fill // Fill the oval.
            };

            // Draw the oval onto the canvas. This defines the shape of the mask.
            canvas.DrawOval(new SKRect(0, 0, width, height), paint);
            _logger.Log($"Circular mask created with dimensions {width}x{height}."); // Use _logger.Log
            return mask;
        }

        /// <summary>
        /// Applies a given mask to all PNG frames in a specified directory.
        /// Frames are first resized to the target dimensions (mask dimensions), then the mask is applied.
        /// The resulting masked frames are saved to an output directory.
        /// </summary>
        /// <param name="framesFolderPath">Path to the directory containing the input PNG frames.</param>
        /// <param name="outputFolderPath">Path to the directory where masked frames will be saved.</param>
        /// <param name="mask">The <see cref="SKBitmap"/> mask to apply.</param>
        /// <param name="width">The target width for resizing frames and applying the mask.</param>
        /// <param name="height">The target height for resizing frames and applying the mask.</param>
        private void ApplyMaskToFrames(string framesFolderPath, string outputFolderPath, SKBitmap mask, int width, int height)
        {
            if (mask == null)
            {
                _logger.Log("Mask is null in ApplyMaskToFrames. Aborting masking process."); // Use _logger.Log
                return;
            }
            if (width <= 0 || height <= 0) 
            {
                 _logger.Log($"Invalid target dimensions ({width}x{height}) for ApplyMaskToFrames. Aborting."); // Use _logger.Log
                return;
            }

            _logger.Log($"Starting to apply mask to frames in {framesFolderPath}. Output to {outputFolderPath}. Target size: {width}x{height}."); // Use _logger.Log
            var files = Directory.GetFiles(framesFolderPath, PngSearchPattern);
            if (!files.Any())
            {
                _logger.Log($"No {PngFileExtension} files found in {framesFolderPath} to apply mask."); // Use _logger.Log
                return;
            }

            foreach (var filePath in files)
            {
                _logger.Log($"Processing file for masking: {filePath}"); // Use _logger.Log
                using var original = SKBitmap.Decode(filePath);
                if (original == null)
                {
                    _logger.Log($"Skipping null image at {filePath} (could not decode)."); // Use _logger.Log
                    continue;
                }
                
                // Resize the original frame to the target dimensions (typically logo dimensions).
                // SKColorType.Rgba8888 and SKAlphaType.Premul are important for handling transparency correctly.
                using var resizedFrame = original.Resize(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul), SKFilterQuality.High);
                // Create a new bitmap to hold the result of the masking operation.
                using var maskedImage = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

                using var canvas = new SKCanvas(maskedImage);
                canvas.Clear(SKColors.Transparent); // Ensure the canvas for the masked image starts transparent.
                canvas.DrawBitmap(resizedFrame, 0, 0); // Draw the resized frame onto this canvas.

                // Prepare paint for applying the mask.
                using var maskPaint = new SKPaint();
                // SKBlendMode.DstIn keeps the destination pixels (the frame) only where the source pixels (the mask) are opaque.
                // This effectively "cuts out" the frame according to the mask's shape.
                maskPaint.BlendMode = SKBlendMode.DstIn; 
                // The mask itself is used as a shader.
                maskPaint.Shader = SKShader.CreateBitmap(mask, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
                
                // Draw a rectangle covering the entire image area, applying the mask paint.
                // This performs the actual masking operation.
                canvas.DrawRect(new SKRect(0, 0, width, height), maskPaint); 

                var outputFilePath = Path.Combine(outputFolderPath, Path.GetFileName(filePath));
                this.SaveBitmapToFile(maskedImage, outputFilePath); 
            }
            _logger.Log("Mask application process finished."); // Use _logger.Log
        }

        /// <summary>
        /// Saves an <see cref="SKBitmap"/> to a file at the specified path as a PNG image.
        /// </summary>
        /// <param name="bitmap">The bitmap to save.</param>
        /// <param name="filePath">The path where the image file will be saved.</param>
        private void SaveBitmapToFile(SKBitmap bitmap, string filePath)
        {
            try
            {
                using var image = SKImage.FromBitmap(bitmap);
                if (image == null)
                {
                    _logger.Log($"Failed to create SKImage from SKBitmap for: {filePath}"); // Use _logger.Log
                    return;
                }
                using var data = image.Encode(SKEncodedImageFormat.Png, PngEncodingQuality);
                if (data == null)
                {
                    _logger.Log($"Failed to encode image to PNG with quality {PngEncodingQuality}: {filePath}"); // Use _logger.Log
                    return;
                }
                File.WriteAllBytes(filePath, data.ToArray());
                _logger.Log($"Successfully saved image: {filePath}"); // Use _logger.Log
            }
            catch (Exception ex)
            {
                _logger.Log($"Error saving bitmap to file {filePath}: {ex.Message}"); // Use _logger.Log
            }
        }
        
        /// <summary>
        /// Loads all PNG images from a specified directory into a list of <see cref="SKImage"/> objects.
        /// </summary>
        /// <param name="directory">The path to the directory from which to load images.</param>
        /// <returns>A list of nullable <see cref="SKImage"/> objects. Images that fail to load will be null.</returns>
        private List<SKImage?> LoadImagesFromDirectory(string directory)
        {
            _logger.Log($"Loading images from directory: {directory}"); // Use _logger.Log
            var imageFiles = Directory.GetFiles(directory, PngSearchPattern); // Find all files matching the PNG pattern.
            if (!imageFiles.Any())
            {
                _logger.Log($"No {PngFileExtension} files found in directory: {directory}"); // Use _logger.Log
                return new List<SKImage?>();
            }
            var loadedImages = new List<SKImage?>();
            foreach (var filePath in imageFiles)
            {
                try
                {
                    using var data = SKData.Create(filePath);
                    var image = SKImage.FromEncodedData(data);
                    if (image == null)
                    {
                        _logger.Log($"Failed to decode image {filePath} using SKImage.FromEncodedData. It might be corrupted or an unsupported PNG variant. Skipping this image."); // Use _logger.Log
                    }
                    loadedImages.Add(image);
                }
                catch (Exception ex)
                {
                    // Log detailed error and indicate that this specific image will be skipped.
                    _logger.Log($"Error loading image {filePath}: {ex.Message}. Skipping this image."); // Use _logger.Log
                    loadedImages.Add(null); 
                }
            }
            _logger.Log($"Finished loading images from {directory}. Loaded {loadedImages.Count(img => img != null)} images successfully out of {imageFiles.Length} files."); // Use _logger.Log
            return loadedImages;
        }

        /// <summary>
        /// Generates SPF (Sprite Packed File) and PAL (Palette) files from images in a given directory.
        /// The SPF file is a custom format containing multiple images, and PAL files store color palettes.
        /// </summary>
        /// <param name="imagesDirectoryPath">The path to the directory containing the images to process (typically masked frames).</param>
        private void GenerateSpfAndPal(string imagesDirectoryPath)
        {
            _logger.Log($"Starting SPF and PAL generation from images in {imagesDirectoryPath}."); // Use _logger.Log
            var spfOutputPath = Path.Combine(Environment.CurrentDirectory, SpfOutputFileName);
            var palOutputPaths = Enumerable.Range(1, 6) // Standard PAL count, could be a constant if it varies
                                           .Select(i => Path.Combine(Environment.CurrentDirectory, $"{PalOutputBaseName}{i}{PalOutputFileExtension}"))
                                           .ToArray();

            // Load all images (frames) from the specified directory.
            var images = this.LoadImagesFromDirectory(imagesDirectoryPath); 
            if (images == null || !images.Any()) 
            {
                _logger.Log($"No images loaded from {imagesDirectoryPath}, or LoadImagesFromDirectory returned null. Cannot generate SPF/PAL files."); // Use _logger.Log
                return;
            }
            
            // Filter out any null images (if loading failed for some) and cast to SKImage.
            var validImages = images.Where(img => img != null).Cast<SKImage>().ToList();
            if (!validImages.Any())
            {
                 _logger.Log($"No valid (non-null) images found after loading from {imagesDirectoryPath}. Cannot generate SPF."); // Use _logger.Log
                return;
            }
            _logger.Log($"Loaded {validImages.Count} valid images for SPF generation."); // Use _logger.Log

            try
            {
                // Create the SPF file from the list of valid images using default quantization options.
                var spf = SpfFile.FromImages(QuantizerOptions.Default, validImages);
                spf.Save(spfOutputPath); // Save the generated SPF file.
                _logger.Log($"SPF file saved: {spfOutputPath}"); // Use _logger.Log

                // If the SPF generation resulted in primary colors (a palette), save them as PAL files.
                if (spf.PrimaryColors != null)
                {
                    foreach (var palPath in palOutputPaths)
                    {
                        spf.PrimaryColors.Save(palPath); // Save each PAL file.
                        _logger.Log($"PAL file saved: {palPath}"); // Use _logger.Log
                    }
                }
                else
                {
                    _logger.Log("No primary colors found in SPF to save PAL files."); // Use _logger.Log
                }
            }
            catch(Exception ex)
            {
                _logger.Log($"Error during SPF/PAL generation: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}"); // Use _logger.Log
            }
            _logger.Log("SPF and PAL generation finished."); // Use _logger.Log
        }
    }

    // Custom Exception Classes
    
    /// <summary>
    /// Base class for custom exceptions related to video processing operations.
    /// </summary>
    public class VideoProcessingException : Exception
    {
        public VideoProcessingException(string message) : base(message) { }
        public VideoProcessingException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents errors that occur specifically during the frame extraction phase of video processing.
    /// </summary>
    public class FrameExtractionException : VideoProcessingException
    {
        public FrameExtractionException(string message) : base(message) { }
        public FrameExtractionException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents errors that occur during file or directory operations, such as creating directories.
    /// </summary>
    public class FileOperationException : VideoProcessingException
    {
        public FileOperationException(string message) : base(message) { }
        public FileOperationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
