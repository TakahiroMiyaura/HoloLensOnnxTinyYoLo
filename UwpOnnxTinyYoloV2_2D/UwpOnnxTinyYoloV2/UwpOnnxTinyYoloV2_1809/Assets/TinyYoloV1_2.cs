using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.AI.MachineLearning;
namespace UwpOnnxTinyYoloV2_1809
{
    
    public sealed class TinyYoloV1_2Input
    {
        public TensorFloat image; // shape(-1,3,416,416)
    }
    
    public sealed class TinyYoloV1_2Output
    {
        public TensorFloat grid; // shape(-1,125,13,13)
    }
    
    public sealed class TinyYoloV1_2Model
    {
        private LearningModel model;
        private LearningModelSession session;
        private LearningModelBinding binding;
        public static async Task<TinyYoloV1_2Model> CreateFromStreamAsync(IRandomAccessStreamReference stream)
        {
            TinyYoloV1_2Model learningModel = new TinyYoloV1_2Model();
            learningModel.model = await LearningModel.LoadFromStreamAsync(stream);
            learningModel.session = new LearningModelSession(learningModel.model);
            learningModel.binding = new LearningModelBinding(learningModel.session);
            return learningModel;
        }
        public async Task<TinyYoloV1_2Output> EvaluateAsync(TinyYoloV1_2Input input)
        {
            binding.Bind("image", input.image);
            var result = await session.EvaluateAsync(binding, "0");
            var output = new TinyYoloV1_2Output();
            output.grid = result.Outputs["grid"] as TensorFloat;
            return output;
        }
    }
}
