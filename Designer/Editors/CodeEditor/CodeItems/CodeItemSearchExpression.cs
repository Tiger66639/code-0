// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemSearchExpression.cs" company="">
//   
// </copyright>
// <summary>
//   The code item search expression.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The code item search expression.</summary>
    public class CodeItemSearchExpression : CodeItemResult
    {
        #region ctor-dtor

        /// <summary>Initializes a new instance of the <see cref="CodeItemSearchExpression"/> class. Initializes a new instance of the<see cref="CodeItemSearchExpression"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItemSearchExpression(SearchExpression toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        #endregion

        #region Fields

        /// <summary>The f info to search for.</summary>
        private CodeItemResult fInfoToSearchFor;

        /// <summary>The f list to search.</summary>
        private CodeItemResult fListToSearch;

        /// <summary>The f search for.</summary>
        private CodeItemResult fSearchFor;

        /// <summary>The f to search.</summary>
        private CodeItemResult fToSearch;

        #endregion

        #region Prop

        /// <summary>Gets or sets a value indicating whether is active.</summary>
        public override bool IsActive
        {
            get
            {
                return base.IsActive;
            }

            set
            {
                if (value != base.IsActive)
                {
                    base.IsActive = value;
                    if (InfoToSearchFor != null)
                    {
                        InfoToSearchFor.IsActive = value;
                    }

                    if (ListToSearch != null)
                    {
                        ListToSearch.IsActive = value;
                    }

                    if (SearchFor != null)
                    {
                        SearchFor.IsActive = value;
                    }

                    if (ToSearch != null)
                    {
                        ToSearch.IsActive = value;
                    }
                }
            }
        }

        #region InfoToSearchFor

        /// <summary>
        ///     Gets/sets the info to search for.
        /// </summary>
        /// <remarks>
        ///     Should only be visible if we are searching an info list.
        /// </remarks>
        public CodeItemResult InfoToSearchFor
        {
            get
            {
                return fInfoToSearchFor;
            }

            set
            {
                if (fInfoToSearchFor != value)
                {
                    InternalSetInfoToSearchFor(value);
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.InfoToSearchFor, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }
                }
            }
        }

        /// <summary>The internal set info to search for.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetInfoToSearchFor(CodeItemResult value)
        {
            if (fInfoToSearchFor != null)
            {
                UnRegisterChild(fInfoToSearchFor);
            }

            fInfoToSearchFor = value;
            if (fInfoToSearchFor != null)
            {
                RegisterChild(fInfoToSearchFor);
            }

            OnPropertyChanged("InfoToSearchFor");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasInfoToSearchFor");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event
        }

        #endregion

        #region NotHasInfoToSearchFor

        /// <summary>
        ///     Gets if there is an <see cref="InfoToSearchFor" /> item.
        /// </summary>
        public bool NotHasInfoToSearchFor
        {
            get
            {
                return fInfoToSearchFor == null;
            }
        }

        #endregion

        #region ListToSearch

        /// <summary>
        ///     Gets/sets the identifier for the list to search (to, from, info,...)
        /// </summary>
        public CodeItemResult ListToSearch
        {
            get
            {
                return fListToSearch;
            }

            set
            {
                InternalSetListToSearch(value);
                InternalChange = true;
                try
                {
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.ListToSearch, value);
                }
                finally
                {
                    InternalChange = false;
                }
            }
        }

        /// <summary>The internal set list to search.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetListToSearch(CodeItemResult value)
        {
            if (fListToSearch != null)
            {
                UnRegisterChild(fListToSearch);
            }

            fListToSearch = value;
            if (fListToSearch != null)
            {
                RegisterChild(fListToSearch);
            }

            OnPropertyChanged("ListToSearch");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasListToSearch");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event
        }

        #endregion

        #region NotHasListToSearch

        /// <summary>
        ///     Gets if there is an <see cref="ListToSearch" /> item.
        /// </summary>
        public bool NotHasListToSearch
        {
            get
            {
                return fListToSearch == null;
            }
        }

        #endregion

        #region SearchFor

        /// <summary>
        ///     Gets/sets the item(s) to search for.
        /// </summary>
        public CodeItemResult SearchFor
        {
            get
            {
                return fSearchFor;
            }

            set
            {
                InternalSetSearchFor(value);
                InternalChange = true;
                try
                {
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.SearchFor, value);
                }
                finally
                {
                    InternalChange = false;
                }
            }
        }

        /// <summary>The internal set search for.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetSearchFor(CodeItemResult value)
        {
            if (fSearchFor != null)
            {
                UnRegisterChild(fSearchFor);
            }

            fSearchFor = value;
            if (fSearchFor != null)
            {
                RegisterChild(fSearchFor);
            }

            OnPropertyChanged("SearchFor");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasSearchFor");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event
        }

        #endregion

        #region NotHasSearchFor

        /// <summary>
        ///     Gets if there is an <see cref="SearchFor" /> item.
        /// </summary>
        public bool NotHasSearchFor
        {
            get
            {
                return fSearchFor == null;
            }
        }

        #endregion

        #region ToSearch

        /// <summary>
        ///     Gets/sets the object who's list should be searched.
        /// </summary>
        public CodeItemResult ToSearch
        {
            get
            {
                return fToSearch;
            }

            set
            {
                InternalSetToSearch(value);
                InternalChange = true;
                try
                {
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.ToSearch, value);
                }
                finally
                {
                    InternalChange = false;
                }
            }
        }

        /// <summary>The internal set to search.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetToSearch(CodeItemResult value)
        {
            if (fToSearch != null)
            {
                UnRegisterChild(fToSearch);
            }

            fToSearch = value;
            if (fToSearch != null)
            {
                RegisterChild(fToSearch);
            }

            OnPropertyChanged("ToSearch");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasToSearch");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event
        }

        #endregion

        #region HasToSearch

        /// <summary>
        ///     Gets if there is an <see cref="ToSearch" /> item.
        /// </summary>
        public bool NotHasToSearch
        {
            get
            {
                return fToSearch == null;
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Check if this item or any of it's children wraps the specified neuron,
        ///     if so, the item is made selected.</summary>
        /// <param name="neuron">The neuron.</param>
        public override void Select(Neuron neuron)
        {
            base.Select(neuron);
            CodeItem iItem = InfoToSearchFor;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }

            iItem = SearchFor;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }

            iItem = ToSearch;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }

            iItem = ListToSearch;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            var iToWrap = value as SearchExpression;
            if (iToWrap != null)
            {
                var iFound = iToWrap.InfoToSearchFor;
                if (iFound != null)
                {
                    InternalSetInfoToSearchFor((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }

                iFound = iToWrap.ListToSearch;
                if (iFound != null)
                {
                    InternalSetListToSearch((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }

                iFound = iToWrap.SearchFor;
                if (iFound != null)
                {
                    InternalSetSearchFor((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }

                iFound = iToWrap.ToSearch;
                if (iFound != null)
                {
                    InternalSetToSearch((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }
            }
        }

        /// <summary>The outgoing link removed.</summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.InfoToSearchFor)
                {
                    InternalSetInfoToSearchFor(null);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.ListToSearch)
                {
                    InternalSetListToSearch(null);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.SearchFor)
                {
                    InternalSetSearchFor(null);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.ToSearch)
                {
                    InternalSetToSearch(null);
                }
            }
        }

        /// <summary>The outgoing link created.</summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.InfoToSearchFor)
                {
                    InternalSetInfoToSearchFor(
                        (CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID] as Expression));
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.ListToSearch)
                {
                    InternalSetListToSearch(
                        (CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID] as Expression));
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.SearchFor)
                {
                    InternalSetSearchFor(
                        (CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID] as Expression));
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.ToSearch)
                {
                    InternalSetToSearch(
                        (CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID] as Expression));
                }
            }
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            if (InfoToSearchFor == child)
            {
                InfoToSearchFor = null;
            }
            else if (ListToSearch == child)
            {
                ListToSearch = null;
            }
            else if (SearchFor == child)
            {
                SearchFor = null;
            }
            else if (ToSearch == child)
            {
                ToSearch = null;
            }
            else
            {
                base.RemoveChild(child);
            }
        }

        #endregion
    }
}