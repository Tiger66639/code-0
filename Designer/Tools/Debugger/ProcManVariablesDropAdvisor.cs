// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcManVariablesDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The drop advisor for the <see cref="ProcessorManager" /> 's
//   <see cref="JaStDev.HAB.Designer.ProcessorManager.Watches" /> list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     The drop advisor for the <see cref="ProcessorManager" /> 's
    ///     <see cref="JaStDev.HAB.Designer.ProcessorManager.Watches" /> list.
    /// </summary>
    public class ProcManVariablesDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>Gets a value indicating whether use preview events.</summary>
        public override bool UsePreviewEvents
        {
            get
            {
                return false;
            }
        }

        /// <summary>Called when a drop is performed.</summary>
        /// <remarks>modified from original, changed IDataobject to DragEventArgs so that
        ///     we have more info about the drop, like on who we are dropping.</remarks>
        /// <param name="obj"></param>
        /// <param name="dropPoint"></param>
        public override void OnDropCompleted(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint)
        {
            if ((obj.AllowedEffects & System.Windows.DragDropEffects.Copy) == System.Windows.DragDropEffects.Copy)
            {
                var iId = (ulong)obj.Data.GetData(Properties.Resources.NeuronIDFormat);

                var iFound = (from i in ProcessorManager.Current.Watches where i.ID == iId select i).FirstOrDefault();
                if (iFound == null)
                {
                    var iWatch = new Watch(iId);
                    ProcessorManager.Current.Watches.Add(iWatch);
                    if (ProcessorManager.Current.SelectedProcessor != null
                        && ProcessorManager.Current.Displaymode == ProcessorManager.DisplayMode.Processors)
                    {
                        iWatch.LoadValuesFor(ProcessorManager.Current.SelectedProcessor.Processor);
                    }
                }
            }
        }

        /// <summary>Gets the effect that should be used for the drop operation.</summary>
        /// <remarks>By default, this function checks the control key, wen pressed, a copy
        ///     is done, otherwise a move.</remarks>
        /// <param name="e">The drag event arguments.</param>
        /// <returns>The prefered effect to use.</returns>
        public override System.Windows.DragDropEffects GetEffect(System.Windows.DragEventArgs e)
        {
            var iId = (ulong)e.Data.GetData(Properties.Resources.NeuronIDFormat);

            if (iId == Neuron.EmptyId)
            {
                var iSource = e.Data.GetData(Properties.Resources.DelayLoadResultType) as DnD.DragSourceBase;
                System.Diagnostics.Debug.Assert(iSource != null);
                var iResultType = iSource.GetDelayLoadResultType(e.Data);
                if (iResultType.IsSubclassOf(typeof(Variable)))
                {
                    return System.Windows.DragDropEffects.Copy;
                }
            }
            else
            {
                Neuron iFound;
                if (Brain.Current.TryFindNeuron(iId, out iFound) && iFound is Variable)
                {
                    // we allow the empty id, this indicates that it is a delay load from the toolbox, the type of which has already been checked.
                    return System.Windows.DragDropEffects.Copy;
                }
            }

            return System.Windows.DragDropEffects.None;
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            System.Type iResultType = null;
            if (obj.GetDataPresent(Properties.Resources.DelayLoadResultType))
            {
                var iSource = obj.GetData(Properties.Resources.DelayLoadResultType) as DnD.DragSourceBase;
                System.Diagnostics.Debug.Assert(iSource != null);
                iResultType = iSource.GetDelayLoadResultType(obj);
                return iResultType.IsSubclassOf(typeof(Variable));
            }

            return obj.GetDataPresent(Properties.Resources.NeuronIDFormat);
        }
    }
}