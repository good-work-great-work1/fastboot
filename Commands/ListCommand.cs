using System;
using System.Linq;
using FastBoot.Services;

namespace FastBoot.Commands;

public static class ListCommand
{
    public static void Run()
    {
        var devices = UsbService.GetDevices();

        if (devices.Count == 0)
        {
            Console.WriteLine("No devices found.");
            return;
        }

        Console.WriteLine("NAME       SIZE       TYPE     MOUNTPOINT   MODEL");
        Console.WriteLine(new string('-', 60));

        foreach (var device in devices)
        {
            Console.WriteLine(device.ToString());
        }

        Console.WriteLine(new string('-', 60));
        Console.WriteLine($"Total: {devices.Count} device(s)");
        Console.WriteLine("\nUse /dev/NAME to specify device (e.g. /dev/sdb)");
    }
}