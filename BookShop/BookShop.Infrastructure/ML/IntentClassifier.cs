using System.Text.Json;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using BookShop.Application.DTOs;
using BookShop.Application.Interface.AI;
using Tokenizers.DotNet;

namespace BookShop.Infrastructure.ML;

public sealed class IntentClassifier(
    string onnxPath,
    string tokJsonPath,
    string labelsPath,
    int maxLen = 128)
    : IIntentClassifier, IDisposable
{
    private readonly InferenceSession _session = new(onnxPath);
    private readonly Tokenizer _hfTok = new(vocabPath: tokJsonPath); // load thẳng tokenizer.json
    private readonly string[] _labels = JsonSerializer.Deserialize<string[]>(File.ReadAllText(labelsPath))!;
    private readonly int _padId = TryReadPadIdFromTokenizerJson(tokJsonPath) ?? 0;

    public IntentPredictionDto Predict(string text)
    {
        var ids  = _hfTok.Encode(text).Select(i => (long)i).ToList(); // đã gồm BOS/EOS theo post-processor JSON
        if (ids.Count > maxLen) ids = ids.Take(maxLen).ToList();

        var attn = Enumerable.Repeat(1L, ids.Count).ToList();
        if (ids.Count < maxLen)
        {
            int pad = maxLen - ids.Count;
            ids.AddRange(Enumerable.Repeat((long)_padId, pad));
            attn.AddRange(Enumerable.Repeat(0L, pad));
        }

        var inputIds = new DenseTensor<long>(new[] {1, maxLen});
        var mask     = new DenseTensor<long>(new[] {1, maxLen});
        for (int i = 0; i < maxLen; i++){ inputIds[0,i]=ids[i]; mask[0,i]=attn[i]; }

        var inputs = new List<NamedOnnxValue> {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", mask),
        };
        if (_session.InputMetadata.ContainsKey("token_type_ids"))
            inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", new DenseTensor<long>(new[] {1,maxLen})));

        using var results = _session.Run(inputs);
        var logits = results.First().AsTensor<float>().ToArray();
        var max = logits.Max(); var exps = logits.Select(v => MathF.Exp(v-max)).ToArray();
        var sum = exps.Sum(); var probs = exps.Select(v => v/sum).ToArray();
        int idx = Array.IndexOf(probs, probs.Max());
        return new IntentPredictionDto(_labels[idx], probs[idx], probs);
    }

    public void Dispose() => _session.Dispose();

    static int? TryReadPadIdFromTokenizerJson(string tokJsonPath)
    {
        try
        {
            if (!File.Exists(tokJsonPath)) return null;

            using var doc = JsonDocument.Parse(File.ReadAllText(tokJsonPath));
            var root = doc.RootElement;

            // 1) Đọc "padding": { "pad_id": <int> } nếu tồn tại và là Object
            if (root.TryGetProperty("padding", out var padding)
                && padding.ValueKind == JsonValueKind.Object
                && padding.TryGetProperty("pad_id", out var padIdEl)
                && padIdEl.ValueKind == JsonValueKind.Number
                && padIdEl.TryGetInt32(out var padId))
            {
                return padId;
            }

            // 2) Thử tìm trong "added_tokens": [{ "content": "<pad>", "id": <int>, "special": true }, ...]
            if (root.TryGetProperty("added_tokens", out var added)
                && added.ValueKind == JsonValueKind.Array)
            {
                foreach (var t in added.EnumerateArray())
                {
                    if (t.ValueKind != JsonValueKind.Object) continue;
                    if (t.TryGetProperty("content", out var contentEl)
                        && contentEl.ValueKind == JsonValueKind.String
                        && contentEl.GetString() == "<pad>"
                        && t.TryGetProperty("id", out var idEl)
                        && idEl.ValueKind == JsonValueKind.Number
                        && idEl.TryGetInt32(out var idVal))
                    {
                        return idVal;
                    }
                }
            }
        }
        catch { /* ignored */ }

        return null;
    }
}
