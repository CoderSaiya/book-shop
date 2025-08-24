namespace BookShop.Application.DTOs;

public record IntentPredictionDto(
    string Label,
    float Confidence,
    IReadOnlyList<float> Scores);