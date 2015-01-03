// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericsImporter.cs" company="">
//   
// </copyright>
// <summary>
//   provides support for importing generic data by using topics to parse and
//   process the data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     provides support for importing generic data by using topics to parse and
    ///     process the data.
    /// </summary>
    public class GenericsImporter
    {
        /// <summary>
        ///     starts an import session.
        /// </summary>
        public static void Import()
        {
            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Filter = "All files (*.*)|*.*";
            iDlg.Multiselect = true;

            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                DlgSelectTopics(iDlg.FileNames);
            }
        }

        /// <summary>shows a dialog for selecting the topics and starts the process.</summary>
        /// <param name="files"></param>
        private static void DlgSelectTopics(string[] files)
        {
            var iDlg = new Dialogs.DlgSelectTopics();
            var iDlgRes = iDlg.ShowDialog();
            if (iDlgRes.HasValue && iDlgRes.Value)
            {
                var iTopics = new System.Collections.Generic.List<Neuron>();
                foreach (var i in iDlg.SelectedItems)
                {
                    if (i.IsParsed == false)
                    {
                        var iRes =
                            System.Windows.MessageBox.Show(
                                string.Format("{0} is not completely parsed, perform this now?", i.Name), 
                                "Generic import", 
                                System.Windows.MessageBoxButton.YesNoCancel, 
                                System.Windows.MessageBoxImage.Question);
                        if (iRes == System.Windows.MessageBoxResult.Cancel)
                        {
                            return;
                        }

                        if (iRes == System.Windows.MessageBoxResult.Yes)
                        {
                            i.IsParsed = true;
                        }
                    }

                    iTopics.Add(i.Item);
                }

                var iChan =
                    (from u in BrainData.Current.CommChannels where u is ChatBotChannel select (ChatBotChannel)u)
                        .FirstOrDefault();
                if (iChan != null)
                {
                    foreach (var iFile in files)
                    {
                        var iContent = System.IO.File.ReadAllText(iFile);
                        iChan.SendTextToSin(iContent, iTopics);
                    }
                }
            }
        }
    }
}