// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewRelationshipTypeEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   Event hanlder for
//   <see cref="JaStDev.HAB.WordNetSin.RelationshipTypeCreated" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Event hanlder for
    ///     <see cref="JaStDev.HAB.WordNetSin.RelationshipTypeCreated" />
    /// </summary>
    public delegate void NewMetaTypeEventHandler(object sender, NewMetaTypeEventArgs e);

    /// <summary>
    ///     Event arguments for the <see cref="WordNet.RelationshipTypeCreated" /> ,
    ///     <see cref="WordNet.RoleCreated" /> and
    ///     <see cref="WordNet.RestrictionCreated" /> event.
    /// </summary>
    public class NewMetaTypeEventArgs : WordNetEventArgs
    {
        /// <summary>
        ///     Gets or sets the relationship/role/restriction as a string value for
        ///     which a new neuron was created.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets any possible description that accompanies the meta data.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is recursive.
        ///     This is used by the event to indicate a new Relationship type.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is recursive; otherwise, <c>false</c> .
        /// </value>
        public bool IsRecursive { get; set; }
    }
}