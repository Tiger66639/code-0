// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DescriptionsImporter.cs" company="">
//   
// </copyright>
// <summary>
//   A project operation that converts/imports old xml description files to
//   the binary, inline format. This was used to convert from 0.3 to 0.4 (if
//   anyone reads this, you'll probably be thinking, bugger, that's old). The
//   filename is the id of the neruon.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A project operation that converts/imports old xml description files to
    ///     the binary, inline format. This was used to convert from 0.3 to 0.4 (if
    ///     anyone reads this, you'll probably be thinking, bugger, that's old). The
    ///     filename is the id of the neruon.
    /// </summary>
    internal class DescriptionsImporter : ProjectOperation
    {
        /// <summary>
        ///     Gets or sets the filter and path where the xml description files are
        ///     located.
        /// </summary>
        /// <value>
        ///     The filter.
        /// </value>
        public string[] ImportFrom { get; set; }

        /// <summary>Gets or sets a value indicating whether delete dir.</summary>
        public bool DeleteDir { get; set; }

        /// <summary>Gets or sets the data path.</summary>
        public string DataPath { get; set; }

        /// <summary>
        ///     Converts the storage system from the brain into the new type as
        ///     defined in <see cref="ProjectConverter.NewStore" /> .
        /// </summary>
        public void Import()
        {
            DisableUI();
            System.Action iConv = InternalImport;
            iConv.BeginInvoke(null, null);
        }

        /// <summary>The internal import.</summary>
        private void InternalImport()
        {
            try
            {
                foreach (var iFile in ImportFrom)
                {
                    ulong iId;
                    if (ulong.TryParse(System.IO.Path.GetFileNameWithoutExtension(iFile), out iId))
                    {
                        // there could be other types of xml files stored there, not likely, but still
                        var iContent = System.IO.File.ReadAllText(iFile);
                        var iData = BrainData.Current.NeuronInfo[iId];
                        iData.DescriptionText = iContent;
                    }
                }

                System.Diagnostics.Debug.Assert(DataPath != null);
                try
                {
                    System.IO.Directory.Delete(DataPath, true);

                        // this needs to go. also delete subs, otherwise, if there are files in there, the dir doesn't go away.
                }
                catch (System.Exception ex)
                {
                    LogService.Log.LogWarning(
                        "ProjectConverter", 
                        string.Format("Failed to delete path {0},with error: {1}", DataPath, ex));
                }

                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(EndOk));
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ProjectConverter.Convert", e.ToString());
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End));
            }
        }
    }
}