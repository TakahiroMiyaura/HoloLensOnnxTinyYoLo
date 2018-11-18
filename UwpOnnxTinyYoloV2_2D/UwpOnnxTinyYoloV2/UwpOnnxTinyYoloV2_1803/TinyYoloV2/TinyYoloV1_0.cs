// Copyright(c) 2018 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.AI.MachineLearning.Preview;
using Windows.Media;
using Windows.Storage;

// TinyYoloV1_0

namespace UwpOnnxTinyYoloV2_1803.TinyYoloV2
{
    public sealed class TinyYoloV1_0ModelInput
    {
        public VideoFrame image { get; set; }
    }

    public sealed class TinyYoloV1_0ModelOutput
    {
        public IList<float> grid { get; set; }

        public TinyYoloV1_0ModelOutput()
        {
            grid = new List<float>(new float[125 * 13 * 13]);
        }
    }

    public sealed class TinyYoloV1_0Model
    {
        private LearningModelPreview learningModel;

        public static async Task<TinyYoloV1_0Model> CreateTinyYoloV1_0Model(StorageFile file)
        {
            var learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            var model = new TinyYoloV1_0Model();
            model.learningModel = learningModel;
            return model;
        }

        public async Task<TinyYoloV1_0ModelOutput> EvaluateAsync(TinyYoloV1_0ModelInput input)
        {
            var output = new TinyYoloV1_0ModelOutput();
            var binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("image", input.image);
            binding.Bind("grid", output.grid);
            var evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}