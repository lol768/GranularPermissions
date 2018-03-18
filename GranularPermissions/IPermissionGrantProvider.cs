using System.Collections.Generic;

namespace GranularPermissions
{
    public interface IPermissionGrantProvider
    {
        IEnumerable<IPermissionGrantSerialized> AllGrants();
    }
}