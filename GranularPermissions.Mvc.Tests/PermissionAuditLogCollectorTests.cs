using System.Collections.Generic;
using System.Linq;
using GranularPermissions.Events;
using Microsoft.AspNetCore.Hosting.Internal;
using NUnit.Framework;
using Shouldly;

namespace GranularPermissions.Mvc.Tests
{
    [TestFixture]
    public class PermissionAuditLogCollectorTests
    {
        [Test]
        public void TestAuditLogCollection()
        {
            var broadcaster = new StubBroadcaster();
            var hostingEnvironment = new HostingEnvironment();
            hostingEnvironment.EnvironmentName = Microsoft.AspNetCore.Hosting.EnvironmentName.Development;
            
            var sut = new PermissionAuditLogCollector(broadcaster, hostingEnvironment);
            broadcaster.Invoke();
            sut.LoggedEvents.Count.ShouldBe(1);
            sut.LoggedEvents.ToList()[0].ChainName.ShouldBe("unit-test");
        }
    }

    public class StubBroadcaster : IPermissionsEventBroadcaster
    {
        public event ComputedChainDecisionHandler ComputedChainDecision;

        public void Invoke()
        {
            ComputedChainDecision?.Invoke(this, new ComputedChainDecisionEventArgs
            (
                new List<PermissionDecision>(), "unit-test", 1, PermissionResult.Allowed, new GenericNode("Unit.Test", "test") 
            ));
        }
    }
}