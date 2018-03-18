using System;
using System.Collections;
using System.Collections.Generic;
using GranularPermissions.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GranularPermissions.Mvc
{
    public class PermissionAuditLogCollector : IPermissionAuditLogCollector
    {
        private readonly IHostingEnvironment _env;

        public ICollection<ComputedChainDecisionEventArgs> LoggedEvents { get; } =
            new List<ComputedChainDecisionEventArgs>();

        public PermissionAuditLogCollector(IPermissionsEventBroadcaster broadcaster, IHostingEnvironment env)
        {
            _env = env;
            if (_env.IsDevelopment())
            {
                broadcaster.ComputedChainDecision += OnComputedChainDecision;
            }
        }

        private void OnComputedChainDecision(object sender, ComputedChainDecisionEventArgs e)
        {
            LoggedEvents.Add(e);
        }
    }
}