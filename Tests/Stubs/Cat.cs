using System;

namespace GranularPermissions.Tests.Stubs
{
    public class Cat : IPermissionManaged
    {
        public string Name { get; set; }
        public CatBreed Breed { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public enum CatBreed
    {
        Bengal, BritishShortHair, BritishLongHair, MaineCoon
    }
}