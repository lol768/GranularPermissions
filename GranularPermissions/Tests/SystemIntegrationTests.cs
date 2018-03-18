using System;
using GranularPermissions.Tests.Stubs;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Shouldly;

namespace GranularPermissions.Tests
{
    [TestFixture]
    public class SystemIntegrationTests
    { 
        [Test]
        public void TestIntegrationWithResourceBounds()
        {
            var evaluator = new ConditionEvaluator();

            var sut = new PermissionsService(new ConditionParser(), (new PermissionsScanner().All(typeof(Permissions))), evaluator);
            sut.InsertSerialized(new GrantStub
            {
                ConditionCode = "0 < 1",
                GrantType = GrantType.Allow,
                Index = 1,
                NodeKey = Permissions.Product.View.Key,
                PermissionType = PermissionType.ResourceBound
            }, 1, "Users");
            
            sut.InsertSerialized(new GrantStub
            {
                ConditionCode = "1 < 0",
                GrantType = GrantType.Deny,
                Index = 2,
                NodeKey = Permissions.Product.View.Key,
                PermissionType = PermissionType.ResourceBound
            }, 1, "Users");

            // We have an explicit grant, this should be true
            sut.GetResultUsingTable("Users", Permissions.Product.View, 1, new Product()).ShouldBe(PermissionResult.Allowed);
            
            sut.GetResultUsingTable("Users", Permissions.Product.Buy, 1, new Product()).ShouldBe(PermissionResult.Unset);
        }
        
        [Test]
        public void TestIntegrationWithGenericNodes()
        {
            var evaluator = new ConditionEvaluator();

            var sut = new PermissionsService(new ConditionParser(), (new PermissionsScanner().All(typeof(Permissions))), evaluator);
            sut.InsertSerialized(new GrantStub
            {
                GrantType = GrantType.Allow,
                Index = 1,
                NodeKey = Permissions.Product.Create.Key,
                PermissionType = PermissionType.Generic,
            }, 1, "Users");
            
            // We have an explicit grant, this should be true
            sut.GetResultUsingTable("Users", Permissions.Product.Create, 1).ShouldBe(PermissionResult.Allowed);
            
            sut.GetResultUsingTable("Users", Permissions.Product.Buy, 1, new Product()).ShouldBe(PermissionResult.Unset);
        }
        
        [Test]
        public void TestWrongUnderlyingNodeType()
        {
            var evaluator = new ConditionEvaluator();

            var sut = new PermissionsService(new ConditionParser(), (new PermissionsScanner().All(typeof(Permissions))), evaluator);
            ActualValueDelegate<bool> del = () =>
            {
                sut.InsertSerialized(new GrantStub
                {
                    GrantType = GrantType.Allow,
                    Index = 1,
                    NodeKey = Permissions.Product.Create.Key,
                    PermissionType = PermissionType.ResourceBound,
                }, 1, "Users");
                return true;
            };
            
            Assert.That(del, Throws.TypeOf<ArgumentException>());
        }
        
        [Test]
        public void TestNxNode()
        {
            var evaluator = new ConditionEvaluator();

            var sut = new PermissionsService(new ConditionParser(), (new PermissionsScanner().All(typeof(Permissions))), evaluator);
            ActualValueDelegate<bool> del = () =>
            {
                sut.InsertSerialized(new GrantStub
                {
                    GrantType = GrantType.Allow,
                    Index = 1,
                    NodeKey = "Dog.Feed",
                    PermissionType = PermissionType.ResourceBound,
                }, 1, "Users");
                return true;
            };
            
            Assert.That(del, Throws.TypeOf<ArgumentException>());
        }
        
        
    }
}