using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GranularPermissions
{
    public interface IPermissionsService
    {
        void InsertSerialized(IPermissionGrantSerialized serialized, int identifier, string table);
    }

    public class PermissionsTable
    {
        private readonly IConditionEvaluator _evaluator;

        public PermissionsTable(IConditionEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        private IDictionary<int, SortedList<int, IPermissionGrant>> _entries =
            new ConcurrentDictionary<int, SortedList<int, IPermissionGrant>>();

        public void Insert(IPermissionGrant grant, int identifier)
        {
            var queue = _entries.ContainsKey(identifier)
                ? _entries[identifier]
                : new SortedList<int, IPermissionGrant>();

            queue.Add(grant.Index, grant);
            _entries[identifier] = queue;
        }

        public bool ResolvePermission(INode nodeToResolve, int identifier, IPermissionManaged resource = null)
        {
            var result = false;
            if (!_entries.ContainsKey(identifier))
            {
                return result;
            }

            var items = _entries[identifier];
            foreach (var keyValuePair in items)
            {
                var grant = keyValuePair.Value;
                if (grant.PermissionType == PermissionType.Generic)
                {
                    var genericGrant = grant as GenericPermissionGrant;
                    result = grant.GrantType == GrantType.Allow;
                }
                else
                {
                    var resourcedGrant = grant as ResourcedPermissionGrant<IPermissionManaged>;
                    var conditionsSatisfied = true;
                    if (resourcedGrant.Condition != null)
                    {
                        conditionsSatisfied = _evaluator.Evaluate(resource, resourcedGrant.Condition);
                    }

                    switch (grant.GrantType)
                    {
                        case GrantType.Allow when conditionsSatisfied:
                            result = true;
                            break;
                        case GrantType.Deny when conditionsSatisfied:
                            result = false;
                            break;
                    }
                }
            }
            return result;
        }
    }

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

        public bool GetResultUsingTable(string table, INode nodeToResolve, int identifier,
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
            var potentialNode = _nodeDefinitions[serialized.NodeKey];

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
                serialized.GrantType, potentialNode as ResourceNode<IPermissionManaged>, compiled, serialized.Index
            );
            
            tableInstance.Insert(resourcedToAdd, identifier);


            Tables[table] = tableInstance;
        }
    }

    public interface IPermissionGrantSerialized
    {
        string NodeKey { get; set; }
        string ConditionCode { get; set; }
        GrantType GrantType { get; set; }
        PermissionType PermissionType { get; set; }
        int Index { get; set; }
    }
}