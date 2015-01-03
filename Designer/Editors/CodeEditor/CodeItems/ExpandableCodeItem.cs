// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpandableCodeItem.cs" company="">
//   
// </copyright>
// <summary>
//   A code item that adds support for an
//   <see cref="JaStDev.HAB.Designer.ExpandableCodeItem.IsExpanded" />
//   property.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A code item that adds support for an
    ///     <see cref="JaStDev.HAB.Designer.ExpandableCodeItem.IsExpanded" />
    ///     property.
    /// </summary>
    public class ExpandableCodeItem : CodeItem
    {
        #region fields

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded = true;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ExpandableCodeItem"/> class. Initializes a new instance of the <see cref="ExpandableCodeItem"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public ExpandableCodeItem(Neuron toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ExpandableCodeItem"/> class. 
        ///     Initializes a new instance of the <see cref="ExpandableCodeItem"/>
        ///     class.</summary>
        public ExpandableCodeItem()
        {
        }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets/sets if the item is expanded or only the header is showing.
        /// </summary>
        /// <remarks>
        ///     This is usefull to remember the state of the code block, otherwise it
        ///     always shows collapsed.
        /// </remarks>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                if (fIsExpanded != value)
                {
                    try
                    {
                        fIsExpanded = value;
                        OnExpandedChanged();
                        OnPropertyChanged("IsExpanded");

                            // important: do the propChanged call after the expandedChanged is called, otherwise some things might get updated before there is an actual list.
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError("Code editor", e.ToString());
                    }
                }
            }
        }

        /// <summary>
        ///     Called when the code item get expanded or contracted again. Allows
        ///     descendents to dynamically load/unload data if needed. Doesn't do
        ///     anything by default.
        /// </summary>
        protected virtual void OnExpandedChanged()
        {
        }

        #endregion
    }
}