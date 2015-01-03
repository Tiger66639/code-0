// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerSelection.cs" company="">
//   
// </copyright>
// <summary>
//   Provides functionality and contains the data for the selection in an
//   explorer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides functionality and contains the data for the selection in an
    ///     explorer.
    /// </summary>
    public class ExplorerSelection : Data.ObservableObject
    {
        /// <summary>The f lower range.</summary>
        private ulong? fLowerRange;

        /// <summary>The f upper is main selection.</summary>
        private bool fUpperIsMainSelection = true;

        /// <summary>The f upper range.</summary>
        private ulong fUpperRange;

        /// <summary>The f owner.</summary>
        private readonly NeuronExplorer fOwner;

        /// <summary>Initializes a new instance of the <see cref="ExplorerSelection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        public ExplorerSelection(NeuronExplorer owner)
        {
            System.Diagnostics.Debug.Assert(owner != null);
            fOwner = owner;
        }

        #region LowerRange

        /// <summary>
        ///     <para>
        ///         Gets/sets the lower value of the selection range. Can be null, when
        ///         there is only
        ///     </para>
        ///     <list type="number">
        ///         <item>
        ///             <description>selection made.</description>
        ///         </item>
        ///     </list>
        /// </summary>
        public ulong? LowerRange
        {
            get
            {
                return fLowerRange;
            }

            set
            {
                if (value != fLowerRange)
                {
                    fLowerRange = value;
                    if (value == null)
                    {
                        UpperIsMainSelection = true;
                    }
                    else if (UpperIsMainSelection == false)
                    {
                        // when this is the main value, need to let the other part of the system know that it changed.
                        OnPropertyChanged("SelectedID");
                    }

                    OnPropertyChanged("LowerRange");
                    fOwner.UpdateSelectionRange();
                }
            }
        }

        #endregion

        #region UpperRange

        /// <summary>
        ///     Gets/sets the upper value of the selection range.
        /// </summary>
        public ulong UpperRange
        {
            get
            {
                return fUpperRange;
            }

            set
            {
                if (value != fUpperRange)
                {
                    fUpperRange = value;
                    if (UpperIsMainSelection)
                    {
                        // when this is the main value, need to let the other part of the system know that it changed.
                        OnPropertyChanged("SelectedID");
                    }

                    OnPropertyChanged("UpperRange");
                    fOwner.UpdateSelectionRange();
                }
            }
        }

        #endregion

        #region SelectionCount

        /// <summary>
        ///     Gets the number of items that have been selected.
        /// </summary>
        public ulong SelectionCount
        {
            get
            {
                if (LowerRange.HasValue)
                {
                    return UpperRange - LowerRange.Value + 1;

                        // need to compensate in case upper and lower have the same value.s
                }

                return 1;
            }
        }

        #endregion

        #region UpperIsMainSelection

        /// <summary>
        ///     Gets/sets the wether the upper value in the selected range is the main
        ///     (first) item in the selection, or wether it is the lowerRange value.
        ///     This determins the value of
        ///     <see cref="ExplorerSelection.SelectedId" />
        /// </summary>
        public bool UpperIsMainSelection
        {
            get
            {
                return fUpperIsMainSelection;
            }

            set
            {
                if (value != fUpperIsMainSelection)
                {
                    fUpperIsMainSelection = value;
                    OnPropertyChanged("UpperIsMainSelection");
                    OnPropertyChanged("SelectedID");
                }
            }
        }

        #endregion

        #region SelectedItem

        /// <summary>
        ///     Gets the first <see cref="ExplorerItem" /> that is selected, (it
        ///     corresponds to SelectedId. Used for menu items and such.
        /// </summary>
        /// <remarks>
        ///     When the item is out of range, a temporary explorer item is created.
        /// </remarks>
        public ExplorerItem SelectedItem
        {
            get
            {
                var iSelected = SelectedID;
                if (iSelected > Neuron.EmptyId)
                {
                    if (iSelected >= fOwner.CurrentScrollPos
                        && iSelected <= fOwner.CurrentScrollPos + (ulong)fOwner.MaxVisible)
                    {
                        // need to check if the explorer item is currently loaded. if this is not the case, we need to create a temp item.
                        return fOwner.Items[(int)(iSelected - fOwner.CurrentScrollPos)];
                    }

                    return fOwner.CreateExplorerItem(iSelected);
                }

                return null;
            }
        }

        #endregion

        #region SelectedIds

        /// <summary>
        ///     Gets the list of all the selected ids.
        /// </summary>
        /// <remarks>
        ///     This is done through the yield statment so that we don't generate use
        ///     list when the entire thing is selected for instance.
        /// </remarks>
        public System.Collections.Generic.IEnumerable<ulong> SelectedIds
        {
            get
            {
                if (LowerRange.HasValue)
                {
                    for (var i = LowerRange.Value; i <= UpperRange; i++)
                    {
                        if (Brain.Current.IsValidID(i))
                        {
                            // we use isValidID since this is faster than IsExisting                 
                            yield return i;
                        }
                    }
                }
                else
                {
                    yield return UpperRange;
                }
            }
        }

        #endregion

        #region SelectedItems

        /// <summary>
        ///     Gets the list of all the selected explorer items, that are visible on
        ///     the ui.
        /// </summary>
        /// <remarks>
        ///     This is done through the yield statment so that we don't generate use
        ///     list. We only return the currently visible items and don't generate
        ///     them to be certain that things don't go wrong.
        /// </remarks>
        public System.Collections.Generic.IEnumerable<NeuronExplorerItem> SelectedItems
        {
            get
            {
                if (LowerRange.HasValue)
                {
                    var iLower = LowerRange.Value < fOwner.CurrentScrollPos ? fOwner.CurrentScrollPos : LowerRange.Value;
                    var iMax = fOwner.CurrentScrollPos + (ulong)fOwner.MaxVisible;
                    var iUpper = UpperRange > iMax ? iMax : UpperRange;
                    for (var i = iLower; i <= iUpper; i++)
                    {
                        var iItem = fOwner.Items[(int)(i - fOwner.CurrentScrollPos)] as NeuronExplorerItem;
                        yield return iItem;
                    }
                }
                else
                {
                    yield return SelectedItem as NeuronExplorerItem;
                }
            }
        }

        #endregion

        /// <summary>Determines whether the item is within the selection range.</summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if [contains] [the specified value]; otherwise,<c>false</c> .</returns>
        internal bool Contains(ulong value)
        {
            if (LowerRange.HasValue)
            {
                return value >= LowerRange && value <= UpperRange;
            }

            return value == UpperRange;
        }

        /// <summary>Selects a range, using the already selected <paramref name="value"/>
        ///     in selectedId. When there is no previous selected value, the
        ///     upperRange is set. When both ranges are filled in, the range that
        ///     receives the new value, will be made the active one.</summary>
        /// <param name="value">The value.</param>
        internal void SelectRange(ulong value)
        {
            var iSelected = SelectedID;
            if (iSelected >= Neuron.StartId && value != iSelected)
            {
                if (iSelected > value)
                {
                    fUpperRange = iSelected;
                    fLowerRange = value;
                    fUpperIsMainSelection = false;
                }
                else
                {
                    fUpperRange = value;
                    fLowerRange = iSelected;
                    fUpperIsMainSelection = true;
                }

                SelectionUpdated();
            }
            else
            {
                SelectedID = value;
            }
        }

        /// <summary>Checks if there is a neuron within the range of selected items that
        ///     can be deleted. If non can be deleted, a <see langword="false"/> is
        ///     returned, otherwise true.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool SelectionCanBeDeleted()
        {
            if (SelectionCount > 0)
            {
                foreach (var i in SelectedIds)
                {
                    Neuron iNeuron;
                    if (Brain.Current.TryFindNeuron(i, out iNeuron))
                    {
                        // we allow deleted items within the range
                        if (iNeuron.CanBeDeleted)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            return false;
        }

        /// <summary>Adjusts the selection range when an item got deleted if this is
        ///     required (the deleted item was a border selection item)</summary>
        /// <param name="id">The id.</param>
        public void AdjustForDelete(ulong id)
        {
            if (UpperRange == id)
            {
                if (LowerRange.HasValue)
                {
                    while (UpperRange <= LowerRange.Value)
                    {
                        UpperRange--;
                        if (Brain.Current.IsExistingID(UpperRange))
                        {
                            // find the first valid id.
                            break;
                        }
                    }
                }
                else
                {
                    UpperRange = Neuron.EmptyId; // no selection.
                }
            }
            else if (LowerRange.HasValue && LowerRange == id)
            {
                while (LowerRange.Value <= UpperRange)
                {
                    LowerRange++;
                    if (Brain.Current.IsExistingID(LowerRange.Value))
                    {
                        // find the first valid id.
                        break;
                    }
                }
            }
        }

        #region SelectedID

        /// <summary>
        ///     Gets/sets the main selected item, in case only 1 selected value is
        ///     required. This is either the upper or lower value, depending on
        ///     <see cref="ExplorerSelection." /> . When set, we always set the upper
        ///     value, the lower value is cleared.
        /// </summary>
        public ulong SelectedID
        {
            get
            {
                if (UpperIsMainSelection || LowerRange.HasValue == false)
                {
                    // upper is the default, also when there is no lowerrange.
                    return UpperRange;
                }

                return LowerRange.Value;
            }

            set
            {
                if (value != Neuron.EmptyId)
                {
                    if (value > fOwner.MaxScrollValue)
                    {
                        fOwner.CurrentScrollPos = fOwner.MaxScrollValue;
                    }
                    else if (value < fOwner.CurrentScrollPos
                             || value >= fOwner.CurrentScrollPos + (ulong)fOwner.MaxVisible)
                    {
                        fOwner.CurrentScrollPos = value;
                    }
                }

                fLowerRange = null;
                fUpperIsMainSelection = true;
                fUpperRange = value;
                SelectionUpdated();
            }
        }

        /// <summary>
        ///     lets observers know that the selection has been changed.
        /// </summary>
        private void SelectionUpdated()
        {
            OnPropertyChanged("UpperIsMainSelection");
            OnPropertyChanged("SelectedID");
            OnPropertyChanged("UpperRange");
            OnPropertyChanged("LowerRange");
            fOwner.UpdateSelectionRange();
        }

        #endregion
    }
}