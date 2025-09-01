using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace BookShop.Application.Helpers;

public static class ImageBase64Helper
{
    public static async Task<string> ToWebpBase64Async(Stream input, int maxW, int maxH, int quality = 75, CancellationToken ct = default)
    {
        using var img = await SixLabors.ImageSharp.Image.LoadAsync(input, ct);
        img.Mutate(x => x.Resize(new ResizeOptions {
            Mode = ResizeMode.Max,
            Size = new Size(maxW, maxH)
        }));

        using var ms = new MemoryStream();
        await img.SaveAsWebpAsync(ms, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder {
            Quality = quality
        }, ct);
        var bytes = ms.ToArray();
        return $"data:image/webp;base64,{Convert.ToBase64String(bytes)}";
    }

    public static async Task<string> OriginalToWebpBase64Async(Stream input, int maxW = 1600, int maxH = 1600, int quality = 80, CancellationToken ct = default)
    {
        using var img = await SixLabors.ImageSharp.Image.LoadAsync(input, ct);
        img.Mutate(x => x.Resize(new ResizeOptions {
            Mode = ResizeMode.Max,
            Size = new Size(maxW, maxH)
        }));
        using var ms = new MemoryStream();
        await img.SaveAsWebpAsync(ms, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder {
            Quality = quality
        }, ct);
        var bytes = ms.ToArray();
        return $"data:image/webp;base64,{Convert.ToBase64String(bytes)}";
    }
}
