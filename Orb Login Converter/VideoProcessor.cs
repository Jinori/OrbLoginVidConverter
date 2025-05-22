using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows; // For Application.GetResourceStream if needed by LoadLogo
using System.Windows.Media.Imaging; // For BitmapImage if needed by LoadLogo
using OpenCvSharp;
using OpenCvSharp.WpfExtensions; // For BitmapSourceConverter
using SkiaSharp;
using DALib.Drawing;
using DALib.Utility;

namespace Orb_Login_Converter
{
    public class VideoProcessor
    {
        private readonly Action<string> _logger;
        private string _framesFolder;
        private string _outputFolder;

        public VideoProcessor(Action<string> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Methods to be moved here
    }
}
