using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FastBoot.Native;

public static class DiskInterop
{
    private const int BLKGETSIZE64 = unchecked((int)0x80081272);
    private const int BLKSSZGET = 0x1268;
    private const int BLKRRPART = 0x125F;
    
    [DllImport("libc", SetLastError = true)]
    private static extern int ioctl(int fd, int request, ref long data);

    [DllImport("libc", SetLastError = true)]
    private static extern int ioctl(int fd, int request, ref int data);

    [DllImport("libc", SetLastError = true)]
    private static extern int open(string pathname, int flags);

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd);

    [DllImport("libc", SetLastError = true)]
    private static extern int fsync(int fd);

    [DllImport("libc", SetLastError = true)]
    private static extern IntPtr strerror(int errnum);

    private const int O_RDONLY = 0;
    private const int O_WRONLY = 1;
    private const int O_RDWR = 2;

    public static long GetDeviceSize(string devicePath)
    {
        int fd = open(devicePath, O_RDONLY);
        if (fd < 0)
            throw new IOException($"Cannot open device: {devicePath}");

        try
        {
            long size = 0;
            int result = ioctl(fd, BLKGETSIZE64, ref size);
            if (result < 0)
                throw new IOException($"ioctl BLKGETSIZE64 failed: {GetErrorString()}");

            return size;
        }
        finally
        {
            close(fd);
        }
    }

    public static int GetSectorSize(string devicePath)
    {
        int fd = open(devicePath, O_RDONLY);
        if (fd < 0)
            throw new IOException($"Cannot open device: {devicePath}");

        try
        {
            int sectorSize = 0;
            int result = ioctl(fd, BLKSSZGET, ref sectorSize);
            if (result < 0)
                throw new IOException($"ioctl BLKSSZGET failed: {GetErrorString()}");

            return sectorSize;
        }
        finally
        {
            close(fd);
        }
    }

    public static void ReReadPartitions(string devicePath)
    {
        int fd = open(devicePath, O_RDONLY);
        if (fd < 0)
            throw new IOException($"Cannot open device: {devicePath}");

        try
        {
            int result = ioctl(fd, BLKRRPART, IntPtr.Zero);
            if (result < 0)
                Console.WriteLine($"Warning: Could not re-read partitions: {GetErrorString()}");
        }
        finally
        {
            close(fd);
        }
    }

    public static void SyncDevice(string devicePath)
    {
        int fd = open(devicePath, O_WRONLY);
        if (fd < 0)
            throw new IOException($"Cannot open device: {devicePath}");

        try
        {
            int result = fsync(fd);
            if (result < 0)
                throw new IOException($"fsync failed: {GetErrorString()}");
        }
        finally
        {
            close(fd);
        }
    }

    public static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }

    private static string GetErrorString()
    {
        int errno = Marshal.GetLastWin32Error();
        IntPtr ptr = strerror(errno);
        return Marshal.PtrToStringAnsi(ptr) ?? $"errno={errno}";
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int ioctl(int fd, int request, IntPtr data);
}