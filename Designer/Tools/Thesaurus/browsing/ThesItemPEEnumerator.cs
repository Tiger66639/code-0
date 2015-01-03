// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesItemPEEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   The thes item pe enumerator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The thes item pe enumerator.</summary>
    public class ThesItemPEEnumerator : ThesItemEnumerator
    {
        /// <summary>The f editor.</summary>
        private ObjectTextPatternEditor fEditor; // in case the neuron has an editor attached.

        /// <summary>Initializes a new instance of the <see cref="ThesItemPEEnumerator"/> class. Initializes a new instance of the <see cref="ThesItemPEEnumerator"/>
        ///     class.</summary>
        /// <param name="thes">The thes.</param>
        /// <param name="pos">The pos.</param>
        /// <param name="neuron">The neuron.</param>
        public ThesItemPEEnumerator(Thesaurus thes, Neuron pos, Neuron neuron)
            : base(thes, pos, neuron)
        {
        }

        /// <summary>
        ///     Gets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        public override bool HasChildren
        {
            get
            {
                var iEditor = Item.FindFirstOut((ulong)PredefinedNeurons.TextPatternTopic);
                if (iEditor != null)
                {
                    fEditor = new ObjectTextPatternEditor(Item);
                    fEditor.Name = NeuronInfo.DisplayTitle;

                        // we need to set the name of the editor, cause it relies on the parent.
                }

                return fEditor != null || base.HasChildren;
            }
        }

        /// <summary>Gets or sets a value indicating whether is expanded.</summary>
        public override bool IsExpanded
        {
            get
            {
                return base.IsExpanded;
            }

            set
            {
                if (value && fEditor != null)
                {
                    Items.Add(new BrowsableOutputsEnumerator(fEditor));
                }

                base.IsExpanded = value;
            }
        }
    }
}