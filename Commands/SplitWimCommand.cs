using System;
using System.Diagnostics;
using System.IO;

namespace FastBoot.Commands;

public static class SplitWimCommand
{
    public static void Run(string wimFile, string sizeMB)
    {
        if (!File.Exists(wimFile))
        {
            Console.WriteLine($"Error: WIM file '{wimFile}' not found.");
            return;
        }

        if (!int.TryParse(sizeMB, out int size) || size <= 0 || size > 4095)
        {
            Console.WriteLine("Error: Size must be 1-4095 MB.");
            return;
        }

        string outputFile = Path.ChangeExtension(wimFile, null) + ".swm";

        Console.WriteLine($"Splitting {wimFile} into {size}MB chunks...");
        Console.WriteLine($"Output: {outputFile}");

        try
        {
            string command;
            string arguments;

            if (CommandExists("wimsplit"))
            {
                command = "wimsplit";
                arguments = $"\"{wimFile}\" \"{outputFile}\" {size}";
            }
            else if (CommandExists("dism"))
            {
                string outputDir = Path.GetDirectoryName(outputFile) ?? ".";
                string swmFile = Path.ChangeExtension(wimFile, null) + ".swm";

                command = "dism";
                arguments = $"/Split-Image /ImageFile:\"{wimFile}\" /SWMFile:\"{swmFile}\" /FileSize:{size}";
            }
            else
            {
                Console.WriteLine("Error: Neither wimsplit nor dism found.");
                Console.WriteLine("Install wimtools: sudo apt install wimtools");
                return;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("Split completed successfully!");
                Console.WriteLine($"\nFiles created ({size}MB each):");
                foreach (var file in Directory.GetFiles(
                    Path.GetDirectoryName(outputFile) ?? ".",
                    Path.GetFileNameWithoutExtension(outputFile) + "*.swm"))
                {
                    var fileInfo = new FileInfo(file);
                    Console.WriteLine($"  {fileInfo.Name} - {fileInfo.Length / 1024 / 1024} MB");
                }
            }
            else
            {
                Console.WriteLine($"Split failed with code: {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static bool CommandExists(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}