using System;
using System.Collections.Generic;
using System.Diagnostics;
using FastBoot.Models;

namespace FastBoot.Services;

public static class UsbService
{
    public static List<DeviceInfo> GetDevices()
    {
        var devices = new List<DeviceInfo>();

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "lsblk",
                    Arguments = "-d -o NAME,SIZE,TYPE,MOUNTPOINT,MODEL -n",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    devices.Add(new DeviceInfo
                    {
                        Name = parts[0],
                        Size = parts.Length > 1 ? parts[1] : "",
                        Type = parts[2],
                        MountPoint = parts.Length > 3 ? parts[3] : "",
                        Model = parts.Length > 4 ? string.Join(" ", parts[4..]) : ""
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error detecting devices: {ex.Message}");
        }

        return devices;
    }

    public static void ShowDeviceInfo(string device)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "lsblk",
                    Arguments = $"-o NAME,SIZE,TYPE,FSTYPE,MOUNTPOINT,MODEL /dev/{device}",
                    RedirectStandardOutput = false,
                    UseShellExecute = true
                }
            };

            process.Start();
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting device info: {ex.Message}");
        }
    }
}