using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface IPublisherRepository : IRepository<Publisher>
{
    Task<Publisher?> GetWithBookAsync(Guid publisherId);
}