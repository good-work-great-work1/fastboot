using System;
using System.Diagnostics;

namespace FastBoot.Services;

public static class PartitionService
{
    public static void CreateMBR(string device)
    {
        Console.WriteLine("Creating MBR partition table...");
        RunCommand("parted", $"-s {device} mklabel msdos");
    }

    public static void CreateGPT(string device)
    {
        Console.WriteLine("Creating GPT partition table...");
        RunCommand("parted", $"-s {device} mklabel gpt");
    }

    public static string CreatePartition(string device, string fsType = "fat32")
    {
        string partition = device.Contains("nvme") ? $"{device}p1" : $"{device}1";

        Console.WriteLine($"Creating partition {partition}...");
        RunCommand("parted", $"-s {device} mkpart primary {fsType} 0% 100%");

        System.Threading.Thread.Sleep(2000);

        return partition;
    }

    public static void FormatPartition(string partition, string fsType = "fat32")
    {
        string command = fsType switch
        {
            "fat32" => "mkfs.fat",
            "ntfs" => "mkfs.ntfs",
            "ext4" => "mkfs.ext4",
            _ => "mkfs.fat"
        };

        string args = fsType switch
        {
            "fat32" => $"-F32 {partition}",
            _ => partition
        };

        Console.WriteLine($"Formatting {partition} as {fsType}...");
        RunCommand(command, args);
    }

    public static void SetBootable(string partition)
    {
        string device = partition.TrimEnd('1', 'p');
        int partNum = int.Parse(partition[^1].ToString());
        RunCommand("parted", $"-s {device} set {partNum} boot on");
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
                Console.WriteLine($"Warning: {error.Trim()}");
        }
    }
}