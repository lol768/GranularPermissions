using System.Collections.Generic;

namespace GranularPermissions
{
    public interface IPermissionsService
    {
        void InsertSerialized(IPermissionGrantSerialized serialized);

        void ReplaceAllGrants(IEnumerable<IPermissionGrantSerialized> entries);

        PermissionResult GetResultUsingChain(string chainName, INode permissionToCheck, int identifier,
            IPermissionManaged resource = null);

        IDictionary<string, INode> GetDefinedNodes();
    }
}