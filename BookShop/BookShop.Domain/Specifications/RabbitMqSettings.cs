namespace BookShop.Domain.Specifications;

public class RabbitMqSettings
{
    public string HostName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string QueueName { get; set; } = null!;
}