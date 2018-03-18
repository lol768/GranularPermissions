using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GranularPermissions
{
    class PermissionsService : IPermissionsService
    {
        private readonly IConditionParser _parser;
        private readonly IDictionary<string, INode> _nodeDefinitions;
        private readonly IConditionEvaluator _evaluator;

        private IDictionary<string, PermissionsTable> Tables = new ConcurrentDictionary<string, PermissionsTable>();

        public PermissionsService(IConditionParser parser, IDictionary<string, INode> nodeDefinitions,
            IConditionEvaluator evaluator)
        {
            _parser = parser;
            _nodeDefinitions = nodeDefinitions;
            _evaluator = evaluator;
        }

        public PermissionResult GetResultUsingTable(string table, INode nodeToResolve, int identifier,
            IPermissionManaged resource = null)
        {
            if (!Tables.ContainsKey(table))
            {
                throw new ArgumentException("Invalid supplied permissions table");
            }
            return Tables[table].ResolvePermission(nodeToResolve, identifier, resource);
        }

        public void InsertSerialized(IPermissionGrantSerialized serialized, int identifier, string table)
        {
            var tableInstance = Tables.ContainsKey(table) ? Tables[table] : new PermissionsTable(_evaluator);
            if (!_nodeDefinitions.ContainsKey(serialized.NodeKey))
            {
                throw new ArgumentException("No such permission node in definition list");
            }
            var potentialNode = _nodeDefinitions[serialized.NodeKey];
            
            Tables[table] = tableInstance;

            if (serialized.PermissionType == PermissionType.Generic)
            {
                if (potentialNode.PermissionType != PermissionType.Generic)
                {
                    throw new ArgumentException(
                        $"Mismatch in resource/generic node type, was expecting generic for {serialized.NodeKey}");
                }

                var genericToAdd = new GenericPermissionGrant(serialized.GrantType, potentialNode as GenericNode, serialized.Index);
                tableInstance.Insert(genericToAdd, identifier);

                return;
            }

            if (potentialNode.PermissionType != PermissionType.ResourceBound)
            {
                throw new ArgumentException(
                    $"Mismatch in resource/generic node type, was expecting resource bound for {serialized.NodeKey}");
            }

            var compiled = _parser.ParseConditionCode(serialized.ConditionCode);
            var resourcedToAdd = new ResourcedPermissionGrant<IPermissionManaged>(
                serialized.GrantType, (IResourceNode)potentialNode, compiled, serialized.Index
            );
            
            tableInstance.Insert(resourcedToAdd, identifier);
        }
    }
}