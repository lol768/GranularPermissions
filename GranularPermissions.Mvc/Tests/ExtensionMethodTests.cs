using System;
using System.Collections;
using System.Collections.Generic;
using GranularPermissions.Tests.Stubs;
using Microsoft.Extensions.DependencyInjection;
using PermissionsStub = GranularPermissions.Tests.Stubs.Permissions;
using NUnit.Framework;
using Shouldly;

namespace GranularPermissions.Mvc.Tests
{
    [TestFixture]
    public class ExtensionMethodTests
    {
        
        [Test]
        public void TestServiceCollectionExtensionMethod()
        {
            // arrange
            var sut = new ServiceCollection();
            
            // act
            sut.AddScoped<IPermissionGrantProvider, PermissionGrantProviderStub>();
            sut.AddGranularPermissions(typeof(PermissionsStub));

            // assert
            var sp = sut.BuildServiceProvider();
            var permissionsService = sp.GetService<IPermissionsService>();
            permissionsService.ShouldNotBeNull();
            var result = permissionsService.GetResultUsingChain("Users", PermissionsStub.Cat.Pet, 1, new Cat
            {
                Breed = CatBreed.Bengal,
                DateOfBirth = DateTime.UtcNow,
                Name = "Felix"
            });
            result.ShouldBe(PermissionResult.Allowed);
        }

    }

    class PermissionGrantProviderStub : IPermissionGrantProvider
    {
        public IEnumerable<IPermissionGrantSerialized> AllGrants()
        {
            return new List<IPermissionGrantSerialized>
            {
                new GrantStub
                {
                    ConditionCode = "0 < 1",
                    GrantType = GrantType.Allow,
                    Index = 1,
                    NodeKey = Permissions.Cat.Pet.Key,
                    PermissionType = PermissionType.ResourceBound,
                    Identifier = 1,
                    PermissionChain = "Users"
                }
            };
        }
    }

}