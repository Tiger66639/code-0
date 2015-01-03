// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParseMapDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   provides drop support for the 'parsemap' listboxt
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides drop support for the 'parsemap' listboxt
    /// </summary>
    public class ParseMapDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>Initializes a new instance of the <see cref="ParseMapDropAdvisor"/> class.</summary>
        public ParseMapDropAdvisor()
        {
            SupportedFormat = Properties.Resources.NeuronIDFormat;
        }

        /// <summary>The on drop completed.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint)
        {
            var iList = ((System.Windows.Controls.ListBox)TargetUI).ItemsSource as SmallIDCollection;

            if (iList != null)
            {
                // we can only add if it is a cluster
                var iId = (ulong)obj.Data.GetData(Properties.Resources.NeuronIDFormat);
                iList.Add(iId);
            }
        }
    }
}