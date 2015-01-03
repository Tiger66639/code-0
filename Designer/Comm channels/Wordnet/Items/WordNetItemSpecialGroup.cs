// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetItemSpecialGroup.cs" company="">
//   
// </copyright>
// <summary>
//   some groups are special: for instance, morphs of, which are loaded from a
//   different table, or conjugations, which are loaded from regexs. There are
//   seperate classes for each type so that we can have an easy representation
//   in wpf.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     some groups are special: for instance, morphs of, which are loaded from a
    ///     different table, or conjugations, which are loaded from regexs. There are
    ///     seperate classes for each type so that we can have an easy representation
    ///     in wpf.
    /// </summary>
    public class WordNetItemSpecialGroup : WordNetItemGroup
    {
        /// <summary>
        ///     Loads all the children for this special group. Note that the group
        ///     must already have been added to the WordnetChannel.
        /// </summary>
        protected internal override void LoadChildren()
        {
            var iRoot = Root;
            foreach (WordInfoRow iRow in WordNetSin.Default.GetWordInfoFor(GroupFor, PosString))
            {
                var iNew = new WordNetItem();
                iNew.Synonyms = new System.Collections.Generic.List<string> { iRow.Word };
                iNew.POS = iRow.pos;
                iNew.SynsetID = iRow.synsetid;
                iNew.Description = iRow.definition;
                Children.Add(iNew);
                iNew.LoadRelatedWordsFor(GroupFor, iRow.synsetid, iRoot.SelectedRelationship);
            }
        }

        /// <summary>Called when the IsLoaded value needs to be initialized.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool InitIsLoaded()
        {
            var iRoot = Root;
            if (iRoot != null)
            {
                TextNeuron iFound;
                if (TextSin.Words.TryGetNeuron(iRoot.LastSearchText, out iFound))
                {
                    return iFound.FindFirstOut(RelationshipID) != null;
                }
            }

            return false;
        }
    }

    /// <summary>
    ///     contains all the items that the root is a morph of.
    /// </summary>
    public class WordNetItemMorphs : WordNetItemSpecialGroup
    {
        /// <summary>The load into network.</summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected override void LoadIntoNetwork()
        {
            var iRoot = Root;
            System.Diagnostics.Debug.Assert(iRoot != null);
            var iRootText = BrainHelper.GetNeuronForText(iRoot.LastSearchText);
            var iMorphOf = WordNetSin.Default.ImportMorphOf(iRootText, GroupFor, PosString, iRoot.LastSearchText);
            if (iMorphOf != null)
            {
                ID = iMorphOf.ID;
                iRoot.RegisterMonitorFor(ID); // need to make certain that it is updated when deleted.
            }
            else
            {
                throw new System.InvalidOperationException("import failed!");
            }
        }
    }

    /// <summary>
    ///     group of conjugations.
    /// </summary>
    public class WordNetItemConjugations : WordNetItemSpecialGroup
    {
        /// <summary>
        ///     Gets or sets the regex definition, so we can easely load it when
        ///     needed.
        /// </summary>
        /// <value>
        ///     The regex def.
        /// </value>
        public WordNetSin.RegexDef RegexDef { get; set; }

        /// <summary>The load into network.</summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected override void LoadIntoNetwork()
        {
            var iRoot = Root;
            System.Diagnostics.Debug.Assert(iRoot != null);
            var iRootText = BrainHelper.GetNeuronForText(iRoot.LastSearchText);
            var iMorphOf = WordNetSin.Default.ImportFromRegexDef(iRootText, RegexDef, iRoot.LastSearchText);
            if (iMorphOf != null)
            {
                ID = iMorphOf.ID;
                iRoot.RegisterMonitorFor(ID); // need to make certain that it is updated when deleted.
            }
            else
            {
                throw new System.InvalidOperationException("import failed!");
            }
        }
    }
}