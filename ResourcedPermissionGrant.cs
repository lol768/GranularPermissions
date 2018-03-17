using System;
using System.Collections.Generic;
using Loyc.Syntax;

namespace GranularPermissions
{
    public interface IPermissionGrant : IComparable<IPermissionGrant>
    {
        PermissionType PermissionType { get; }
        GrantType GrantType { get; }
        int Index { get; }
    }

    public class ResourcedPermissionGrant<T> : IPermissionGrant where T : IPermissionManaged
    {
        public PermissionType PermissionType { get; }
        public GrantType GrantType { get; }
        public int Index { get; }
        public ResourceNode<T> UnderlyingNode { get; }
        public LNode Condition { get; }

        public ResourcedPermissionGrant(GrantType grantType, ResourceNode<T> underlyingNode, LNode condition, int index)
        {
            GrantType = grantType;
            Condition = condition;
            Index = index;
            UnderlyingNode = underlyingNode;
            PermissionType = PermissionType.ResourceBound;
        }

        public override string ToString()
        {
            return $"PermissionGrant[{GrantType}, on {UnderlyingNode} with {Condition.Print()}]";
        }

        public int CompareTo(IPermissionGrant other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Index.CompareTo(other.Index);
        }
    }

    public class GenericPermissionGrant : IPermissionGrant
    {
        public PermissionType PermissionType { get; }
        public GrantType GrantType { get; }
        public int Index { get; }
        public GenericNode UnderlyingNode { get; }

        public GenericPermissionGrant(GrantType grantType, GenericNode underlyingNode, int index)
        {
            GrantType = grantType;
            UnderlyingNode = underlyingNode;
            Index = index;
            PermissionType = PermissionType.Generic;
        }

        public int CompareTo(IPermissionGrant other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Index.CompareTo(other.Index);
        }

        public override string ToString()
        {
            return $"PermissionGrant[{GrantType}, on {UnderlyingNode}]";
        }
    }
    
    public class PermissionGrantComparer : IComparer<IPermissionGrant> {
        public int Compare(IPermissionGrant x, IPermissionGrant y)
        {
            if (x == null && y == null)
                return 0;
            else if (x == null)
                return -1;
            else if (y == null)
                return 1;
           return x.CompareTo(y);
        }
    }
}