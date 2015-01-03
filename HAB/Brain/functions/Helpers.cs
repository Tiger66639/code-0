// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helpers.cs" company="">
//   
// </copyright>
// <summary>
//   Stores all the important data for a link. These are always neurons, so we
//   can check if they were deleted during the operation, which could be. If
//   we were to use ulongs, they could be re-used.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Stores all the important data for a link. These are always neurons, so we
    ///     can check if they were deleted during the operation, which could be. If
    ///     we were to use ulongs, they could be re-used.
    /// </summary>
    internal class LinkData
    {
        /// <summary>Gets or sets the neuron.</summary>
        public Neuron Neuron { get; set; }

        /// <summary>Gets or sets the meaning.</summary>
        public Neuron Meaning { get; set; }

        /// <summary>Gets or sets the info.</summary>
        public System.Collections.Generic.List<Neuron> Info { get; set; }

        /// <summary>Gets or sets the link.</summary>
        public Link Link { get; set; }
    }

    /// <summary>
    ///     All the data to duplicate 1 neuron.
    /// </summary>
    internal class DuplicationData
    {
        /// <summary>The f children.</summary>
        internal System.Collections.Generic.List<Neuron> fChildren; // no id, this can be re-used.

        /// <summary>The f links in.</summary>
        internal System.Collections.Generic.List<LinkData> fLinksIn;

        /// <summary>The f links out.</summary>
        internal System.Collections.Generic.List<LinkData> fLinksOut;

        /// <summary>The f meaning.</summary>
        internal Neuron fMeaning;

        /// <summary>The iscluster.</summary>
        internal bool Iscluster = false;

        /// <summary>The lock requests.</summary>
        internal LockRequestList LockRequests; // the lock used during the duplication.

        /// <summary>
        ///     Contains a list of all the already registered locks per neuron, so we
        ///     don't try to register the same item 2 times (mostly possible in
        ///     meanings?)
        /// </summary>
        internal System.Collections.Generic.Dictionary<Neuron, LockRequestList> LockRequestsDict =
            new System.Collections.Generic.Dictionary<Neuron, LockRequestList>();

        /// <summary>Initializes a new instance of the <see cref="DuplicationData"/> class.</summary>
        public DuplicationData()
        {
            LockRequestsDict = LockRequestsFactory.Create();
            LockRequests = LockRequestList.CreateBig();
            fLinksIn = LockRequestsFactory.CreateLinkDataList();
            fLinksOut = LockRequestsFactory.CreateLinkDataList();
        }
    }

    /// <summary>
    ///     Deletiondata needs some extra data to store: the parent clusters, which
    ///     must also be modified.
    /// </summary>
    internal class DeletionData : DuplicationData
    {
        /// <summary>The f parents.</summary>
        internal System.Collections.Generic.List<NeuronCluster> fParents;
    }
}