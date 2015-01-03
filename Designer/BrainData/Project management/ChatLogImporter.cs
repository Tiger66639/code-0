// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatLogImporter.cs" company="">
//   
// </copyright>
// <summary>
//   imports a chatlog file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     imports a chatlog file.
    /// </summary>
    internal class ChatLogImporter : ProjectOperation
    {
        /// <summary>The f files.</summary>
        private string[] fFiles; // this list of files that need to be imported.

        /// <summary>The f problems.</summary>
        private bool fProblems; // flag to keep track if there were problems.

        /// <summary>The import.</summary>
        internal void Import()
        {
            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Filter = Properties.Resources.ChatLogFileFilter;
            iDlg.FilterIndex = 1;
            iDlg.Multiselect = true;

            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                fFiles = iDlg.FileNames;
                DisableUI();
                System.Action iImportMod = InternalImport;
                iImportMod.BeginInvoke(null, null);
            }
        }

        /// <summary>
        ///     Imports the topic.
        /// </summary>
        private void InternalImport()
        {
            foreach (var iFile in fFiles)
            {
                try
                {
                    LogHistoryStreamer.Import(iFile);
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError(
                        "Chatlog Import", 
                        string.Format(
                            "An error occured while trying to imort the file '{0}', with the message: {1}", 
                            iFile, 
                            e.Message));
                    fProblems = true;
                }
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(InternalImportUI));
        }

        /// <summary>
        ///     Disables the UI.
        /// </summary>
        protected override void DisableUI()
        {
            base.DisableUI();
            BrainData.Current.LearnCount++; // we indicate that we are learning, cause we are importing data.
        }

        /// <summary>
        ///     Performs the UI section of the import.
        /// </summary>
        private void InternalImportUI()
        {
            if (fProblems)
            {
                End();
                if (fFiles.Length > 1)
                {
                    System.Windows.MessageBox.Show(
                        "There were problems while importing the files. Please check the log for more info.", 
                        "Import", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Warning);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "There were problems while importing the file. Please check the log for more info.", 
                        "Import", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Warning);
                }
            }
            else
            {
                EndOk();
            }
        }

        /// <summary>
        ///     This function is called by End and EndOk. So it provides a location to
        ///     put common code that always needs to be called at the end. When you
        ///     <see langword="override" /> this function, don't forget to call the
        ///     base.s
        /// </summary>
        protected override void PerformOnEnd()
        {
            BrainData.Current.LearnCount--; // also need to disable learning.
            base.PerformOnEnd();
        }
    }
}