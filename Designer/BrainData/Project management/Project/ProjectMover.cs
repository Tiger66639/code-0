// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectMover.cs" company="">
//   
// </copyright>
// <summary>
//   Moves the project data tot he specified location and makes certain all
//   objects point to the new location. Used for a saveAs
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Moves the project data tot he specified location and makes certain all
    ///     objects point to the new location. Used for a saveAs
    /// </summary>
    internal class ProjectMover : ProjectOperation
    {
        /// <summary>The f to.</summary>
        private string fTo;

        /// <summary>The f to designer file.</summary>
        private string fToDesignerFile;

        /// <summary>Starts <paramref name="to"/> move the poject <paramref name="to"/> the
        ///     specified designer file.</summary>
        /// <param name="to">To.</param>
        internal void Start(string to)
        {
            fToDesignerFile = to;
            fTo = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(to), 
                System.IO.Path.GetFileNameWithoutExtension(to));
            DisableUI();
            System.Action iMovDataTo = MoveDataTo;
            iMovDataTo.BeginInvoke(null, null);
        }

        /// <summary>
        ///     Copies all the project data to new location. The brain data is also
        ///     moved, keeping the same sub dir name for the brain data.
        /// </summary>
        /// <param name="newLoc">The new loc.</param>
        private void MoveDataTo()
        {
            try
            {
                MoveData(fTo, fToDesignerFile);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(EndOk));
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "ProjectMover.MoveDataTo", 
                    string.Format("Failed to move project data to '{0}': {1}.", fTo, e));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End)); // call the end when the designer data has been loaded 
            }
        }
    }
}