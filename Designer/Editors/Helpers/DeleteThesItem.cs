// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteThesItem.cs" company="">
//   
// </copyright>
// <summary>
//   The editors helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>The editors helper.</summary>
    internal partial class EditorsHelper
    {
        /// <summary>Deletes a neuroncluster that is part of the thesaurus. The cluster is
        ///     presumed to be an object. If it has a textPatternEditor attached, this
        ///     is first deleted. Any attached clusters that manage sub thesaurus
        ///     items are also removed (1 to many, non recursive ). Any attached
        ///     clusters that manage child thesausus items are also removed (cascaded
        ///     relationship). If there are any possible thesaurus children or sub
        ///     thesaurus items found, a question is asked to also delete</summary>
        /// <param name="toDelete">To delete.</param>
        public static void DeleteThesaurusItem(NeuronCluster toDelete)
        {
            var iRels = GetThesRelationships(toDelete);
            if (iRels != null)
            {
                var iRes =
                    System.Windows.MessageBox.Show(
                        "Also delete the children? Note: not removing the children can cause leakage.", 
                        "Delete", 
                        System.Windows.MessageBoxButton.YesNoCancel, 
                        System.Windows.MessageBoxImage.Warning);
                if (iRes == System.Windows.MessageBoxResult.Cancel)
                {
                    return;
                }

                if (iRes == System.Windows.MessageBoxResult.Yes)
                {
                    DeleteThesItem(toDelete, iRels);
                    return;
                }

                iRels = null;
            }

            DeleteThesItem(toDelete, iRels);
        }

        /// <summary>Returns a list of all the thesaurus relationships that are presents on
        ///     the cluster and which are recursive. The list contains the actual
        ///     clusters that wrap the children, not the neurons used on the links.</summary>
        /// <param name="toDelete">The to Delete.</param>
        /// <remarks>When the list is empty, a <see langword="null"/> is returned.</remarks>
        /// <returns>The <see cref="List"/>.</returns>
        private static System.Collections.Generic.List<Neuron> GetThesRelationships(NeuronCluster toDelete)
        {
            var iRes = new System.Collections.Generic.List<Neuron>();
            using (var iList = toDelete.LinksOut)
                foreach (var i in iList)
                {
                    var iFound =
                        (from u in BrainData.Current.Thesaurus.Relationships where u.Item.ID == i.MeaningID select i.To)
                            .FirstOrDefault();
                    if (iFound != null)
                    {
                        iRes.Add(iFound);
                    }
                }

            if (iRes.Count > 0)
            {
                return iRes;
            }

            return null; // don't return a list if it's empty, so we can check for null.
        }

        /// <summary>Returns a list of all the thesaurus relationships that are presents on
        ///     the cluster and which are non recursive. The list contains the actual
        ///     clusters that wrap the children, not the neurons used on the links.</summary>
        /// <param name="toDelete">The to Delete.</param>
        /// <remarks>When the list is empty, a <see langword="null"/> is returned.</remarks>
        /// <returns>The <see cref="List"/>.</returns>
        private static System.Collections.Generic.List<Neuron> GetThesSubRelationships(NeuronCluster toDelete)
        {
            var iRes = new System.Collections.Generic.List<Neuron>();
            using (var iList = toDelete.LinksOut)
                foreach (var i in iList)
                {
                    var iFound =
                        (from u in BrainData.Current.Thesaurus.NoRecursiveRelationships
                         where u.Item.ID == i.MeaningID
                         select i.To).FirstOrDefault();
                    if (iFound != null)
                    {
                        iRes.Add(iFound);
                    }
                }

            if (iRes.Count > 0)
            {
                return iRes;
            }

            return null; // don't return a list if it's empty, so we can check for null.
        }

        /// <summary>Deletes the thesaurus <paramref name="item"/> and all related data.</summary>
        /// <param name="item">The item.</param>
        /// <param name="recursiveRels">The list of neurons that define all the recursive relationships that
        ///     need to be deleted.</param>
        private static void DeleteThesItem(NeuronCluster item, System.Collections.Generic.List<Neuron> recursiveRels)
        {
            var iFound = item.FindFirstOut((ulong)PredefinedNeurons.TextPatternTopic);
            if (iFound != null)
            {
                var iView =
                    (from i in BrainData.Current.OpenDocuments

                     // check if there is an editor open for any of the items, if so, close it.
                     where
                         i is INeuronWrapper && (((INeuronWrapper)i).Item == item || ((INeuronWrapper)i).Item == iFound)
                     select i).FirstOrDefault();
                if (iView != null)
                {
                    BrainData.Current.OpenDocuments.Remove(iView);
                }

                var iEditor = new ObjectTextPatternEditor(iFound);

                    // we open on ifound, not the object, cause otherwise we could be deleting the object itself.
                DeleteTextPatternEditor(iEditor);
            }

            if (recursiveRels != null)
            {
                foreach (var i in recursiveRels)
                {
                    DeleteChildrenOfThesIem(i);
                }
            }

            var iNoRecursive = GetThesSubRelationships(item);
            if (iNoRecursive != null)
            {
                foreach (var i in iNoRecursive)
                {
                    DeleteChildrenOfThesIem(i);
                }
            }

            if (item.ID != Neuron.EmptyId)
            {
                // could be that it is already deleted
                DeleteObject(item);
            }
        }

        /// <summary>Deletes all the children of thesaurus item, for the specified
        ///     relationship.</summary>
        /// <remarks>All the relationsips of the children are also deleted, not just this
        ///     relationship. Children are only deleted when no longer used. The
        ///     cluster that contains all the children is also deleted.</remarks>
        /// <param name="relationship">The relationship.</param>
        private static void DeleteChildrenOfThesIem(Neuron relationship)
        {
            var iChildren = relationship as NeuronCluster;
            if (iChildren != null)
            {
                System.Collections.Generic.List<NeuronCluster> iChildList;
                using (var iList = iChildren.Children) iChildList = iList.ConvertTo<NeuronCluster>();
                try
                {
                    WindowMain.DeleteItemFromBrain(iChildren);

                        // delete the list, so we can easely see which children also need deleting.
                    if (iChildList != null)
                    {
                        foreach (var i in iChildList)
                        {
                            if (ObjectHasParents(i) == false)
                            {
                                // posgroups don't count as parents, only other relationships.
                                DeleteThesItem(i, GetThesRelationships(i));

                                    // we get all the possible relationships for this one, we know that all the children are deleted, cause the parent's relationship gets deleted.
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iChildList);
                }
            }
        }

        /// <summary>Returns <see langword="true"/> if the object has any other parents
        ///     then a posgroup.</summary>
        /// <param name="toCheck">the cluster to check.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool ObjectHasParents(NeuronCluster toCheck)
        {
            var iRes = true;
            int iCount;
            if (toCheck.ClusteredByIdentifier != null)
            {
                using (var iList = toCheck.ClusteredBy) iCount = iList.Count;
            }
            else
            {
                iCount = 0;
            }

            if (iCount == 0)
            {
                return false;
            }

            if (iCount == 1)
            {
                NeuronCluster iParent;
                using (var iList = toCheck.ClusteredBy) iParent = Brain.Current[iList[0]] as NeuronCluster;
                System.Diagnostics.Debug.Assert(iParent != null);
                return iParent.Meaning != (ulong)PredefinedNeurons.POSGroup;
            }

            return iRes;
        }
    }
}