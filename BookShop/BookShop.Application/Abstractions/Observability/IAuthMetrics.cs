namespace BookShop.Application.Abstractions.Observability;

public interface IAuthMetrics
{
    void Registered(string method);
    void LoginAttempt(string outcome, string grantType);
    void TokenRefreshed(string outcome);
}