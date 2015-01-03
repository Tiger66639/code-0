// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FirstLinkOutIndexer.cs" company="">
//   
// </copyright>
// <summary>
//   Provides fast access to referenced (outgoing) neurons. For each scalar
//   value, only the first neuron <see langword="ref" /> is stored, so only the
//   'findFirst' search technique is supporte Not thread save, should be used
//   in a single thread.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Indexing
{
    /// <summary>
    ///     Provides fast access to referenced (outgoing) neurons. For each scalar
    ///     value, only the first neuron <see langword="ref" /> is stored, so only the
    ///     'findFirst' search technique is supporte Not thread save, should be used
    ///     in a single thread.
    /// </summary>
    public class FirstLinkOutIndexer
    {
        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.Dictionary<ulong, LinkOutIndexerItem> fItems =
            new System.Collections.Generic.Dictionary<ulong, LinkOutIndexerItem>();

        /// <summary>Initializes a new instance of the <see cref="FirstLinkOutIndexer"/> class. 
        ///     Initializes a new instance of the <see cref="FirstLinkOutIndexer"/>
        ///     class.</summary>
        public FirstLinkOutIndexer()
        {
            JaStDev.HAB.Brain.Current.Cleared += DB_Cleared;
            JaStDev.HAB.Brain.Current.LinkChanged += DB_LinkChanged;
            JaStDev.HAB.Brain.Current.NeuronChanged += DB_NeuronChanged;
        }

        /// <summary>Looks up the <paramref name="value"/> in the index for the specified
        ///     item and return the id of the first outgoing referenced neuron.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from">From.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        public ulong FindFirst<T>(JaStDev.HAB.Neuron from, T value)
        {
            if (from == null)
            {
                throw new System.ArgumentNullException("from");
            }

            LinkOutIndexerItem iFound;
            if (fItems.TryGetValue(from.ID, out iFound) == false)
            {
                iFound = Add(from);
            }

            iFound.AccessCount++;
            return iFound.FindFirst(value);
        }

        /// <summary>Looks up the <paramref name="value"/> in the index for the specified
        ///     item and return the id of the first outgoing referenced neuron.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from">From.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="from"/></exception>
        /// <returns>The <see cref="ulong"/>.</returns>
        public ulong FindFirst<T>(ulong from, T value)
        {
            if (from == 0)
            {
                throw new System.ArgumentNullException("from");
            }

            LinkOutIndexerItem iFound;
            if (fItems.TryGetValue(from, out iFound) == false)
            {
                iFound = Add(JaStDev.HAB.Brain.Current[from]);
            }

            iFound.AccessCount++;
            return iFound.FindFirst(value);
        }

        /// <summary>adds the specified neuron to the list of indexed items. It's outgoing
        ///     links will be indexed and monitored for changes from now on.</summary>
        /// <param name="toAdd"></param>
        /// <returns>The <see cref="LinkOutIndexerItem"/>.</returns>
        public LinkOutIndexerItem Add(JaStDev.HAB.Neuron toAdd)
        {
            if (toAdd == null)
            {
                throw new System.ArgumentNullException("toAdd");
            }

            var iRes = new LinkOutIndexerItem(toAdd);
            fItems.Add(toAdd.ID, iRes);
            return iRes;
        }

        #region DB syncing

        /// <summary>Handles the NeuronChanged event of the DB control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.NeuronChangedEventArgs"/> instance containing the
        ///     event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void DB_NeuronChanged(object sender, JaStDev.HAB.NeuronChangedEventArgs e)
        {
            switch (e.Action)
            {
                case JaStDev.HAB.BrainAction.Created:
                    break;
                case JaStDev.HAB.BrainAction.Changed:
                    fItems.Remove(e.OriginalSourceID);
                    break;
                case JaStDev.HAB.BrainAction.Removed:
                    fItems.Remove(e.OriginalSourceID);
                    break;
                default:
                    break;
            }
        }

        /// <summary>Handles the LinkChanged event of the DB control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.LinkChangedEventArgs"/> instance containing the event
        ///     data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void DB_LinkChanged(object sender, JaStDev.HAB.LinkChangedEventArgs e)
        {
            LinkOutIndexerItem iFound;
            switch (e.Action)
            {
                case JaStDev.HAB.BrainAction.Created:
                    if (fItems.TryGetValue(e.OriginalSource.FromID, out iFound))
                    {
                        iFound.TryAddValue(e.OriginalSource.Meaning, e.OriginalSource.ToID);
                    }

                    break;
                case JaStDev.HAB.BrainAction.Changed:
                    if (e.OldFrom != e.NewFrom)
                    {
                        // the from part has changed.
                        if (fItems.TryGetValue(e.OldFrom, out iFound))
                        {
                            iFound.TryRemoveValue(e.OriginalSource.Meaning, e.OriginalSource.ToID);
                        }

                        if (fItems.TryGetValue(e.NewFrom, out iFound))
                        {
                            iFound.TryAddValue(e.OriginalSource.Meaning, e.OriginalSource.ToID);
                        }
                    }
                    else if (e.OldTo != e.NewTo && fItems.TryGetValue(e.OldFrom, out iFound))
                    {
                        iFound.TryReplaceValue(e.OriginalSource.Meaning, e.OldTo, e.NewTo);
                    }

                    break;
                case JaStDev.HAB.BrainAction.Removed:
                    break;
                default:
                    break;
            }
        }

        /// <summary>Handles the Cleared event of the DB control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void DB_Cleared(object sender, System.EventArgs e)
        {
            fItems.Clear();
        }

        #endregion
    }
}