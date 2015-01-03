// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetRelationship.cs" company="">
//   
// </copyright>
// <summary>
//   The word net relationship.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The word net relationship.</summary>
    public class WordNetRelationship
    {
        /// <summary>
        ///     Gets or sets the name of the relationship.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="ID" /> of the relationship.
        /// </summary>
        /// <value>
        ///     The ID.
        /// </value>
        public int ID { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this
        ///     <see cref="WordNetRelationship" /> is recursive.
        /// </summary>
        /// <value>
        ///     <c>true</c> if recursive; otherwise, <c>false</c> .
        /// </value>
        public bool Recurses { get; set; }
    }
}