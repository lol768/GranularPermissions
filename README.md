# GranularPermissions
The world's most over-engineered permissions system

## Features:
* Generic nodes (not tied to a resource)
* Resource-bound nodes, evaluated with respect to a particular resource marked `IPermissionManaged`
* Simple chains allow/deny grants with respect to an identifier (e.g. user ID)
  * Evaluated one after each other in order of provided `Index` to come up with a final pass/fail answer
* Default disallow policy if no matching grants
* Complex resource-bound grants using a DSL which is compiled into AST transformation at startup and then evaluated at runtime
