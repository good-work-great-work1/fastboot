using System;
using System.Diagnostics;
using System.IO;

namespace FastBoot.Services;

public static class WriteService
{
    public static void WriteImage(string isoPath, string devicePath, bool useDd = true)
    {
        if (!File.Exists(isoPath))
            throw new FileNotFoundException($"ISO file not found: {isoPath}");

        if (!File.Exists(devicePath))
            throw new FileNotFoundException($"Device not found: {devicePath}");

        Console.WriteLine($"Source: {isoPath}");
        Console.WriteLine($"Target: {devicePath}");

        var isoInfo = new FileInfo(isoPath);
        Console.WriteLine($"Size: {isoInfo.Length / 1024 / 1024} MB");

        if (useDd)
        {
            WriteWithDd(isoPath, devicePath);
        }
        else
        {
            WriteWithCat(isoPath, devicePath);
        }
    }

    private static void WriteWithDd(string isoPath, string devicePath)
    {
        Console.WriteLine("Writing with dd (with progress)...");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dd",
                Arguments = $"if=\"{isoPath}\" of=\"{devicePath}\" bs=4M status=progress conv=fsync oflag=direct",
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception($"dd failed with exit code: {process.ExitCode}");
    }

    private static void WriteWithCat(string isoPath, string devicePath)
    {
        Console.WriteLine("Writing with cp (fallback method)...");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cp",
                Arguments = $"\"{isoPath}\" \"{devicePath}\"",
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
            throw new Exception($"Write failed: {error}");
        }
    }

    public static void VerifyWrite(string isoPath, string devicePath)
    {
        Console.WriteLine("Verifying write...");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmp",
                Arguments = $"\"{isoPath}\" \"{devicePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode == 0)
            Console.WriteLine("Verification: OK");
        else
            Console.WriteLine("Verification: MISMATCH - write may be corrupted");
    }

    public static void SyncDevice()
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
}