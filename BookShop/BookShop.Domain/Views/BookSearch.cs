namespace BookShop.Domain.Views;

public class BookSearch
{
    public Guid   BookId { get; set; }
    public string Title { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public string PublisherName { get; set; } = null!;
}