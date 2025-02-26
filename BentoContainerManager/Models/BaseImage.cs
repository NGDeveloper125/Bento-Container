
namespace BentoContainerManager.Models;

public class BaseImage
{
    private static readonly Dictionary<(Platform, BaseImageType), string> ImageMappings = new()
    {
        { (Platform.Linux, BaseImageType.UbuntuLatest), "ubuntu:latest" },
        { (Platform.Linux, BaseImageType.AlpineLatest), "alpine:latest" },
        { (Platform.Linux, BaseImageType.DebianSlim), "debian:slim" },
        { (Platform.Windows, BaseImageType.WindowsServerCore), "mcr.microsoft.com/windows/servercore:ltsc2022" },
        { (Platform.Windows, BaseImageType.NanoServer), "mcr.microsoft.com/windows/nanoserver:ltsc2022" }
    };

    public Platform Platform { get; set; } = Platform.Linux;
    public BaseImageType ImageType { get; set; } = BaseImageType.UbuntuLatest;
    public string? CustomBaseImage { get; set; }

    public string GetImageName()
    {
        var key = (Platform, ImageType);
        return ImageMappings.TryGetValue(key, out var imageName) 
            ? imageName 
            : ImageMappings[(Platform, GetDefaultImageType(Platform))];
    }

    private static BaseImageType GetDefaultImageType(Platform platform) =>
        platform == Platform.Linux ? BaseImageType.UbuntuLatest : BaseImageType.WindowsServerCore;
}