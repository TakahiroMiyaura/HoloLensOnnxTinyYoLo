using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 8d4d0fa662b14686b1865e0e6d3c598e

namespace UwpOnnxTinyYoloV2_1803
{
    public sealed class 8d4d0fa662b14686b1865e0e6d3c598eModelInput
    {
        public VideoFrame image { get; set; }
    }

    public sealed class 8d4d0fa662b14686b1865e0e6d3c598eModelOutput
    {
        public IList<float> grid { get; set; }
        public 8d4d0fa662b14686b1865e0e6d3c598eModelOutput()
        {
            this.grid = new List<float>();
        }
    }

    public sealed class 8d4d0fa662b14686b1865e0e6d3c598eModel
    {
        private LearningModelPreview learningModel;
        public static async Task<8d4d0fa662b14686b1865e0e6d3c598eModel> Create8d4d0fa662b14686b1865e0e6d3c598eModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            8d4d0fa662b14686b1865e0e6d3c598eModel model = new 8d4d0fa662b14686b1865e0e6d3c598eModel();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<8d4d0fa662b14686b1865e0e6d3c598eModelOutput> EvaluateAsync(8d4d0fa662b14686b1865e0e6d3c598eModelInput input) {
            8d4d0fa662b14686b1865e0e6d3c598eModelOutput output = new 8d4d0fa662b14686b1865e0e6d3c598eModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("image", input.image);
            binding.Bind("grid", output.grid);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
