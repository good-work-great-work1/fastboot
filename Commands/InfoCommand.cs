using System;
using FastBoot.Services;

namespace FastBoot.Commands;

public static class InfoCommand
{
    public static void Run(string device)
    {
        string devicePath = device.StartsWith("/dev/") ? device : $"/dev/{device}";
        UsbService.ShowDeviceInfo(device);
    }
}