namespace GranularPermissions.Tests.Stubs
{
    public class GrantStub : IPermissionGrantSerialized
    {
        public string NodeKey { get; set; }
        public string ConditionCode { get; set; }
        public GrantType GrantType { get; set; }
        public PermissionType PermissionType { get; set; }
        public int Index { get; set; }
        public int Identifier { get; set; }
        public string PermissionChain { get; set; }
    }
}