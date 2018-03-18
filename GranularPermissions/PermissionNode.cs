using System;

namespace GranularPermissions
{
    public interface INode
    {
        PermissionType PermissionType { get; }
        string Key { get; }
        string Description { get; }
    }
     
    public interface IResourceNode : INode
    {
    }
    
    public class ResourceNode<T> : IResourceNode where T : IPermissionManaged
    {
        public ResourceNode(string key, string description)
        {
            PermissionType = PermissionType.ResourceBound;
            Key = key;
            Description = description;
        }

        public PermissionType PermissionType { get; }
        public string Key { get; }
        public string Description { get; }

        public bool IsGranted(IPermissionsService service, T resourceToCheck)
        {
            return false;
        }

        public override string ToString()
        {
            return $"ResourceBound[{typeof(T).Name}].{Key} = {Description}";
        }
    }

    public class GenericNode: INode
    {
        public GenericNode(string key, string description)
        {
            PermissionType = PermissionType.Generic;
            Key = key;
            Description = description;
        }

        public PermissionType PermissionType { get; }
        public string Key { get; }
        public string Description { get; }
        
        public bool IsGranted(IPermissionsService service)
        {
            return false;
        }
        
        public override string ToString()
        {
            return $"Generic.{Key} = {Description}";
        }
    }

    public interface IPermissionManaged
    {
    }
}