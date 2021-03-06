﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GranularPermissions
{
    public class PermissionsScanner : IPermissionsScanner
    {
        public IDictionary<string, INode> All(Type enclosingClass)
        {
            var staticMemberClasses = enclosingClass.GetNestedTypes();
            var stack = new Stack<Type>();
            var list = new List<INode>();
            var dictionary = new Dictionary<string, INode>();
            foreach (var staticMemberClass in staticMemberClasses)
            {
                stack.Push(staticMemberClass);
            }

            while (stack.Any())
            {
                var type = stack.Pop();
                list.AddRange(type.GetFields().ToList().Where(f => f.IsStatic && !f.IsLiteral).Select(f => f.GetValue(null) as INode));
            }
            
            foreach (var node in list)
            {
                if (dictionary.ContainsKey(node.Key))
                {
                    throw new ArgumentException("At least one duplicate key: " + node.Key, nameof(enclosingClass));
                }

                dictionary[node.Key] = node;
            }

            return dictionary;
        }
    }
}