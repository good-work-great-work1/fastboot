using System;
using System.Diagnostics;
using System.IO;

namespace FastBoot.Commands;

public static class UefiNtfsCommand
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
        string answer = Console.ReadLine() ?? string.Empty;

        if (answer.ToLower() != "yes")
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        Console.WriteLine("Creating GPT partition table...");
        RunCommand("parted", $"-s {devicePath} mklabel gpt");

        long deviceSize = GetDeviceSize(devicePath);
        long dataEnd = deviceSize - (1024 * 1024);

        Console.WriteLine("Creating data partition (NTFS)...");
        RunCommand("parted", $"-s {devicePath} mkpart primary ntfs 1MiB {dataEnd}B");

        Console.WriteLine("Creating UEFI:NTFS partition (1MB)...");
        RunCommand("parted", $"-s {devicePath} mkpart primary fat32 {dataEnd}B 100%");
        RunCommand("parted", $"-s {devicePath} set 2 msftdata on");

        string part1 = devicePath.Contains("nvme") ? $"{devicePath}p1" : $"{devicePath}1";
        string part2 = devicePath.Contains("nvme") ? $"{devicePath}p2" : $"{devicePath}2";

        System.Threading.Thread.Sleep(2000);

        Console.WriteLine("Formatting data partition as NTFS...");
        RunCommand("mkfs.ntfs", $"-Q {part1}");

        Console.WriteLine("Formatting UEFI partition as FAT32...");
        RunCommand("mkfs.fat", $"-F32 {part2}");

        string mountData = "/mnt/fastboot_data";
        string mountUefi = "/mnt/fastboot_uefi";
        string mountIso = "/mnt/fastboot_iso";

        Directory.CreateDirectory(mountData);
        Directory.CreateDirectory(mountUefi);
        Directory.CreateDirectory(mountIso);

        Console.WriteLine("Mounting partitions...");
        RunCommand("mount", $"{part1} {mountData}");
        RunCommand("mount", $"{part2} {mountUefi}");
        RunCommand("mount", $"-o loop \"{iso}\" {mountIso}");

        Console.WriteLine("Copying files...");
        RunCommand("cp", $"-r {mountIso}/* {mountData}/");

        Console.WriteLine("Installing UEFI:NTFS bootloader...");
        RunCommand("dd", $"if={mountIso}/efi/boot/bootx64.efi of={mountUefi}/bootx64.efi bs=1M");

        Console.WriteLine("Unmounting...");
        RunCommand("umount", mountIso);
        RunCommand("umount", mountUefi);
        RunCommand("umount", mountData);

        Directory.Delete(mountData);
        Directory.Delete(mountUefi);
        Directory.Delete(mountIso);

        Console.WriteLine("Done! Windows UEFI USB created successfully.");
    }

    private static long GetDeviceSize(string devicePath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "blockdev",
                Arguments = $"--getsize64 {devicePath}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return long.Parse(output.Trim());
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