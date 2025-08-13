using System.Text;
using System.Text.Json;
using BookShop.Domain.Models;
using BookShop.Domain.Specifications;
using BookShop.Application.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BookShop.Infrastructure.Services.Background;

public class RabbitMqListener : BackgroundService
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly IMailSender _emailSender;
    private readonly ILogger<RabbitMqListener> _logger;
    private readonly RabbitMqSettings _settings;
    
    public RabbitMqListener(
        IOptions<RabbitMqSettings> opts,
        IMailSender emailSender,
        ILogger<RabbitMqListener> logger)
    {
        _settings = opts.Value;
        _emailSender = emailSender;
        _logger = logger;
        _factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = 5672,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Mở connection bất đồng bộ
        _connection = await _factory.CreateConnectionAsync(cancellationToken)  // :contentReference[oaicite:0]{index=0}
            .ConfigureAwait(false);
        
        // Tạo channel bất đồng bộ
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken)  // :contentReference[oaicite:1]{index=1}
            .ConfigureAwait(false);
        
        // Khai báo queue bất đồng bộ
        await _channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel chưa được khởi tạo.");
        
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var json  = Encoding.UTF8.GetString(ea.Body.ToArray());
                var email = JsonSerializer.Deserialize<EmailMessage>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (email != null)
                {
                    await _emailSender.SendEmailAsync(email);
                    await _channel!.BasicAckAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);
                    _logger.LogInformation("Email sent to {Recipient}", email.ToEmail);
                }
                else
                {
                    await _channel!.BasicNackAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);
                    _logger.LogError("Deserialize EmailMessage failed: {Json}", json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ message");
            }
        };

        await _channel!.BasicConsumeAsync(
            queue:    _settings.QueueName,
            autoAck:  false,
            consumer: consumer);
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken).ConfigureAwait(false);
            _channel.Dispose();
        }
        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken).ConfigureAwait(false);
            _connection.Dispose();
        }
        await base.StopAsync(cancellationToken);
    }
}