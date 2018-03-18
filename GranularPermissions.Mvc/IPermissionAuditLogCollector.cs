using System.Collections.Generic;
using GranularPermissions.Events;

namespace GranularPermissions.Mvc
{
    public interface IPermissionAuditLogCollector
    {
        ICollection<ComputedChainDecisionEventArgs> LoggedEvents { get; }
    }
}