using HddSanitizer.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HddSanitizer.Core;

public interface IDriveScanner
{
    Task<IEnumerable<DriveModel>> GetConnectedDrivesAsync();
}
