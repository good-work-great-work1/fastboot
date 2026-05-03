# FastBoot

FastBoot — a lightweight CLI tool for creating bootable USB drives on Linux.

## Features
- List USB devices
- Write ISO images to USB
- Format USB to FAT32
- Create MBR/GPT partition tables
- Split install.wim for FAT32 compatibility
- Create Windows UEFI USB with NTFS + UEFI:NTFS

## Installation
Download binary for your architecture from Releases.

## Usage
- fastboot list
- fastboot write ubuntu.iso /dev/sdb
- fastboot format /dev/sdb
- fastboot split-wim install.wim 4094
- fastboot uefi-ntfs win11.iso /dev/sdb

## Requirements
- Linux
- Root privileges
- wimtools (optional, for split-wim)
- ntfs-3g (optional, for uefi-ntfs)