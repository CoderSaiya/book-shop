namespace BookShop.Application.Interface;

public interface ITextHasher
{
    string ComputeHash(string text);
}