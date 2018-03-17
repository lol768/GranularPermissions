using GranularPermissions.Models;
using Loyc.Syntax;
using Loyc.Syntax.Les;
using NUnit.Framework;
using Shouldly;

namespace GranularPermissions.Tests
{
    [TestFixture]
    public class ConditionParserTests
    {
        [Test]
        public void TestSimple()
        {
            var sut = new ConditionEvaluator();
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("0 < 1")).ShouldBe(true);
        }
        
        [Test]
        public void TestIntegration()
        {
            var evaluator = new ConditionEvaluator();

            var sut = new PermissionsService(new ConditionParser(), (new PermissionsScanner().All(typeof(Permissions))), evaluator);
            sut.InsertSerialized(new ResourceGrantStub
            {
                ConditionCode = "0 < 1",
                GrantType = GrantType.Allow,
                Index = 1,
                NodeKey = Permissions.Product.View.Key,
                PermissionType = PermissionType.ResourceBound
            }, 1, "Users");
            
            sut.InsertSerialized(new ResourceGrantStub
            {
                ConditionCode = "0 < 1",
                GrantType = GrantType.Deny,
                Index = 2,
                NodeKey = Permissions.Product.View.Key,
                PermissionType = PermissionType.ResourceBound
            }, 1, "Users");

            sut.GetResultUsingTable("Users", Permissions.Product.View, 1, new Product()).ShouldBe(true);
        }
    }

    public class ResourceGrantStub : IPermissionGrantSerialized
    {
        public string NodeKey { get; set; }
        public string ConditionCode { get; set; }
        public GrantType GrantType { get; set; }
        public PermissionType PermissionType { get; set; }
        public int Index { get; set; }
    }
}