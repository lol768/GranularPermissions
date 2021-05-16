using System.Collections.Generic;
using GranularPermissions.Events;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace GranularPermissions.Mvc
{
    public class PermissionAuditLogCollector : IPermissionAuditLogCollector
    {
        private readonly IHostEnvironment _env;

        public ICollection<ComputedChainDecisionEventArgs> LoggedEvents => _backingField.GetRange(0, _backingField.Count);

        private readonly List<ComputedChainDecisionEventArgs> _backingField = new List<ComputedChainDecisionEventArgs>();

        public PermissionAuditLogCollector(IPermissionsEventBroadcaster broadcaster, IHostEnvironment env)
        {
            _env = env;
            if (_env.IsDevelopment())
            {
                broadcaster.ComputedChainDecision += OnComputedChainDecision;
            }
        }

        private void OnComputedChainDecision(object sender, ComputedChainDecisionEventArgs e)
        {
            _backingField.Add(e);
        }
    }
}