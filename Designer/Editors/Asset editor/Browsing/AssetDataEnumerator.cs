// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetDataEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   enumerates through each asset data part from an assetItem
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     enumerates through each asset data part from an assetItem
    /// </summary>
    public class AssetDataEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable
    {
        /// <summary>The f counter.</summary>
        private int fCounter = -1; // -1 to indicate that at first run, a reset needs to be done

        /// <summary>Initializes a new instance of the <see cref="AssetDataEnumerator"/> class. Initializes a new instance of the <see cref="AssetDataEnumerator"/>
        ///     class.</summary>
        /// <param name="item">The item.</param>
        public AssetDataEnumerator(AssetItem item)
        {
            System.Diagnostics.Debug.Assert(item != null);
            Owner = item;
        }

        /// <summary>
        ///     Gets a value indicating whether can be selected. This is a group, so
        ///     it can't be selected, the children can.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [allow selection]; otherwise, <c>false</c> .
        /// </value>
        public bool AllowSelection
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Gets the title tot display. We display the attribute of the assetItem,
        ///     so that it's clear for which attribute this is a record.
        /// </summary>
        public string Title
        {
            get
            {
                var iAttrib = Owner.Attribute;
                if (iAttrib != null)
                {
                    return BrainData.Current.NeuronInfo[iAttrib].DisplayTitle;
                }

                return "Uknown attribute";
            }
        }

        /// <summary>
        ///     Gets or sets the current pos counter.
        /// </summary>
        /// <value>
        ///     The counter.
        /// </value>
        public int Counter
        {
            get
            {
                return fCounter;
            }

            set
            {
                fCounter = value;
            }
        }

        /// <summary>
        ///     Gets the owner.
        /// </summary>
        public AssetItem Owner { get; private set; }

        #region HasChildren

        /// <summary>
        ///     Gets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        public bool HasChildren
        {
            get
            {
                foreach (var i in Owner.Data)
                {
                    if (i.Value != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c> .
        /// </value>
        public bool IsExpanded { get; set; }

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate
        ///     through the collection.
        /// </returns>
        public System.Collections.IEnumerator GetEnumerator()
        {
            return this;
        }

        #endregion

        /// <summary>
        ///     Gets the current element in the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The enumerator is positioned before the first element of the
        ///     collection or after the last element.
        /// </exception>
        /// <returns>
        ///     The current element in the collection.
        /// </returns>
        public virtual object Current
        {
            get
            {
                var iData = Owner.Data[Counter];
                if (iData.HasChildren)
                {
                    if (Owner.Assets != null && Owner.Assets.Cluster == iData.Value)
                    {
                        return new AssetItemEnumerator(Owner.Assets, iData.Column.Name);
                    }

                    return new AssetClusterEnumerator((NeuronCluster)iData.Value, Owner.Root, iData.Column.Name);

                        // don't create an entire asset collection: to much overhead with the monitoring and stuff, only need simple browsing.
                }

                return iData;
            }
        }

        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        /// <returns>
        ///     <see langword="true" /> if the enumerator was successfully advanced to
        ///     the next element; <see langword="false" /> if the enumerator has passed
        ///     the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            Counter++;
            while (Counter < Owner.Data.Count && Owner.Data[Counter].Value == null)
            {
                Counter++;
            }

            return Counter < Owner.Data.Count;
        }

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first
        ///     element in the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        public void Reset()
        {
            fCounter = -1;
        }
    }
}