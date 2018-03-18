using System.Collections.Generic;

namespace GranularPermissions
{
    public interface IPermissionsService
    {
        void InsertSerialized(IPermissionGrantSerialized serialized);

        PermissionResult GetResultUsingChain(string chainName, INode permissionToCheck, int identifier,
            IPermissionManaged resource = null);

        IDictionary<string, INode> GetDefinedNodes();
    }
}