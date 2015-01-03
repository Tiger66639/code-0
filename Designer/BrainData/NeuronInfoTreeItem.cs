// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronInfoTreeItem.cs" company="">
//   
// </copyright>
// <summary>
//   A base class that can be used to implement a 'static'/snapshot-view on
//   the network, in a tree like structure. Descendents can implement browsing
//   details preloaded or load on demand.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A base class that can be used to implement a 'static'/snapshot-view on
    ///     the network, in a tree like structure. Descendents can implement browsing
    ///     details preloaded or load on demand.
    /// </summary>
    public abstract class NeuronInfoTreeItemBase : INeuronInfo
    {
        #region Fields

        /// <summary>The f item.</summary>
        private Neuron fItem;

        #endregion

        #region Item

        /// <summary>
        ///     Gets the actual neuron.
        /// </summary>
        /// <remarks>
        ///     This allows us to get the neuron as the result value in a combobox for
        ///     instance.
        /// </remarks>
        public Neuron Item
        {
            get
            {
                return fItem;
            }

            set
            {
                if (fItem != value)
                {
                    fItem = value;
                    if (fItem != null)
                    {
                        NeuronInfo = BrainData.Current.NeuronInfo[fItem];
                    }
                    else
                    {
                        NeuronInfo = null;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c> .
        /// </value>
        public abstract bool IsExpanded { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        public abstract bool HasChildren { get; }

        /// <summary>
        ///     Gets the list of children.
        /// </summary>
        public abstract System.Collections.Generic.List<NeuronInfoTreeItemBase> Children { get; }

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        public NeuronData NeuronInfo { get; private set; }

        #endregion
    }

    /// <summary>
    ///     a generic NeuronTreeItem that requires everything to be preloaded.
    /// </summary>
    public class NeuronInfoTreeIten : NeuronInfoTreeItemBase
    {
        /// <summary>The f children.</summary>
        private readonly System.Collections.Generic.List<NeuronInfoTreeItemBase> fChildren =
            new System.Collections.Generic.List<NeuronInfoTreeItemBase>();

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c> .
        /// </value>
        public override bool IsExpanded { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        public override bool HasChildren
        {
            get
            {
                return fChildren.Count > 0;
            }
        }

        #region children

        /// <summary>
        ///     Gets the list of children.
        /// </summary>
        public override System.Collections.Generic.List<NeuronInfoTreeItemBase> Children
        {
            get
            {
                return fChildren;
            }
        }

        #endregion
    }
}