// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleManager.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for ModuleManager.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Interaction logic for ModuleManager.xaml
    /// </summary>
    public partial class ModuleManager : System.Windows.Window
    {
        /// <summary>The f selected.</summary>
        private System.Windows.Controls.TreeViewItem fSelected;

        /// <summary>The f items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<ModuleWrapper> fItems;

        /// <summary>Initializes a new instance of the <see cref="ModuleManager"/> class.</summary>
        public ModuleManager()
        {
            fItems = new System.Collections.ObjectModel.ObservableCollection<ModuleWrapper>();
            for (var i = 0; i < Brain.Current.Modules.Items.Count; i++)
            {
                fItems.Add(new ModuleWrapper(Brain.Current.Modules.Items[i], i));

                    // create a wrapper for the items so that they can be observed by the ui.
            }

            InitializeComponent();
            TrvModules.ItemsSource = fItems;
        }

        /// <summary>The btn delete_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TrvModules.SelectedItem is ModuleExtensionWrapper)
            {
                var iExt = TrvModules.SelectedItem as ModuleExtensionWrapper;
                System.Diagnostics.Debug.Assert(iExt != null);
                DeleteExension(iExt);
            }
            else
            {
                var iMod = TrvModules.SelectedItem as ModuleWrapper;
                System.Diagnostics.Debug.Assert(iMod != null);
                var iDeleter = new ModuleDeleter();
                iDeleter.EndedOk += ContinueAfterDelete;
                iDeleter.Tag = iMod;
                iDeleter.Delete(iMod.Module);
            }
        }

        /// <summary>The continue after delete.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ContinueAfterDelete(object sender, System.EventArgs e)
        {
            var iDeleter = (ModuleDeleter)sender;
            var iMod = (ModuleWrapper)iDeleter.Tag;
            fItems.Remove(iMod);
        }

        /// <summary>The btn import_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnImport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iLoader = new ModuleImporter();
            iLoader.EndedOk += iLoader_EndedOk;
            iLoader.Import();
        }

        /// <summary>The i loader_ ended ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iLoader_EndedOk(object sender, System.EventArgs e)
        {
            var iLast = Enumerable.Last(Brain.Current.Modules.Items);
            if (iLast != null)
            {
                var iWrapper = new ModuleWrapper(iLast, Brain.Current.Modules.Items.Count - 1);
                fItems.Add(iWrapper);
            }
        }

        /// <summary>The delete exension.</summary>
        /// <param name="item">The item.</param>
        private void DeleteExension(ModuleExtensionWrapper item)
        {
            item.Owner.Module.ExtensionFiles.Remove(item.Extension);
            var iReloader = new ModuleImporter();
            iReloader.Tag = item;
            iReloader.EndedNok += iReloader_EndedNok;
            iReloader.EndedOk += iReloader_EndedOk;
            iReloader.Import(item.Owner.Module);
        }

        /// <summary>The i reloader_ ended ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iReloader_EndedOk(object sender, System.EventArgs e)
        {
            var iImporter = (ModuleImporter)sender;
            var iExt = (ModuleExtensionWrapper)iImporter.Tag;
            iExt.Owner.Extension.Remove(iExt); // when the operation succeeded, remove the item from the ui.
        }

        /// <summary>The i reloader_ ended nok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iReloader_EndedNok(object sender, System.EventArgs e)
        {
            var iImporter = (ModuleImporter)sender;
            var iExt = (ModuleExtensionWrapper)iImporter.Tag;
            iExt.Owner.Module.ExtensionFiles.Add(iExt.Extension);

                // when the operation failed, we need to make certain that the item is registered again in the server.
        }

        /// <summary>Handles the Selected event of the TrvModules control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TrvModules_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            fSelected = e.OriginalSource as System.Windows.Controls.TreeViewItem;
        }

        /// <summary>The upgrade_executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Upgrade_executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMod = TrvModules.SelectedItem as ModuleWrapper;
            System.Diagnostics.Debug.Assert(iMod != null);
            var iUpdate = new ModuleUpdater();
            iUpdate.Update(iMod.Module);
        }

        /// <summary>The upgrade_ can excute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Upgrade_CanExcute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TrvModules != null && TrvModules.SelectedItem is ModuleWrapper;
        }

        /// <summary>The merge_executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Merge_executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMod = TrvModules.SelectedItem as ModuleWrapper;
            System.Diagnostics.Debug.Assert(iMod != null);
            var iMerger = new ModuleMerger();
            iMerger.Tag = iMod;
            iMerger.EndedOk += Merge_EndedOk;
            iMerger.Merge(iMod.Module);
        }

        /// <summary>The merge_ ended ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Merge_EndedOk(object sender, System.EventArgs e)
        {
            var iMerger = (ModuleMerger)sender;
            var iMod = (ModuleWrapper)iMerger.Tag;
            fItems.Remove(iMod);
        }
    }
}