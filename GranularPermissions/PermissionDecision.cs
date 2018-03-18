namespace GranularPermissions
{
    public class PermissionDecision
    {
        public IPermissionGrant ConsideredGrant { get; }
        public PermissionResult Result { get; }
        public bool ConditionsSatisfied { get; }

        public PermissionDecision(IPermissionGrant consideredGrant, PermissionResult result, bool conditionsSatisfied)
        {
            ConsideredGrant = consideredGrant;
            Result = result;
            ConditionsSatisfied = conditionsSatisfied;
        }
    }
}