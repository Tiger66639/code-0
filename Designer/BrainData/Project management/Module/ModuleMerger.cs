// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleMerger.cs" company="">
//   
// </copyright>
// <summary>
//   A project operation that merges a module into the project so that it is
//   no longer seen as an import, but part of the core network.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A project operation that merges a module into the project so that it is
    ///     no longer seen as an import, but part of the core network.
    /// </summary>
    internal class ModuleMerger : ProjectOperation
    {
        /// <summary>The f to merge.</summary>
        private Module fToMerge;

        /// <summary>
        ///     a general purpose field to store some data that needs to be passed to
        ///     the event handler
        /// </summary>
        public object Tag { get; set; }

        /// <summary>The merge.</summary>
        /// <param name="toMerge">The to merge.</param>
        public void Merge(Module toMerge)
        {
            fToMerge = toMerge;
            DisableUI();
            System.Action iMerger = InternalMerge;
            iMerger.BeginInvoke(null, null);
        }

        /// <summary>The internal merge.</summary>
        private void InternalMerge()
        {
            try
            {
                Brain.Current.Modules.Release(fToMerge);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ModuleMerger", string.Format("Merge failed with message: ", e));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(End));
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            LogService.Log.LogInfo("ModuleImporter.Import", string.Format("Module '{0}' merged.", fToMerge.FileName));
        }
    }
}