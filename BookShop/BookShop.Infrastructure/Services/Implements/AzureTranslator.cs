using System.Net.Http.Json;
using System.Text.Json;
using BookShop.Application.Interface;
using Microsoft.Extensions.Configuration;

namespace BookShop.Infrastructure.Services.Implements;

public class AzureTranslator(HttpClient http, IConfiguration config) : ITranslator
{
    private readonly string _key = config["Translator:Key"]    ?? throw new("Translator:Key missing");
    private readonly string _region = config["Translator:Region"] ?? throw new("Translator:Region missing");
    private readonly string _endpoint = (config["Translator:Endpoint"] ?? "https://api.cognitive.microsofttranslator.com")
        .TrimEnd('/');

    public async Task<string> TranslateAsync(string text, string from, string to)
    {
        // POST {endpoint}/translate?api-version=3.0&from=vi&to=en
        var url = $"{_endpoint}/translate?api-version=3.0&from={from}&to={to}";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("Ocp-Apim-Subscription-Key", _key);
        req.Headers.Add("Ocp-Apim-Subscription-Region", _region);
        req.Content = JsonContent.Create(new[] { new { Text = text } });

        using var resp = await http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        // Response shape: [{ "translations": [ { "text": "...", "to": "en" } ] }]
        await using var stream = await resp.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);
        var translations = doc.RootElement[0].GetProperty("translations");
        var translated = translations[0].GetProperty("text").GetString();
        return translated ?? text;
    }
}