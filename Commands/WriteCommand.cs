using System;
using System.Diagnostics;
using System.IO;

namespace FastBoot.Commands;

public static class WriteCommand
{
    public static void Run(string iso, string device)
    {
        if (!File.Exists(iso))
        {
            Console.WriteLine($"Error: ISO file '{iso}' not found.");
            return;
        }

        string devicePath = device.StartsWith("/dev/") ? device : $"/dev/{device}";

        if (!File.Exists(devicePath))
        {
            Console.WriteLine($"Error: Device '{devicePath}' not found.");
            return;
        }

        Console.WriteLine($"WARNING: All data on {devicePath} will be destroyed!");
        Console.Write("Are you sure? (yes/no): ");
        string? answer = Console.ReadLine();

        if (answer?.ToLower() != "yes")
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        Console.WriteLine($"Writing {iso} to {devicePath}...");

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dd",
                    Arguments = $"if=\"{iso}\" of=\"{devicePath}\" bs=4M status=progress conv=fsync",
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("\nWrite completed successfully!");
                Console.WriteLine("Syncing...");
                Sync();
            }
            else
            {
                Console.WriteLine($"\nWrite failed with code: {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void Sync()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sync",
                    RedirectStandardOutput = false,
                    UseShellExecute = true
                }
            };
            process.Start();
            process.WaitForExit();
            Console.WriteLine("Sync complete.");
        }
        catch
        {
            Console.WriteLine("Could not sync. Run 'sync' manually.");
        }
    }
}