// Copyright(c) 2018 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using UwpAppYolo01.Yolo9000;

namespace UwpOnnxTinyYoloV2_1809.TinyYoloV2
{
    /// <summary>
    /// Presentation a class evaluate using ONNX(Tiny Yolo V2) on Windows ML.
    /// thisn class can execute in Windows 10(1809).
    /// </summary>
    public class TinyYoloEvaluatorFor1809
    {
        /// <summary>
        /// Declares a delegate for a method that output message for processing status.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="append">true - message append false - message override</param>
        public delegate Task LogActionAsync(string message, bool append = true);

        /// <summary>
        /// Declares a delegate for a method that display evaluate result data.
        /// </summary>
        /// <param name="boxes">list of <see cref="YoloBoundingBox"/></param>
        public delegate Task VisualizeUIAsync(IList<YoloBoundingBox> boxes);

        /// <summary>
        /// Represents the input image width.
        /// </summary>
        public static readonly uint EvaluateWidth = 416;

        /// <summary>
        /// Represents the input image height.
        /// </summary>
        public static readonly uint EvaluateHeight = 416;

        /// <summary>
        ///  Register an action that output message for processing status.
        /// </summary>
        public LogActionAsync OnLogActionAsync;

        /// <summary>
        /// Register an action that display evaluate result data.
        /// </summary>
        public VisualizeUIAsync OnVisualizeUIAsync;

        /// <summary>
        /// Represents Tiny Yolo Parser.
        /// </summary>
        protected static readonly YoloWinMlParser _parser = new YoloWinMlParser();

        /// <summary>
        /// Set or Get result parameters of Tiny Yolo V2.
        /// </summary>
        protected static IList<YoloBoundingBox> _boxes = new List<YoloBoundingBox>();

        /// <summary>
        /// Represents ONNX Model object.
        /// </summary>
        private static TinyYoloV1_2Model _model;

        /// <summary>
        /// Load Onnx Model.
        /// </summary>
        /// <returns></returns>
        public virtual async Task InitializeModelAsync()
        {
            var modelFile =
                await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appx:///Assets/TinyYoloV1_2.onnx"));
            _model = await TinyYoloV1_2Model.CreateFromStreamAsync(modelFile);
        }

        /// <summary>
        ///  Evaluate an image data by using ONNX.
        /// </summary>
        /// <param name="softwareBitmap">input image data.</param>
        /// <returns></returns>
        public virtual async Task EvaluatedByONNXAsync(SoftwareBitmap softwareBitmap)
        {
            var input = new TinyYoloV1_2Input();
            input.image =
                VideoFrame.CreateWithSoftwareBitmap(SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8));

            await OnLogActionAsync("Start - Evaluate\n");
            var tinyYoloV10ModelOutput = await _model.EvaluateAsync(input);

            await OnLogActionAsync("Finish - Evalutate\n");
            _boxes = _parser.ParseOutputs(tinyYoloV10ModelOutput.grid.GetAsVectorView().ToArray());

            await OnLogActionAsync("Finish - Parse\n");


            await OnVisualizeUIAsync(_boxes);
        }

        /// <summary>
        /// Normalize Image data for evaluate "Tiny Yolo V2" onnx model
        /// </summary>
        /// <remarks>
        /// Resize to 416 x 416.
        /// </remarks>
        /// <param name="convert">Image data</param>
        /// <returns>Normalized Image data.</returns>
        public virtual async Task<SoftwareBitmap> ImageNormalizedAsync(SoftwareBitmap convert)
        {
            SoftwareBitmap softwareBitmap;

            using (var stream = new InMemoryRandomAccessStream())
            {
                var encorder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                encorder.SetSoftwareBitmap(convert);
                encorder.BitmapTransform.ScaledWidth = EvaluateWidth;
                encorder.BitmapTransform.ScaledHeight = EvaluateHeight;

                await encorder.FlushAsync();
                stream.Seek(0);
                var decorder = await BitmapDecoder.CreateAsync(stream);

                softwareBitmap =
                    await decorder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            }

            return softwareBitmap;
        }
    }
}