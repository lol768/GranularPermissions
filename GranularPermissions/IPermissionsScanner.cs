using System;
using System.Collections;
using System.Collections.Generic;

namespace GranularPermissions
{
    public interface IPermissionsScanner
    {
        IDictionary<string, INode> All(Type enclosingClass);
    }
}