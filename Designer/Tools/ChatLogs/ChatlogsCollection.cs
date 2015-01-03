// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatlogsCollection.cs" company="">
//   
// </copyright>
// <summary>
//   a collection of chatlogs.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a collection of chatlogs.
    /// </summary>
    public class ChatlogsCollection : ClusterCollection<ChatLog>
    {
        /// <summary>Initializes a new instance of the <see cref="ChatlogsCollection"/> class. Initializes a new instance of the <see cref="ChatlogsCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="cluster">The cluster.</param>
        public ChatlogsCollection(INeuronWrapper owner, NeuronCluster cluster)
            : base(owner, cluster)
        {
        }

        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="ChatLog"/>.</returns>
        public override ChatLog GetWrapperFor(Neuron toWrap)
        {
            return new ChatLog(toWrap);
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return linkMeaning;
        }
    }
}