using System;
using FastBoot.Services;

namespace FastBoot.Commands;

public static class GptCommand
{
    public static void Run(string device)
    {
        string devicePath = device.StartsWith("/dev/") ? device : $"/dev/{device}";

        Console.WriteLine($"WARNING: All data on {devicePath} will be destroyed!");
        Console.Write("Are you sure? (yes/no): ");
        string? answer = Console.ReadLine();

        if (answer?.ToLower() != "yes")
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        PartitionService.CreateGPT(devicePath);
        Console.WriteLine("GPT partition table created.");
    }
}