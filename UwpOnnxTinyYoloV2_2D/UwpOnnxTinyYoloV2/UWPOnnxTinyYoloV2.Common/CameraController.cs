// Copyright(c) 2018 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace UWPOnnxTinyYoloV2.Common
{
    /// <summary>
    /// Provides access to a camera device for client universal windows platform..
    /// </summary>
    public class CameraController
    {
        /// <summary>
        /// delgate to output processing messages.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="append">true-append,false-override</param>
        /// <returns></returns>
        public delegate Task ProcessNotice(string message, bool append = true);

        /// <summary>
        /// Define an action output processing messages.
        ///  </summary>
        public ProcessNotice OnProcessNotice;

        private static LowLagPhotoCapture _lowLagPhotoCapture;
        private static MediaCapture _capture;

        /// <summary>
        /// Initializes a new instance of the MediaCapture class setting a camera device.
        /// </summary>
        public async Task InitializeCameraAsync()
        {
            try
            {
                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                var device = devices[0];
                _capture = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = device.Id
                };
                await _capture.InitializeAsync(settings);

                _lowLagPhotoCapture = await _capture.PrepareLowLagPhotoCaptureAsync(
                    ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));
                _capture.VideoDeviceController.Focus.TrySetAuto(true);

            }
            catch (Exception e)
            {
                await OnProcessNotice(e.ToString());
            }

        }

        /// <summary>
        /// Capture a photo.
        /// </summary>
        /// <param name="bitmapPixelFormat"><see cref="BitmapPixelFormat"/></param>
        /// <returns>Captured image</returns>
        public async Task<SoftwareBitmap> CapturePhotoAsync(BitmapPixelFormat bitmapPixelFormat)
        {

            SoftwareBitmap convert = null;
            try
            {
                CapturedPhoto asyncOperation = null;
                try
                {
                    asyncOperation = await _lowLagPhotoCapture.CaptureAsync();
                }
                catch (Exception exception)
                {
                    await OnProcessNotice(exception.ToString());
                }

                using (var asyncOperationFrame = asyncOperation.Frame)
                {
                    convert = SoftwareBitmap.Convert(asyncOperationFrame.SoftwareBitmap, bitmapPixelFormat);
                }
            }
            catch (Exception e)
            {
                await OnProcessNotice(e.ToString());
            }

            return convert;
        }
    }
}