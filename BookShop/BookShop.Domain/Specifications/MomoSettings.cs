namespace BookShop.Domain.Specifications;

public class MomoSettings
{
    public string PartnerCode { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public string StatusEndpoint { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
    public string NotifyUrl { get; set; } = null!;
}