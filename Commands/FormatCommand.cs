using System;
using System.Diagnostics;

namespace FastBoot.Commands;

public static class FormatCommand
{
    public static void Run(string device)
    {
        string devicePath = device.StartsWith("/dev/") ? device : $"/dev/{device}";

        if (!System.IO.File.Exists(devicePath))
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

        Console.WriteLine($"Formatting {devicePath} to FAT32...");

        try
        {
            RunCommand("parted", $"-s {devicePath} mklabel msdos");
            RunCommand("parted", $"-s {devicePath} mkpart primary fat32 0% 100%");

            string partition = devicePath.Contains("nvme") ? $"{devicePath}p1" : $"{devicePath}1";

            System.Threading.Thread.Sleep(2000);

            RunCommand("mkfs.fat", $"-F32 {partition}");

            Console.WriteLine("Format completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void RunCommand(string fileName, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
                Console.WriteLine($"Warning: {error}");
        }
    }
}