// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronDataUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   This is a special <see cref="UndoSystem.UndoItem" /> implementation for extra neuron
//   data, like it's display title.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     This is a special <see cref="UndoSystem.UndoItem" /> implementation for extra neuron
    ///     data, like it's display title.
    /// </summary>
    /// <remarks>
    ///     This is a special type of undo item, cause if the neuron got deleted and
    ///     recreated, it has a different <see cref="NeuronData" /> object, which
    ///     needs to be chaned during the undo operation. This is done by getting the
    ///     new <see cref="NeuronData" /> object, instead of the one that created the
    ///     undo data.
    /// </remarks>
    public class NeuronDataUndoItem : UndoSystem.UndoItem
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="NeuronDataUndoItem"/> class. Initializes a new instance of the <see cref="NeuronDataUndoItem"/>
        ///     class.</summary>
        /// <param name="source">The source.</param>
        /// <param name="prop">The prop.</param>
        /// <param name="value">The value.</param>
        public NeuronDataUndoItem(Neuron source, string prop, object value)
        {
            Value = value;
            Source = source;
            Property = prop;
        }

        #endregion

        #region Functions

        /// <summary>Executes the specified a caller.</summary>
        /// <param name="aCaller">A caller.</param>
        public override void Execute(UndoSystem.UndoStore aCaller)
        {
            if (Brain.Current.IsValidID(Source.ID))
            {
                var iItem = BrainData.Current.NeuronInfo[Source];
                var iType = iItem.GetType();
                iType.GetProperty(Property).SetValue(iItem, Value, null);
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        #endregion

        /// <summary>Checks if this <see cref="UndoSystem.UndoItem"/> has the same target(s) as the
        ///     undo item to compare.</summary>
        /// <param name="toCompare">An undo item to compare this undo item with.</param>
        /// <returns>True, if they have the same target, otherwise false.</returns>
        public override bool HasSameTarget(UndoSystem.UndoItem toCompare)
        {
            var iUndo = toCompare as NeuronDataUndoItem;
            if (iUndo != null)
            {
                return iUndo.Source == Source && iUndo.Property == Property;
            }

            return false;
        }

        /// <summary>Checks if this undo item contains data from the specified<paramref name="source"/> object.</summary>
        /// <param name="source">The object to check for.</param>
        /// <returns>True if this object contains undo data for the specified object.</returns>
        public override bool StoresDataFrom(object source)
        {
            return false;
        }

        #region prop

        /// <summary>
        ///     Gets the previous value of the property.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        ///     Gets the name of the property that was changed.
        /// </summary>
        public string Property { get; private set; }

        /// <summary>
        ///     Gets or sets the source as a neuron. We use a neuron, cause the id of
        ///     the neuron might change if it is deleted. This way, we always have the
        ///     latest id.
        /// </summary>
        /// <value>
        ///     The source.
        /// </value>
        public Neuron Source { get; private set; }

        #endregion
    }
}