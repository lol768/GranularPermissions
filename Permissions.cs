using static GranularPermissions.PermissionType;
using ProductModel = GranularPermissions.Models.Product;

namespace GranularPermissions
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
    }
}