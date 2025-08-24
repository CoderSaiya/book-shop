using BookShop.Application.DTOs;

namespace BookShop.Application.Interface.AI;

public interface IIntentClassifier
{
    IntentPredictionDto Predict(string text);
}