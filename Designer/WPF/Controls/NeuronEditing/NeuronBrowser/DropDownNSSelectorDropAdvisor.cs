// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropDownNSSelectorDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Provides default drag drop behaviour for
//   <see cref="DropDownNSSelector" /> objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Provides default drag drop behaviour for
    ///     <see cref="DropDownNSSelector" /> objects.
    /// </summary>
    public class DropDownNSSelectorDropAdvisor : DnD.DropTargetBase
    {
        #region prop

        #region Target

        /// <summary>Gets the target.</summary>
        public DropDownNSSelector Target
        {
            get
            {
                return (DropDownNSSelector)TargetUI;
            }
        }

        #endregion

        #region UsePreviewEvents

        /// <summary>
        ///     Gets if the preview event versions should be used or not.
        /// </summary>
        /// <remarks>
        ///     don't use preview events cause than the sub lists don't get used but
        ///     only the main list cause this gets the events first, while we usually
        ///     want to drop in a sublist.
        /// </remarks>
        public override bool UsePreviewEvents
        {
            get
            {
                return false;
            }
        }

        #endregion

        #endregion

        #region Overrides

        /// <summary>Raises the <see cref="DropCompleted"/> event.</summary>
        /// <param name="arg">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            var iId = (ulong)arg.Data.GetData(Properties.Resources.NeuronIDFormat);
            Target.SelectedNeuron = Brain.Current[iId];
        }

        /// <summary>Determines whether [is valid data object] [the specified obj].</summary>
        /// <param name="obj">The obj.</param>
        /// <returns><c>true</c> if [is valid data object] [the specified obj]; otherwise,<c>false</c> .</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            return Target != null && obj.GetDataPresent(Properties.Resources.NeuronIDFormat);
        }

        #endregion
    }
}