// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   provides drop functionality to the children list of a
//   <see cref="NeuronExplorer" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides drop functionality to the children list of a
    ///     <see cref="NeuronExplorer" /> .
    /// </summary>
    public class ExplorerDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>Initializes a new instance of the <see cref="ExplorerDropAdvisor"/> class.</summary>
        public ExplorerDropAdvisor()
        {
            SupportedFormat = Properties.Resources.NeuronIDFormat;
        }

        /// <summary>The on drop completed.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint)
        {
            var iExplorer = ((System.Windows.FrameworkElement)TargetUI).DataContext as NeuronExplorer;

            if (iExplorer.Children != null)
            {
                // we can only add if it is a cluster
                var iId = (ulong)obj.Data.GetData(Properties.Resources.NeuronIDFormat);
                var iNew = new NeuronExplorerItem(Brain.Current[iId]);
                iExplorer.Children.Add(iNew);
            }
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            var iRes = base.IsValidDataObject(obj);
            if (iRes)
            {
                var iExplorer = ((System.Windows.FrameworkElement)TargetUI).DataContext as NeuronExplorer;
                if (iExplorer.Children != null)
                {
                    // could be that it is not a neuron explorer item.
                    return true;
                }

                return false;
            }

            return iRes;
        }
    }
}