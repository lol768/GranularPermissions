using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GranularPermissions
{
    public class PermissionsService : IPermissionsService
    {
        private readonly IConditionParser _parser;
        private readonly IDictionary<string, INode> _nodeDefinitions;
        private readonly IConditionEvaluator _evaluator;

        private IDictionary<string, PermissionsChain> Chains = new ConcurrentDictionary<string, PermissionsChain>();

        public PermissionsService(IConditionParser parser, IDictionary<string, INode> nodeDefinitions,
            IConditionEvaluator evaluator)
        {
            _parser = parser;
            _nodeDefinitions = nodeDefinitions;
            _evaluator = evaluator;
        }

        public PermissionResult GetResultUsingChain(string chainName, INode nodeToResolve, int identifier,
            IPermissionManaged resource = null)
        {
            if (!Chains.ContainsKey(chainName))
            {
                throw new ArgumentException("Invalid supplied permissions chain");
            }
            return Chains[chainName].ResolvePermission(nodeToResolve, identifier, resource);
        }

        public IDictionary<string, INode> GetDefinedNodes()
        {
            return new ReadOnlyDictionary<string, INode>(_nodeDefinitions);
        }

        public void InsertSerialized(IPermissionGrantSerialized serialized)
        {
            var chainName = serialized.PermissionChain;
            var identifier = serialized.Identifier;
            
            var tableInstance = Chains.ContainsKey(chainName) ? Chains[chainName] : new PermissionsChain(_evaluator);
            if (!_nodeDefinitions.ContainsKey(serialized.NodeKey))
            {
                throw new ArgumentException("No such permission node in definition list");
            }
            var potentialNode = _nodeDefinitions[serialized.NodeKey];
            
            Chains[chainName] = tableInstance;

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