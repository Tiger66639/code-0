// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesauruscsvStreamer.cs" company="">
//   
// </copyright>
// <summary>
//   provides streaming functionality to the thesaurus for csv files. The
//   content is converted to objects and added as children to the specified
//   thesaurus item, or, in the other way, the children of the thesaurus item
//   are written to a csv file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides streaming functionality to the thesaurus for csv files. The
    ///     content is converted to objects and added as children to the specified
    ///     thesaurus item, or, in the other way, the children of the thesaurus item
    ///     are written to a csv file.
    /// </summary>
    internal class ThesauruscsvStreamer
    {
        /// <summary>The f items.</summary>
        private NeuronCluster fItems; // the cluster attached to fRoot that contains the related items.

        /// <summary>The f relationship.</summary>
        private Neuron fRelationship;

        /// <summary>The f root.</summary>
        private ThesaurusItem fRoot; // identifies the item to import into/export from. 

        #region export

        /// <summary>Exports the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="toExport">To export.</param>
        /// <param name="relationship">The relationship.</param>
        public static void Export(string filename, ThesaurusItem toExport, Neuron relationship)
        {
            var iStreamer = new ThesauruscsvStreamer();
            iStreamer.fRoot = toExport;
            System.Collections.Generic.List<Neuron> iToExport;

            var iChildren = toExport.Item.FindFirstOut(relationship.ID) as NeuronCluster;
            if (iChildren == null)
            {
                throw new System.InvalidOperationException("No children to export!");
            }

            var iBuildRes = new System.Text.StringBuilder();
            using (var iList = iChildren.Children) iToExport = iList.ConvertTo<Neuron>();
            try
            {
                for (var i = 0; i < iToExport.Count; i++)
                {
                    if (i > 0)
                    {
                        iBuildRes.Append(", ");
                    }

                    iBuildRes.Append(BrainHelper.GetTextFrom(iToExport[i]));
                }
            }
            finally
            {
                Factories.Default.NLists.Recycle(iToExport);
            }

            using (
                var iFile = new System.IO.FileStream(
                    filename, 
                    System.IO.FileMode.Create, 
                    System.IO.FileAccess.ReadWrite))
            {
                using (System.IO.TextWriter iReader = new System.IO.StreamWriter(iFile)) iReader.Write(iBuildRes.ToString());
            }
        }

        #endregion

        #region import

        /// <summary>Imports the specified file as a csv file and adds the content as
        ///     children of the thesaurus item.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="into">The item to import data into.</param>
        /// <param name="relationship">The relationship to import to (should be the same as that of 'into').</param>
        public static void Import(string filename, ThesaurusItem into, Neuron relationship)
        {
            string iInput;
            using (var iFile = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.TextReader iReader = new System.IO.StreamReader(iFile)) iInput = iReader.ReadToEnd();
            }

            ImportString(iInput, into, relationship);
        }

        /// <summary>Imports the string.</summary>
        /// <param name="iInput">The i input.</param>
        /// <param name="into">The into.</param>
        /// <param name="relationship">The relationship.</param>
        public static void ImportString(string iInput, ThesaurusItem into, Neuron relationship)
        {
            var iStreamer = new ThesauruscsvStreamer();
            iStreamer.fRoot = into;
            iStreamer.fRelationship = relationship;
            iStreamer.fItems = into.Item.FindFirstOut(relationship.ID) as NeuronCluster;

            var iWords = iInput.Split(new[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var iWord in iWords)
            {
                iStreamer.ImportWord(iWord.Trim());
            }
        }

        /// <summary>imports data from a drag operation (originating from wordnet) to the
        ///     thesaurus.</summary>
        /// <param name="input"></param>
        /// <param name="into"></param>
        /// <param name="relationship"></param>
        /// <param name="description"></param>
        public static void ImportFromWordNet(System.Collections.Generic.List<string> input, 
            ThesaurusItem into, 
            Neuron relationship, 
            string description)
        {
            System.Diagnostics.Debug.Assert(input.Count > 0);
            var iStreamer = new ThesauruscsvStreamer();
            iStreamer.fRoot = into;
            iStreamer.fRelationship = relationship;
            iStreamer.fItems = into.Item.FindFirstOut(relationship.ID) as NeuronCluster;

            var iObj = iStreamer.ImportWord(input[0].Trim(), description);
            if (iObj != null)
            {
                for (var i = 1; i < input.Count; i++)
                {
                    EditorsHelper.AddSynonym(iObj as NeuronCluster, input[i], into.POS);
                }
            }

            into.IsExpanded = true;
        }

        /// <summary>Imports the word. Doesn't create a pos for this, cause the list could
        ///     be large. First checks if there isn't already an object in the list
        ///     for the specified, word, if so, no new item is created.</summary>
        /// <param name="word">The word.</param>
        /// <param name="description">The description.</param>
        /// <returns>the newly created item, or <see langword="null"/> if it already
        ///     existed.</returns>
        private Neuron ImportWord(string word, string description = null)
        {
            var iWord = EditorsHelper.ConvertStringToNeurons(word);

                // we can get the neurons for the word, if it's already in the list, the neurons already exist, if it's not, we need them to create a new object.
            if (fItems == null)
            {
                fItems = NeuronFactory.GetCluster();
                WindowMain.AddItemToBrain(fItems);
                fItems.Meaning = fRelationship.ID;
                Link.Create(fRoot.Item, fItems, fRelationship);
            }

            System.Collections.Generic.List<NeuronCluster> iChilren;
            using (var iList = fItems.Children) iChilren = iList.ConvertTo<NeuronCluster>();
            try
            {
                foreach (var iObj in iChilren)
                {
                    using (var iList = iObj.Children)
                        if (iList.Contains(iWord))
                        {
                            // the item is already in the list, no need to try more.
                            return null; // it already existed, so don't return a value.
                        }
                }
            }
            finally
            {
                Factories.Default.CLists.Recycle(iChilren);
            }

            var iObject = NeuronFactory.GetCluster(); // when we get here, the item wasn't in the list yet.
            WindowMain.AddItemToBrain(iObject);
            iObject.Meaning = (ulong)PredefinedNeurons.Object;
            using (var iChildren = iObject.ChildrenW) iChildren.Add(iWord);
            var iInfo = BrainData.Current.NeuronInfo[iObject];
            iInfo.DisplayTitle = word;
            if (description != null)
            {
                // also store the description is there is any.
                iInfo.StoreDescription(description);
            }

            using (var iChildren = fItems.ChildrenW) iChildren.Add(iObject);
            return iObject;
        }

        #endregion
    }
}