namespace GranularPermissions.Tests.Stubs
{
    public class Product : IPermissionManaged
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string GetNodeName()
        {
            return "Product";
        }
        public Category Category { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public Product ProductReference { get; set; }
    }
}