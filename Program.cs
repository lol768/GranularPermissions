using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GranularPermissions.Models;
using Loyc.Collections;
using Loyc.Syntax;
using Loyc.Syntax.Les;

namespace GranularPermissions
{
    class Program
    {
        static void Main(string[] args)
        {
            var results = (new PermissionsScanner()).All(typeof(Permissions));
            foreach (var permission in results)
            {
                Console.WriteLine(permission.ToString());
            }
        }
    }
}