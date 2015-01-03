// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewRelationshipEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   Event hanlder for
//   <see cref="JaStDev.HAB.WordNetSin.RelationshipCreated" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Event hanlder for
    ///     <see cref="JaStDev.HAB.WordNetSin.RelationshipCreated" />
    /// </summary>
    public delegate void NewRelationshipEventHandler(object sender, NewRelationshipEventArgs e);

    /// <summary>
    ///     Event arguments for the
    ///     <see cref="JaStDev.HAB.WordNetSin.RelationshipCreated" /> event.
    /// </summary>
    public class NewRelationshipEventArgs : WordNetEventArgs
    {
        /// <summary>
        ///     Gets or sets the Neuroncluster that contains all the related objects.
        /// </summary>
        public NeuronCluster Related { get; set; }

        /// <summary>
        ///     Gets or sets the synset id of the Neuron.
        /// </summary>
        /// <value>
        ///     The synset id.
        /// </value>
        public int SynsetId { get; set; }
    }
}