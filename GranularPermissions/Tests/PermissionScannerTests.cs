using PermissionsStub = GranularPermissions.Tests.Stubs.Permissions;
using NUnit.Framework;
using Shouldly;

namespace GranularPermissions.Tests
{
    [TestFixture]
    public class PermissionScannerTests
    {
        
        [Test]
        public void TestScanningForOneNestedClass()
        {
            // arrange
            var sut = new PermissionsScanner();
            
            // act..
            var result = sut.All(typeof(PermissionsStub));
            
            // assert!
            result.Keys.ShouldContain("Product.View");
            result.Keys.ShouldContain("Product.Create");
            result.Keys.ShouldContain("Product.Purchase");
            result["Product.Purchase"].PermissionType.ShouldBe(PermissionType.ResourceBound);
            result["Product.Create"].PermissionType.ShouldBe(PermissionType.Generic);
        }
        
        [Test]
        public void TestScanningForMultipleNestedClasses()
        {
            // arrange
            var sut = new PermissionsScanner();
            
            // act..
            var result = sut.All(typeof(PermissionsStub));
            
            // assert!
            result.Keys.ShouldContain("Cat.Pet");
            result.Keys.ShouldContain("Cat.Adopt");
            result["Cat.Pet"].PermissionType.ShouldBe(PermissionType.ResourceBound);
            result["Cat.Adopt"].PermissionType.ShouldBe(PermissionType.Generic);
        }

    }

}