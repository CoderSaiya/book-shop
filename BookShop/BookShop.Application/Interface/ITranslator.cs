namespace BookShop.Application.Interface;

public interface ITranslator
{
    Task<string> TranslateAsync(string text, string from, string to);
}