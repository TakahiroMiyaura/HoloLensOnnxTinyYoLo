// Copyright(c) 2018 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace UwpOnnxTinyYoloV2_1809.TinyYoloV2
{
    /// <summary>
    ///     File I/O Utility class
    /// </summary>
    internal static class FileUtil
    {
        /// <summary>
        ///     Write an image file by png format.
        /// </summary>
        /// <param name="softwareBitmap">
        ///     <see cref="SoftwareBitmap" />
        /// </param>
        /// <returns></returns>
        public static async Task WriteImageFileAsync(SoftwareBitmap softwareBitmap)
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.SuggestedFileName = "image";
            picker.FileTypeChoices.Add("Image File", new List<string> {".png"});

            var file = await picker.PickSaveFileAsync();
            if (file == null) return;
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encorder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8);
                encorder.SetSoftwareBitmap(softwareBitmap);

                await encorder.FlushAsync();
            }
        }

        /// <summary>
        ///     Load an image file by png format.
        /// </summary>
        /// <returns>
        ///     <see cref="SoftwareBitmap" />
        /// </returns>
        public static async Task<SoftwareBitmap> LoadImageFileAsync()
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.ViewMode = PickerViewMode.Thumbnail;

            var input = await picker.PickSingleFileAsync();

            if (input == null) return null;

            SoftwareBitmap softwareBitmap;
            ulong dataSize = 0;
            using (var stream = await input.OpenAsync(FileAccessMode.Read))
            {
                var decorder = await BitmapDecoder.CreateAsync(stream);
                softwareBitmap =
                    await decorder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            }

            return softwareBitmap;
        }
    }
}