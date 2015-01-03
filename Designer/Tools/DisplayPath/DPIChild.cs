// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPIChild.cs" company="">
//   
// </copyright>
// <summary>
//   Contains the information to go from the current position, which should be
//   a cluster, to a child at a specific index.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Contains the information to go from the current position, which should be
    ///     a cluster, to a child at a specific index.
    /// </summary>
    public class DPIChild : DisplayPathItem, ISelectDisplayPathForThes
    {
        #region Index

        /// <summary>
        ///     Gets/sets the index of the child.
        /// </summary>
        public int Index { get; set; }

        #endregion

        #region ISelectDisplayPathForThes Members

        /// <summary>Selects the object related to the specified <paramref name="item"/>
        ///     and returns this.</summary>
        /// <param name="dataSource">The data Source.</param>
        /// <param name="item"></param>
        /// <returns>The <see cref="object"/>.</returns>
        public object SelectFrom(Thesaurus dataSource, object item)
        {
            if (item is ThesaurusItem)
            {
                var iItem = (ThesaurusItem)item;
                return iItem.Items[Index];
            }

            if (item is ThesaurusSubItemCollection)
            {
                return ((ThesaurusSubItemCollection)item)[Index];
            }

            return null;
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DPIChild"/> class.</summary>
        public DPIChild()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DPIChild"/> class.</summary>
        /// <param name="index">The index.</param>
        public DPIChild(int index)
        {
            Index = index;
        }

        #endregion

        #region overrides

        /// <summary>returns a code item, based on the path selection method of this item,
        ///     applied to an ICodeItemsOwner.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            if (owner != null && owner.Items.Count > Index)
            {
                return owner.Items[Index];
            }

            return null;
        }

        /// <summary>Returns a PatterEditorItem, basedon the path selection method ofthis
        ///     item, applied to the owning pattern Editor item. We can only return
        ///     the child of a PatternDef, all others must be reached through the
        ///     linkOut+Child path item.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(PatternEditorItem owner)
        {
            var iDef = owner as PatternRule;
            if (iDef != null && iDef.TextPatterns != null && iDef.TextPatterns.Count > Index)
            {
                // make certain everythin is within range.
                return iDef.TextPatterns[Index];
            }

            if (owner is PatternRuleOutput)
            {
                var iOwner = (PatternRuleOutput)owner;
                if (iOwner.Outputs.Count > Index)
                {
                    // if not within range, simply return null, the display path can't disturb normal workings
                    return iOwner.Outputs[Index];
                }
            }
            else if (owner is ResponsesForGroup)
            {
                var iOwner = (ResponsesForGroup)owner;
                if (iOwner.Conditionals.Count > Index)
                {
                    return iOwner.Conditionals[Index];
                }
            }

            return null;
        }

        /// <summary>Returns an object from the specified list. This is usually only
        ///     implemented by indexed accessors.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(System.Collections.IList list)
        {
            if (list.Count > Index)
            {
                return list[Index];
            }

            return null;
        }

        #endregion
    }
}