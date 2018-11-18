// Copyright(c) 2018 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using UwpAppYolo01.Yolo9000;
using UwpOnnxTinyYoloV2_1803.TinyYoloV2;
using UwpOnnxTinyYoloV2_1809.TinyYoloV2;

namespace UwpOnnxTinyYoloV2_1803
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        ///     Represents the Tiny Yolo Evaluator.
        /// </summary>
        private readonly TinyYoloEvaluatorFor1803 _tinyYoloEvaluator = new TinyYoloEvaluatorFor1803();

        /// <summary>
        ///     Represents the camera capture.
        /// </summary>
        private readonly CameraController _cameraController = new CameraController();
        private bool _isReady;

        /// <summary>
        ///     Represents the capture image width.
        /// </summary>
        public uint _yoloCanvasActualWidth;

        /// <summary>
        ///     Represents the capture image height.
        /// </summary>
        public uint _yoloCanvasActualHeight;

        /// <summary>
        ///     Represents the brush for markers of detected object.(other)
        /// </summary>
        private readonly SolidColorBrush _lineBrushYellow = new SolidColorBrush(Colors.Yellow);

        /// <summary>
        ///     Represents the brush for markers of detected object.(person)
        /// </summary>
        private readonly SolidColorBrush _lineBrushGreen = new SolidColorBrush(Colors.Green);

        /// <summary>
        ///     Represents the brush for markers of detected object.(person)
        /// </summary>private readonly SolidColorBrush _lineBrushGreen = new SolidColorBrush(Colors.Green);
        private readonly SolidColorBrush _fillBrush = new SolidColorBrush(Colors.Transparent);

        /// <summary>
        ///     Represents the line tickness for markers of detected object.
        /// </summary>
        private readonly double _lineThickness = 2.0;

        /// <summary>
        ///     Initialize Service and add service.
        /// </summary>
        public async Task InitizlizeServiceAsync()
        {
            _tinyYoloEvaluator.OnLogActionAsync = SetResult;
            _tinyYoloEvaluator.OnVisualizeUAsync = VisualizeUI;

            _cameraController.OnProcessNotice = SetResult;


            await _tinyYoloEvaluator.InitializeModel();

            await _cameraController.InitializeCameraAsync();

            _isReady = true;
            SetResult(_isReady.ToString());
        }

        /// <summary>
        ///     Capture the photo,and evaluate it.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">EventArgs</param>
        private async void Capture_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isReady) return;

                await SetResult("", false);
                SoftwareBitmap convert = null;

                convert = await _cameraController.CapturePhotoAsync(BitmapPixelFormat.Bgra8);
                await SetResult("Photo Captured\n");

                await DisplayImageData(convert);

                var softwareBitmap = await _tinyYoloEvaluator.ImageNormalized(convert);

                await SetResult("Image Normalized\n");

                await _tinyYoloEvaluator.EvaluatedByONNX(softwareBitmap);

                //await WriteImageFileAsync(softwareBitmap);
            }
            catch (Exception exception)
            {
                await SetResult(exception.ToString());
            }
        }

        /// <summary>
        ///     Load the image file,and evaluate it.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">EventArgs</param>
        private async void LoadFile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isReady) return;
                await SetResult("", false);
                SoftwareBitmap softwareBitmap = null;

                await SetResult("Start\n");

                softwareBitmap = await FileUtil.LoadImageFileAsync();
                await SetResult("Image Load\n");


                await DisplayImageData(softwareBitmap);

                softwareBitmap = await _tinyYoloEvaluator.ImageNormalized(softwareBitmap);

                await SetResult("Image Normalized\n");

                await _tinyYoloEvaluator.EvaluatedByONNX(softwareBitmap);
            }
            catch (Exception exception)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { Ready.Text += exception; });
            }
        }

        /// <summary>
        ///     Set Canvas for displying markers.
        /// </summary>
        /// <param name="bitmap"><see cref="SoftwareBitmap"/></param>
        /// <returns></returns>
        private async Task DisplayImageData(SoftwareBitmap bitmap)
        {
            var sbSource = new SoftwareBitmapSource();
            await sbSource.SetBitmapAsync(SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Ignore));
            var image = new Image();
            ImageData.Source = sbSource;
            Canvas1.Width = bitmap.PixelWidth;
            Canvas1.Height = bitmap.PixelHeight;
            Canvas1.Children.Add(image);
            _yoloCanvasActualWidth = (uint) bitmap.PixelWidth;
            _yoloCanvasActualHeight = (uint) bitmap.PixelHeight;
        }

        /// <summary>
        ///     Display markers of detected object.
        /// </summary>
        /// <param name="box"><see cref="YoloBoundingBox"/></param>
        /// <param name="overlayCanvas"><see cref="Canvas"/></param>
        public void DrawYoloBoundingBox(YoloBoundingBox box, Canvas overlayCanvas)
        {
            // process output boxes
            var x = (uint) Math.Max(box.X, 0);
            var y = (uint) Math.Max(box.Y, 0);
            var w = (uint) Math.Min(overlayCanvas.ActualWidth - x, box.Width);
            var h = (uint) Math.Min(overlayCanvas.ActualHeight - y, box.Height);

            // fit to current canvas and webcam size
            x = _yoloCanvasActualWidth * x / TinyYoloEvaluatorFor1803.EvaluateWidth;
            y = _yoloCanvasActualHeight * y / TinyYoloEvaluatorFor1803.EvaluateHeight;
            w = _yoloCanvasActualWidth * w / TinyYoloEvaluatorFor1803.EvaluateWidth;
            h = _yoloCanvasActualHeight * h / TinyYoloEvaluatorFor1803.EvaluateHeight;

            var rectStroke = box.Label == "person" ? _lineBrushGreen : _lineBrushYellow;

            var r = new Rectangle
            {
                Tag = box,
                Width = w,
                Height = h,
                Fill = _fillBrush,
                Stroke = rectStroke,
                StrokeThickness = _lineThickness,
                Margin = new Thickness(x, y, 0, 0)
            };

            var tb = new TextBlock
            {
                Margin = new Thickness(x + 4, y + 4, 0, 0),
                Text = $"{box.Label} ({Math.Round(box.Confidence, 4)})",
                FontWeight = FontWeights.Bold,
                Width = 126,
                Height = 21,
                HorizontalTextAlignment = TextAlignment.Center
            };

            var textBack = new Rectangle
            {
                Width = 134,
                Height = 29,
                Fill = rectStroke,
                Margin = new Thickness(x, y, 0, 0)
            };

            overlayCanvas.Children.Add(textBack);
            overlayCanvas.Children.Add(tb);
            overlayCanvas.Children.Add(r);
        }

        /// <summary>
        ///     Display evaluate result data.
        /// </summary>
        private async Task VisualizeUI(IList<YoloBoundingBox> _boxes)
        {
            Canvas1.Children.Clear();
            var sb = new StringBuilder();
            foreach (var yoloBoundingBox in _boxes)
            {
                DrawYoloBoundingBox(yoloBoundingBox, Canvas1);

                sb.AppendLine(yoloBoundingBox.ToString());
            }

            ;
            await SetResult(sb.ToString(), false);
        }

        /// <summary>
        ///     Output message for processing status.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="append">true - message append false - message override</param>
        private async Task SetResult(string message, bool append = true)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    if (append)
                        Ready.Text += message;
                    else
                        Ready.Text = message;
                });
        }
    }
}