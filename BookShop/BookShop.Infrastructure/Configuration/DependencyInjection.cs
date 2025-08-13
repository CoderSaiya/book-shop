using System.Diagnostics;
using BookShop.Application.Interface;
using BookShop.Application.Services;
using BookShop.Domain.Interfaces;
using BookShop.Infrastructure.Identity;
using BookShop.Infrastructure.Persistence.Data;
using BookShop.Infrastructure.Persistence.Data.Repositories;
using BookShop.Infrastructure.Services.Background;
using BookShop.Infrastructure.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookShop.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    connectionString: configuration["ConnectionStrings:DefaultConnection"],
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);
        
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                // tự động gắn traceId nếu chưa có
                ctx.ProblemDetails.Extensions.TryAdd("traceId",
                    Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier);
            };
        });
        
        services.AddExceptionHandler<AppExceptionHandler>();
        
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPublisherRepository, PublisherRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IPublisherService, PublisherService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddSingleton<IMailSender, EmailSender>();
        
        services.AddHostedService<RabbitMqListener>();
        
        return services;
    }
    
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}