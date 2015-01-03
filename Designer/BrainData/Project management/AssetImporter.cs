// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetImporter.cs" company="">
//   
// </copyright>
// <summary>
//   imports asset files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     imports asset files.
    /// </summary>
    internal class AssetImporter : ProjectStreamingOperation
    {
        /// <summary>The f imported.</summary>
        private readonly System.Collections.Generic.List<AssetEditor> fImported =
            new System.Collections.Generic.List<AssetEditor>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssetImporter" /> class.
        /// </summary>
        public AssetImporter()
            : base("Asset Import")
        {
        }

        /// <summary>The import.</summary>
        internal void Import()
        {
            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Filter = Properties.Resources.AssetFileFilter;
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
            var iStillToResolve = new System.Collections.Generic.List<ToResolve>();
            var iErrors = new System.Collections.Generic.List<ParseErrors>();
            var fReadAssets = new System.Collections.Generic.Dictionary<string, Neuron>();

                // stores references to the assets that have already been read (including subs, so contains more then only the assets that represent the file roots, but also possible child asset groups.
            Parsers.ParserBase.BlockLogErrors = true; // so we only report the errors at the end when we want it.
            try
            {
                foreach (var iFile in fFiles)
                {
                    try
                    {
                        var iErr = new ParseErrors();
                        iErr.FileName = iFile;
                        iErrors.Add(iErr);
                        var iEditor =
                            AssetXmlStreamer.Import(iFile, iErr.Errors, iStillToResolve, fReadAssets) as AssetEditor;
                        fImported.Add(iEditor);
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError(
                            "Asset Import", 
                            string.Format(
                                "An error occured while trying to imort the asset-file '{0}', with the message: {1}", 
                                iFile, 
                                e.Message));
                        fProblems = true;
                    }
                }
            }
            finally
            {
                // BaseXmlStreamer.Placesholders = null;                   
                BaseXmlStreamer.AlreadyRendered = null; // need to make certain that there are no more buffers left.
                BaseXmlStreamer.AlreadyRead = null;
                Parsers.ParserBase.BlockLogErrors = false;
            }

            ResolveProblems(iErrors, iStillToResolve);
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(InternalImportUI));

            // App.Current.Dispatcher.BeginInvoke(new Action(EndOk));
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
            System.Collections.Generic.IList<EditorBase> iAddTo;
            if (BrainData.Current.CurrentEditorsList != null)
            {
                iAddTo = BrainData.Current.CurrentEditorsList;
            }
            else
            {
                iAddTo = BrainData.Current.Editors; // add to the root if there is no other option.
            }

            foreach (var i in fImported)
            {
                if (i != null)
                {
                    iAddTo.Add(i);
                    WindowMain.Current.AddItemToOpenDocuments(i);
                }
            }

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