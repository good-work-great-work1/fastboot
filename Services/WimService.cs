using System;
using System.Diagnostics;
using System.IO;

namespace FastBoot.Services;

public static class WimService
{
    public static bool IsWimFile(string path)
    {
        if (!File.Exists(path))
            return false;

        try
        {
            using var stream = File.OpenRead(path);
            byte[] header = new byte[8];
            stream.ReadExactly(header, 0, 8);
            
            return header[0] == 'M' && header[1] == 'S' && 
                   header[2] == 'W' && header[3] == 'I' &&
                   header[4] == 'M';
        }
        catch
        {
            return false;
        }
    }

    public static long GetWimSize(string wimPath)
    {
        var fileInfo = new FileInfo(wimPath);
        return fileInfo.Length;
    }

    public static string FindWimsplit()
    {
        string[] possiblePaths = {
            "/usr/bin/wimsplit",
            "/usr/local/bin/wimsplit",
            "/bin/wimsplit"
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = "wimsplit",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(result) && File.Exists(result))
                return result;
        }
        catch
        {
        }

        return string.Empty;
    }

    public static int CalculateChunks(long fileSize, int chunkSizeMB)
    {
        long chunkSizeBytes = chunkSizeMB * 1024L * 1024L;
        return (int)Math.Ceiling((double)fileSize / chunkSizeBytes);
    }

    public static void SplitWim(string wimPath, string outputPath, int chunkSizeMB)
    {
        if (!IsWimFile(wimPath))
            throw new InvalidDataException($"Not a valid WIM file: {wimPath}");

        string wimsplit = FindWimsplit();
        if (string.IsNullOrEmpty(wimsplit))
            throw new FileNotFoundException("wimsplit not found. Install: sudo apt install wimtools");

        long fileSize = GetWimSize(wimPath);
        int chunks = CalculateChunks(fileSize, chunkSizeMB);

        Console.WriteLine($"WIM size: {fileSize / 1024 / 1024} MB");
        Console.WriteLine($"Chunk size: {chunkSizeMB} MB");
        Console.WriteLine($"Chunks: {chunks}");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = wimsplit,
                Arguments = $"\"{wimPath}\" \"{outputPath}\" {chunkSizeMB}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception($"wimsplit failed: {error}");

        Console.WriteLine(output);
    }
}