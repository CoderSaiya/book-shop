using BookShop.Domain.Models;

namespace BookShop.Application.Interface;

public interface IMailSender
{
    Task SendEmailAsync(EmailMessage message);
}