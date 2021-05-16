using BenchmarkDotNet.Attributes;
using GranularPermissions.Conditions;
using GranularPermissions.Tests.Stubs;

namespace GranularPermissions.Tests
{
    [RPlotExporter, RankColumn]
    public class SystemPerformanceTests
    {
        private PermissionsService _permissionsService;
        private int i = 1;

        [GlobalSetup]
        public void Setup()
        {
            var evaluator = new ConditionEvaluator();
            _permissionsService = new PermissionsService(new ConditionParser(),
                (new PermissionsScanner().All(typeof(Permissions))), evaluator);
        }

        [Benchmark]
        public void TestIntegrationWithResourceBounds()
        {
            _permissionsService.InsertSerialized(new GrantStub
            {
                ConditionCode = "((0 < 1) && false || true) || resource.Name == \"Huel\"",
                GrantType = GrantType.Allow,
                Index = i++,
                NodeKey = Permissions.Product.View.Key,
                PermissionType = PermissionType.ResourceBound,
                Identifier = 1,
                PermissionChain = "Users"
            });
        }
    }
}