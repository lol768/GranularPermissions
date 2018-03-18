namespace GranularPermissions
{
    public interface IPermissionsService
    {
        void InsertSerialized(IPermissionGrantSerialized serialized, int identifier, string table);
    }

    public interface IPermissionGrantSerialized
    {
        string NodeKey { get; set; }
        string ConditionCode { get; set; }
        GrantType GrantType { get; set; }
        PermissionType PermissionType { get; set; }
        int Index { get; set; }
    }
}