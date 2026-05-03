using System;
using FastBoot.Commands;

namespace FastBoot;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        switch (args[0].ToLower())
        {
            case "list":
                ListCommand.Run();
                break;
            case "write":
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: fastboot write <iso> <device>");
                    return;
                }
                WriteCommand.Run(args[1], args[2]);
                break;
            case "format":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: fastboot format <device>");
                    return;
                }
                FormatCommand.Run(args[1]);
                break;
            case "split-wim":
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: fastboot split-wim <wimfile> <sizeMB>");
                    return;
                }
                SplitWimCommand.Run(args[1], args[2]);
                break;
            case "info":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: fastboot info <device>");
                    return;
                }
                InfoCommand.Run(args[1]);
                break;
            case "mbr":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: fastboot mbr <device>");
                    return;
                }
                MbrCommand.Run(args[1]);
                break;
            case "gpt":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: fastboot gpt <device>");
                    return;
                }
                GptCommand.Run(args[1]);
                break;
            case "uefi-ntfs":
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: fastboot uefi-ntfs <iso> <device>");
                    return;
                }
                UefiNtfsCommand.Run(args[1], args[2]);
                break;
            case "--help":
            case "-h":
                ShowHelp();
                break;
            default:
                Console.WriteLine($"Unknown command: {args[0]}");
                ShowHelp();
                break;
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine(@"
FastBoot v1.0 - Lightweight bootable USB creator
Usage: fastboot <command> [options]

Commands:
  list                         List devices
  info <device>                Show device info
  write <iso> <device>         Write ISO to USB
  format <device>              Format USB to FAT32
  mbr <device>                 Create MBR partition table
  gpt <device>                 Create GPT partition table
  split-wim <wim> <sizeMB>     Split install.wim
  uefi-ntfs <iso> <device>     Create Windows UEFI USB (NTFS + UEFI:NTFS)

Examples:
  fastboot list
  fastboot info /dev/sdb
  fastboot write ubuntu.iso /dev/sdb
  fastboot format /dev/sdb
  fastboot mbr /dev/sdb
  fastboot gpt /dev/sdb
  fastboot split-wim install.wim 4094
  fastboot uefi-ntfs win11.iso /dev/sdb

WARNING: Requires root/sudo. Wrong device = data loss!
");
    }
}