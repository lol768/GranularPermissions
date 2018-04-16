using System;
using System.Collections.Generic;
using GranularPermissions.Conditions;
using GranularPermissions.Tests.Stubs;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Shouldly;

namespace GranularPermissions.Tests
{
    [TestFixture]
    public class SystemIntegrationTests
    {
        private const string PermissionChainName = "Users";

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
                PermissionType = PermissionType.ResourceBound,
                Identifier = 1,
                PermissionChain = PermissionChainName
            });
            
            sut.InsertSerialized(new GrantStub
            {
                ConditionCode = "1 < 0",
                GrantType = GrantType.Deny,
                Index = 2,
                NodeKey = Permissions.Product.View.Key,
                PermissionType = PermissionType.ResourceBound,
                Identifier = 1,
                PermissionChain = PermissionChainName
            });

            // We have an explicit grant, this should be true
            sut.GetResultUsingChain(PermissionChainName, Permissions.Product.View, 1, new Product()).ShouldBe(PermissionResult.Allowed);
            
            sut.GetResultUsingChain(PermissionChainName, Permissions.Product.Buy, 1, new Product()).ShouldBe(PermissionResult.Unset);

            var entries = new List<IPermissionGrantSerialized>()
            {
                new GrantStub
                {
                    ConditionCode = "false",
                    GrantType = GrantType.Allow,
                    Index = 1,
                    NodeKey = Permissions.Product.View.Key,
                    PermissionType = PermissionType.ResourceBound,
                    Identifier = 1,
                    PermissionChain = PermissionChainName
                }
            };
            
            sut.ReplaceAllGrants(entries);
            sut.GetResultUsingChain(PermissionChainName, Permissions.Product.View, 1, new Product()).ShouldBe(PermissionResult.Unset);
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
                Identifier = 1,
                PermissionChain = PermissionChainName
            });
            
            // We have an explicit grant, this should be true
            sut.GetResultUsingChain(PermissionChainName, Permissions.Product.Create, 1).ShouldBe(PermissionResult.Allowed);
            
            sut.GetResultUsingChain(PermissionChainName, Permissions.Product.Buy, 1, new Product()).ShouldBe(PermissionResult.Unset);
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
                    Identifier = 1,
                    PermissionChain = PermissionChainName
                });
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
                    Identifier = 1,
                    PermissionChain = PermissionChainName
                });
                return true;
            };
            
            Assert.That(del, Throws.TypeOf<ArgumentException>());
        }
        
        [Test]
        public void TestDuplicateIndexes()
        {
            var evaluator = new ConditionEvaluator();

            var sut = new PermissionsService(new ConditionParser(), (new PermissionsScanner().All(typeof(Permissions))), evaluator);
            sut.InsertSerialized(new GrantStub
            {
                GrantType = GrantType.Allow,
                Index = 1,
                NodeKey = Permissions.Product.Create.Key,
                PermissionType = PermissionType.Generic,
                Identifier = 1,
                PermissionChain = PermissionChainName
            });
            
            sut.InsertSerialized(new GrantStub
            {
                GrantType = GrantType.Allow,
                Index = 1,
                NodeKey = Permissions.Product.Buy.Key,
                PermissionType = PermissionType.ResourceBound,
                Identifier = 1,
                PermissionChain = PermissionChainName
            });
            
            sut.GetResultUsingChain(PermissionChainName, Permissions.Product.Create, 1).ShouldBe(PermissionResult.Allowed);
            sut.GetResultUsingChain(PermissionChainName, Permissions.Product.Buy, 1).ShouldBe(PermissionResult.Allowed);

            
        }
        
        
    }
}