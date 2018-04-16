# GranularPermissions

[![Build status](https://api.travis-ci.org/lol768/GranularPermissions.svg?branch=master)](https://travis-ci.org/lol768/GranularPermissions)

The world's most over-engineered permissions system.

## Features

* Generic nodes (not tied to a resource)
* Resource-bound nodes, evaluated with respect to a particular resource marked `IPermissionManaged`
* Simple chains allow/deny grants with respect to an identifier (e.g. user ID)
  * Evaluated one after each other in order of provided `Index` to come up with a final pass/fail answer
* Default disallow policy if no matching grants
* Complex resource-bound grants using a DSL which is compiled into AST transformation at startup and then evaluated at runtime
* AspNetMvc (Core) extensions for registering services, logging permissions for debugging etc

## Terminology

* Node: A permission entry. Something that can be done that needs its access controlled.
  * These have a key which by convention is represented as `EntityName.[SubGroup].Action`.
* Grants: Allow/deny rules that cover a single permission node. Can be typed as `Generic` or `ResourceBound`.
* ResourceBound grant: Evaluation of the grant must be performed in the context pf a _resource_.
  * For example, `Product.Edit` probably requires the product in question to be considered when
    checking to see if the user should have the permission or not.
* Generic grant: Does not require a resource when evaluating. E.g. `Product.Create`.
* Chain: grants are organised into _chains_ and have identifiers _within a chain_. When nodes are checked, all
grants that match the identifier in the specified chain are considered.
  * Example chains: `Groups` or `Users`
  * Example identifiers: `Group ID` or `User ID`
* Condition: An additional requirement on top of a ResourceBound grant. Written in a DSL for this permissions system.

## DSL examples

```csharp
new Cat
{
    Breed = CatBreed.Bengal,
    Age = 10,
    Name = "Felix"
}
```

You could write some conditions for a resource bound grant on node `Cat.Adopt`:

`resource.Name == "Felix" || resource.Age < 10`

`resource.Age != 5`

Supported operators: `<=`, `>=`, `<`, `>`, `&&`, `||`, `.`, `==`, `!=`, `!`, `~=` (regex)

## Usage

From an ASP.NET MVC Core project, in `ConfigureServices` in `Startup.cs`:

```csharp
services.AddScoped<IPermissionGrantProvider, SomePermissionGrantProvider>();
services.AddGranularPermissions(typeof(Permissions));
```

`SomePermissionGrantProvider` must implement `IPermissionGrantProvider`. Its role is to
return all grants (which implement `IPermissionGrantSerialized`) persisted in the system.
You may wish to retrieve them from a database, for instance.

The `Permissions` class must define all permission nodes you wish to exist in your project:

```csharp
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
```

To check a permission within a specified chain, call `GetResultUsingChain` on the `IPermissionsService` instance which will return a `PermissionResult` (Unset, Allow or Deny).

It is possible to reload the grants at runtime by calling `ReplaceAllGrants` with a new `IEnumerable<IPermissionGrantSerialized>`.

## Example use-case

A web application has two groups which have different levels of access. Group 1 is for "Everyone" and includes a basic level of permission grants. Group 2, "Administrators" includes a higher level of access for a subset of application users who are in this group.

Within GranularPermissions, you'd have:

* One chain, `"Groups"`
* A set of `IPermissionGrantSerialized` for the "Groups" chain with Identifier=1. These will apply to all users in the _Everyone_ group.
* A set of `IPermissionGrantSerialized` for the "Groups" chain with Identifier=2. These will apply to all users in the _Administrators_ group.
* Some `IPermissionGrantProvider` which returns the aforementioned `IPermissionGrantSerialized` instances from DB

A simple permissions checker would call `GetResultUsingChain("Groups", Permissions.Node.ToCheck, groupId)` for each of the user's groups and then aggregate the results to come up with a final allowed/denied result. For example, the administrators group may be considered a higher priority than the everyone group.

For resource bound nodes, it's simply a case of passing an additional argument `GetResultUsingChain("Groups", Permissions.Node.ToCheckWithResource, groupId, resourceToCheckAgainst)`.

## TODO

* Add ability to register DSL functions/identifiers
