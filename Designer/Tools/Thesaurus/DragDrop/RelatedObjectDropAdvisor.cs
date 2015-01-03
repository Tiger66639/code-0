// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedObjectDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The related object drop advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The related object drop advisor.</summary>
    public class RelatedObjectDropAdvisor : DnD.DropTargetBase
    {
        #region prop

        #region Target

        /// <summary>Gets the target.</summary>
        public ThesaurusLinkedItem Target
        {
            get
            {
                return ((System.Windows.FrameworkElement)TargetUI).DataContext as ThesaurusLinkedItem;
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
                return true;
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
            var iTarget = Target;
            System.Diagnostics.Debug.Assert(iTarget != null);
            var iId = (ulong)arg.Data.GetData(Properties.Resources.NeuronIDFormat);
            var iDropped = Brain.Current[iId];
            if (iDropped is TextNeuron)
            {
                iDropped = EditorsHelper.CreateObject((TextNeuron)iDropped);
            }

            if (iTarget.Related != null)
            {
                var iRelated = iTarget.Related as NeuronCluster;
                iTarget.Related = null; // remove the link so we can check if the related object is still valid
                if (iRelated != null && BrainHelper.HasReferences(iRelated) == false)
                {
                    // normally, the related data is an object that get created and managed from the Thesaurus item, so if it no longe has any refs, remove.
                    EditorsHelper.DeleteObject(iRelated);
                }
            }

            iTarget.Related = iDropped;
        }

        /// <summary>Determines whether [is valid data object] [the specified obj]. Allow
        ///     drop when dropped object is a textneuron or an object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns><c>true</c> if [is valid data object] [the specified obj]; otherwise,<c>false</c> .</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            if (Target != null && obj.GetDataPresent(Properties.Resources.NeuronIDFormat))
            {
                var iId = (ulong)obj.GetData(Properties.Resources.NeuronIDFormat);
                var iDropped = Brain.Current[iId];
                return (iDropped is TextNeuron)
                       || (iDropped is NeuronCluster
                           && ((NeuronCluster)iDropped).Meaning == (ulong)PredefinedNeurons.Object);
            }

            return false;
        }

        #endregion
    }
}