// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPILinkOut.cs" company="">
//   
// </copyright>
// <summary>
//   Allows to go from the current path item to the neuron found on an
//   outgoing link, for a speicific meaning
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Allows to go from the current path item to the neuron found on an
    ///     outgoing link, for a speicific meaning
    /// </summary>
    public class DPILinkOut : DPILink, ISelectDisplayPathForThes
    {
        /// <summary>Initializes a new instance of the <see cref="DPILinkOut"/> class.</summary>
        public DPILinkOut()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DPILinkOut"/> class.</summary>
        /// <param name="meaning">The meaning.</param>
        public DPILinkOut(ulong meaning)
            : base(meaning)
        {
        }

        #region ISelectDisplayPathForThes Members

        /// <summary>Selects the object related to the specified <paramref name="item"/>
        ///     and returns this (or selects it when a terminator).</summary>
        /// <param name="dataSource"></param>
        /// <param name="item"></param>
        /// <returns>The <see cref="object"/>.</returns>
        public object SelectFrom(Thesaurus dataSource, object item)
        {
            if (item is ThesaurusItem)
            {
                var iItem = (ThesaurusItem)item;
                iItem.IsSelected = true;
                iItem.NeedsBringIntoView = true;
                dataSource.IsSubItemsLoaded = true;
                var iMeaning = Brain.Current[Meaning];
                if (iMeaning is NeuronCluster && ((NeuronCluster)iMeaning).Meaning == (ulong)PredefinedNeurons.Object)
                {
                    for (var i = 0; i < dataSource.ObjectRelated.Items.Count; i++)
                    {
                        var iThesItem = dataSource.ObjectRelated.Items[i];
                        if (iThesItem.Relationship == iMeaning)
                        {
                            dataSource.ObjectRelated.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < dataSource.POSRelated.Items.Count; i++)
                    {
                        var iThesItem = dataSource.POSRelated.Items[i];
                        if (iThesItem.Relationship == iMeaning)
                        {
                            dataSource.POSRelated.SelectedIndex = i;
                            return null;
                        }
                    }

                    for (var i = 0; i < dataSource.Conjugations.Items.Count; i++)
                    {
                        var iThesItem = dataSource.Conjugations.Items[i];
                        if (iThesItem.Relationship == iMeaning)
                        {
                            dataSource.Conjugations.SelectedIndex = i;
                            return null;
                        }
                    }
                }

                foreach (var i in dataSource.SubItems)
                {
                    if (i.Relationship == iMeaning)
                    {
                        return i; // when a sub item collection, return the list.
                    }
                }
            }

            return null;
        }

        #endregion

        /// <summary>returns a code item, based on the path selection method of this item,
        ///     applied to an ICodeItemsOwner.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            var iOwner = owner as CodeItem;
            if (iOwner != null)
            {
                return iOwner.GetCodeItemFor(Meaning);
            }

            return null;
        }

        /// <summary>The get from.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(PatternEditorItem owner)
        {
            if (Meaning == (ulong)PredefinedNeurons.TextPatternOutputs)
            {
                return ((PatternRule)owner).OutputSet;
            }

            if (Meaning == (ulong)PredefinedNeurons.InvalidResponsesForPattern)
            {
                return ((OutputPattern)owner).InvalidResponses;
            }

            if (Meaning == (ulong)PredefinedNeurons.ResponseForOutputs)
            {
                if (owner is PatternRule)
                {
                    return ((PatternRule)owner).ResponsesFor;
                }

                if (owner is ResponsesForGroup)
                {
                    return ((ResponsesForGroup)owner).ResponseFor;
                }
            }
            else if (Meaning == (ulong)PredefinedNeurons.Condition)
            {
                if (owner is PatternRule)
                {
                    return ((PatternRule)owner).Conditionals;
                }

                if (owner is PatternRuleOutput)
                {
                    return ((PatternRuleOutput)owner).Condition;
                }
            }
            else if (Meaning == (ulong)PredefinedNeurons.Evaluate)
            {
                return ((PatternRule)owner).ToEval;
            }
            else if (Meaning == (ulong)PredefinedNeurons.DoPatterns)
            {
                PatternRuleOutput iOwner = null;
                if (owner is PatternRule)
                {
                    iOwner = ((PatternRule)owner).OutputSet;
                }
                else if (owner is PatternRuleOutput)
                {
                    iOwner = (PatternRuleOutput)owner;
                }
                else if (owner is OutputPattern)
                {
                    return ((OutputPattern)owner).Do;
                }

                if (iOwner != null)
                {
                    iOwner.IsDoPatternVisible = true;
                    return iOwner.Do;
                }
            }
            else if (Meaning == (ulong)PredefinedNeurons.Calculate)
            {
                return ((PatternRule)owner).ToCal;
            }

            return null;
        }

        /// <summary>Returns an object from the specified list. This is usually only
        ///     implemented by indexed accessors.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(System.Collections.IList list)
        {
            throw new System.NotImplementedException();
        }
    }
}