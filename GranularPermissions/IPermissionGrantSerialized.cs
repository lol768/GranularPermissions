namespace GranularPermissions
{
    public interface IPermissionGrantSerialized
    {
        /// <summary>
        /// Permission node name, such as Product.View
        /// Must be a pre-defined node, as made available to the service.
        /// </summary>
        string NodeKey { get; set; }
        
        /// <summary>
        /// DSL code scoping the grant. Optional and only
        /// evaluated for resource-bound grants.
        /// </summary>
        string ConditionCode { get; set; }
        
        /// <summary>
        /// Explicitly: Allow the access or deny it.
        /// </summary>
        GrantType GrantType { get; set; }
        
        /// <summary>
        /// Specifies if the grant requires evaluation in
        /// the context of a resource or if it is generic.
        /// </summary>
        PermissionType PermissionType { get; set; }
        
        /// <summary>
        /// Index, used for ordering.
        /// MUST be unique.
        /// </summary>
        int Index { get; set; }
        
        /// <summary>
        /// Identifier scoping this grant within the
        /// permission chain.
        /// </summary>
        int Identifier { get; set; }
        
        /// <summary>
        /// Name of the permission chain in which
        /// this grant will be placed.
        /// </summary>
        string PermissionChain { get; set; }
    }
}