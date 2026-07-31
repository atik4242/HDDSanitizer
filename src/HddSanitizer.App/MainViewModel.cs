using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HddSanitizer.Core;
using HddSanitizer.Domain;
using HddSanitizer.Infrastructure;
using HddSanitizer.SeaChest;

namespace HddSanitizer.App;

public class MainViewModel
{
    private readonly IDriveScanner _scanner;

    public ObservableCollection<DriveModel> Drives { get; } = new();

    public MainViewModel()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _scanner = new WindowsDriveScanner();
        }
        else
        {
            _scanner = new SeaChestDriveScanner();
        }

        _ = LoadDrivesAsync();
    }

    public async Task LoadDrivesAsync()
    {
        Drives.Clear();
        var result = await _scanner.GetConnectedDrivesAsync();
        foreach (var drive in result)
        {
            Drives.Add(drive);
        }
    }
}
