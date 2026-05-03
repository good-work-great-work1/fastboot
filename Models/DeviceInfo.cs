namespace FastBoot.Models;

public class DeviceInfo
{
    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string MountPoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Name,-10} {Size,-10} {Type,-8} {MountPoint,-12} {Model}";
    }
}