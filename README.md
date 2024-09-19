# Flash Disk Locker

A small Windows tool that makes a USB flash drive (or any other disk) read-only, so nothing can be written to it or deleted from it, and makes it writable again. Written in C# with WPF on .NET Framework 4.0. It works through Windows' own `diskpart`.

## Features

- Lists the disks with their number, status, size, free space and a USB flash, HDD or SSD icon; a red or green mark shows whether each disk is read-only.
- **Lock** sets the read-only attribute of the selected disk (`attributes disk set readonly`); on a read-only disk the button becomes **Unlock** and clears it. diskpart's own message shows the result, and the list refreshes.
- On a disk that is not USB or SD (a system or internal disk), Lock and Unlock ask for confirmation first.
- The info button shows diskpart's full details of the selected disk; the refresh button reads the list again.
- The **Log** tab shows everything diskpart printed.

## Requirements

- Windows with .NET Framework 4.0 or later. Windows 8, 10 and 11 already include it.
- Administrator rights: diskpart needs them, so Windows asks for them (UAC prompt) every time the app starts.

## Usage

Download the latest `Flash-Disk-Locker-v<version>.7z` from [Releases](https://github.com/Mohammad-Diab/Flash-Disk-Locker/releases), extract it, and run `Flash Disk Locker.exe`. There is no installer.

The exe is not signed, so Windows SmartScreen may warn the first time you run it: click **More info**, then **Run anyway**.

## Building

```
git clone https://github.com/Mohammad-Diab/Flash-Disk-Locker.git
```

Open `Flash Disk Locker.sln` in Visual Studio and build, or run:

```
MSBuild "Flash Disk Locker.sln" /p:Configuration=Release
```

The exe is written to `Flash Disk Locker\bin\Release`.

The project targets .NET Framework 4.0. Recent Visual Studio versions don't ship its reference assemblies; if the build stops with `MSB3644`, extract the [Microsoft.NETFramework.ReferenceAssemblies.net40](https://www.nuget.org/packages/Microsoft.NETFramework.ReferenceAssemblies.net40) NuGet package and point MSBuild at its `build` folder:

```
MSBuild "Flash Disk Locker.sln" /p:Configuration=Release /p:TargetFrameworkRootPath=<package>\build
```

## Author

Mohammad Diab

## License

[Apache 2.0](LICENSE)
