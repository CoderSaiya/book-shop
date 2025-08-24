namespace BookShop.Infrastructure.ML;

public class MlIntentOptions
{
    public string IntentModelPath { get; set; } = "ML/models/intent_llm/onnx/model.onnx";
    public string IntentTokenizerPath { get; set; } = "ML/models/intent_llm/hf_model/tokenizer.json";
    public string IntentLabelsPath { get; set; } = "ML/models/intent_llm/labels.json";
    public int IntentMaxSeqLen { get; set; } = 128;
    public int PadId { get; set; } = 1; // XLM-R
}