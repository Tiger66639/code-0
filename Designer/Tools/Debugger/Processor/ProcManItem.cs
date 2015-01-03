// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcManItem.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for processors, stored by the <see cref="ProcessorManager" />
//   . This allows for folders to be displayed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Base class for processors, stored by the <see cref="ProcessorManager" />
    ///     . This allows for folders to be displayed.
    /// </summary>
    public abstract class ProcManItem : Data.OwnedObject
    {
        #region HasFolder

        /// <summary>
        ///     Gets wether this is a folder or not (ProcManFolder overrides it and
        ///     returns true). This is to let wpf bind to the value.
        /// </summary>
        public virtual bool IsFolder
        {
            get
            {
                return false;
            }
        }

        #endregion

        /// <summary>The get values for.</summary>
        /// <param name="toDisplay">The to display.</param>
        public abstract void GetValuesFor(Variable toDisplay);

        /// <summary>The clear values.</summary>
        public abstract void ClearValues();

        /// <summary>Called when a split <paramref name="path"/> was unselected. Allows us
        ///     to update all the processors for a selectionchange in the splitpaths.</summary>
        /// <param name="path">The path.</param>
        public abstract void OnSplitPathUnSelected(SplitPath path);

        /// <summary>Called when a split <paramref name="path"/> was selected. Allows us to
        ///     update all the processors for a selectionchange in the splitpaths.</summary>
        /// <param name="path">The path.</param>
        public abstract void OnSplitPathSelected(SplitPath path);
    }
}