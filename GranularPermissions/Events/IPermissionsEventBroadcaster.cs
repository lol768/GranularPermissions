namespace GranularPermissions.Events
{
    public delegate void ComputedChainDecisionHandler(object source, ComputedChainDecisionEventArgs e);
    public interface IPermissionsEventBroadcaster
    {
        event ComputedChainDecisionHandler ComputedChainDecision;
    }
}