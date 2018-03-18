using ProductModel = GranularPermissions.Tests.Stubs.Product;

namespace GranularPermissions.Tests.Stubs
{
    public static class Permissions
    {
        public static class Product
        {
            public static readonly ResourceNode<ProductModel> View =
                new ResourceNode<ProductModel>("Product.View", "View an individual product");
            
            public static readonly GenericNode Create =
                new GenericNode("Product.Create", "Create a product");
            
            public static readonly ResourceNode<ProductModel> Buy =
                new ResourceNode<ProductModel>("Product.Purchase", "Purchase an individual product");
        }
        
        public static class Cat
        {
            public static readonly ResourceNode<ProductModel> Pet =
                new ResourceNode<ProductModel>("Cat.Pet", "Pet the cat without being bitten/scratched");
            
            public static readonly GenericNode Adopt =
                new GenericNode("Cat.Adopt", "Be adopted by a cat");
        }
    }
}