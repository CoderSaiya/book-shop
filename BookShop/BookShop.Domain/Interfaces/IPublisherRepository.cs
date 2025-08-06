using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IPublisherRepository
{
    Task<Publisher?> GetWithBookAsync(Guid publisherId);
}