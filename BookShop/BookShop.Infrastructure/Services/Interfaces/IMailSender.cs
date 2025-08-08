using BookShop.Domain.Models;

namespace BookShop.Infrastructure.Services.Interfaces;

public interface IMailSender
{
    Task SendEmailAsync(EmailMessage message);
}