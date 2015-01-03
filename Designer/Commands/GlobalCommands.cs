// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalCommands.cs" company="">
//   
// </copyright>
// <summary>
//   Defines all the application wide commands, that are available everywhere.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Defines all the application wide commands, that are available everywhere.
    /// </summary>
    /// <remarks>
    ///     Some dialogs need access to these while not to others?
    /// </remarks>
    public class GlobalCommands
    {
        /// <summary>Initializes static members of the <see cref="GlobalCommands"/> class.</summary>
        static GlobalCommands()
        {
            BrowseNeuronCmd = new System.Windows.Input.RoutedUICommand(
                "Browse neuron (F8)", 
                "Browse", 
                typeof(GlobalCommands));
            BrowseNeuronCmd.InputGestures.Add(new System.Windows.Input.KeyGesture(System.Windows.Input.Key.F8));

            ViewCodeCmd = new System.Windows.Input.RoutedUICommand("View code...", "ViewCode", typeof(GlobalCommands));
            ViewCodeCmd.InputGestures.Add(
                new System.Windows.Input.KeyGesture(
                    System.Windows.Input.Key.F4, 
                    System.Windows.Input.ModifierKeys.Shift));

            ViewAttachedAssetsCmd = new System.Windows.Input.RoutedUICommand(
                "View asset...", 
                "ViewAttachedAssets", 
                typeof(GlobalCommands));
            ViewAttachedAssetsCmd.InputGestures.Add(
                new System.Windows.Input.KeyGesture(
                    System.Windows.Input.Key.F5, 
                    System.Windows.Input.ModifierKeys.Shift));

            ViewAttachedFramessCmd = new System.Windows.Input.RoutedUICommand(
                "View attached frames...", 
                "ViewAttached", 
                typeof(GlobalCommands));
            ViewAttachedFramessCmd.InputGestures.Add(
                new System.Windows.Input.KeyGesture(
                    System.Windows.Input.Key.F6, 
                    System.Windows.Input.ModifierKeys.Shift));

            ViewAttachedPatternsCmd = new System.Windows.Input.RoutedUICommand(
                "View attached patterns...", 
                "ViewAttached", 
                typeof(GlobalCommands));
            ViewAttachedPatternsCmd.InputGestures.Add(
                new System.Windows.Input.KeyGesture(
                    System.Windows.Input.Key.F7, 
                    System.Windows.Input.ModifierKeys.Shift));

            SearchInCodeCmd = new System.Windows.Input.RoutedUICommand(
                "Search in code", 
                "SearchInCode", 
                typeof(GlobalCommands));
            SearchInCodeCmd.InputGestures.Add(
                new System.Windows.Input.KeyGesture(
                    System.Windows.Input.Key.F3, 
                    System.Windows.Input.ModifierKeys.Control));

            App.SyncCmd = new System.Windows.Input.RoutedUICommand("Sync with explorer", "Sync", typeof(App));
            App.SyncCmd.InputGestures.Add(new System.Windows.Input.KeyGesture(System.Windows.Input.Key.F4));

            ExportCmd = new System.Windows.Input.RoutedUICommand("Export", "ExportCmd", typeof(GlobalCommands));
            ImportCmd = new System.Windows.Input.RoutedUICommand("Import", "ImportCmd", typeof(GlobalCommands));

            ExportWordListCmd = new System.Windows.Input.RoutedUICommand(
                "ExportWordList", 
                "ExportWordListCmd", 
                typeof(GlobalCommands));
            ImportWordListCmd = new System.Windows.Input.RoutedUICommand(
                "ImportWordList", 
                "ImportWordListCmd", 
                typeof(GlobalCommands));
            ImportClipboardWordListCmd = new System.Windows.Input.RoutedUICommand(
                "ImportClipboardWordList", 
                "ImportClipboardWordListCmd", 
                typeof(GlobalCommands));
        }

        /// <summary>Registers the specified element so that it can use the commands
        ///     defined in this class.</summary>
        /// <param name="toRegister">To register.</param>
        public static void Register(System.Windows.UIElement toRegister)
        {
            var iNew = new System.Windows.Input.CommandBinding(
                BrowseNeuronCmd, 
                BrowseNeuronCmd_Executed, 
                BrowseNeuronCmd_CanExecute);
            toRegister.CommandBindings.Add(iNew);
            RegisterInputGestures(BrowseNeuronCmd, toRegister);

            iNew = new System.Windows.Input.CommandBinding(ViewCodeCmd, ViewCode_Executed, ViewCode_CanExecute);
            toRegister.CommandBindings.Add(iNew);
            RegisterInputGestures(ViewCodeCmd, toRegister);

            iNew = new System.Windows.Input.CommandBinding(
                ViewAttachedAssetsCmd, 
                ViewAssets_Executed, 
                ViewAssets_CanExecute);
            toRegister.CommandBindings.Add(iNew);
            RegisterInputGestures(ViewAttachedAssetsCmd, toRegister);

            iNew = new System.Windows.Input.CommandBinding(
                ViewAttachedFramessCmd, 
                ViewFrames_Executed, 
                ViewAssets_CanExecute);
            toRegister.CommandBindings.Add(iNew);
            RegisterInputGestures(ViewAttachedFramessCmd, toRegister);

            iNew = new System.Windows.Input.CommandBinding(
                ViewAttachedPatternsCmd, 
                ViewPatterns_Executed, 
                ViewAssets_CanExecute);
            toRegister.CommandBindings.Add(iNew);
            RegisterInputGestures(ViewAttachedPatternsCmd, toRegister);

            iNew = new System.Windows.Input.CommandBinding(SearchInCodeCmd, SearchInCode_Executed, ViewCode_CanExecute);
            toRegister.CommandBindings.Add(iNew);
            RegisterInputGestures(SearchInCodeCmd, toRegister);

            iNew = new System.Windows.Input.CommandBinding(App.SyncCmd, Sync_Executed, ViewCode_CanExecute);
            toRegister.CommandBindings.Add(iNew);
            RegisterInputGestures(App.SyncCmd, toRegister);

            iNew = new System.Windows.Input.CommandBinding(ExportCmd, Export_Executed, Export_CanExecute);
            toRegister.CommandBindings.Add(iNew);
            RegisterInputGestures(ExportCmd, toRegister);

            if (toRegister != WindowMain.Current)
            {
                // a bit of a shortcut: we can reuse the commandbindings that were declared on the main window, so I don't need to relocate the code
                toRegister.CommandBindings.Add(WindowMain.Current.CmdBindImportTopic);
                toRegister.CommandBindings.Add(WindowMain.Current.CmdBindExportTopic);
                toRegister.CommandBindings.Add(WindowMain.Current.CmdBindExportAllTopics);
                toRegister.CommandBindings.Add(WindowMain.Current.CmdBindImportGlobalPatterns);
                toRegister.CommandBindings.Add(WindowMain.Current.CmdBindExportGlobalPatterns);
                toRegister.CommandBindings.Add(WindowMain.Current.cmdBindProperties);
                toRegister.CommandBindings.Add(WindowMain.Current.CmdBindRebuildProject);
                toRegister.CommandBindings.Add(WindowMain.Current.CmdBindRebuilAllTopics);
            }

            // <CommandBinding Command="self:App.SyncCmd"  CanExecute="ViewCode_CanExecute" Executed="Sync_Executed"/>
        }

        /// <summary>Registers all the input gestures that are defined in a command, with
        ///     the ui element so it can use them.</summary>
        /// <param name="command">The command.</param>
        /// <param name="toRegister">To register.</param>
        private static void RegisterInputGestures(
            System.Windows.Input.RoutedUICommand command, 
            System.Windows.UIElement toRegister)
        {
            foreach (System.Windows.Input.InputGesture gesture in command.InputGestures)
            {
                var iBind = new System.Windows.Input.InputBinding(command, gesture);
                toRegister.InputBindings.Add(iBind);
            }
        }

        #region Attached frames

        /// <summary>The view frames_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private static void ViewFrames_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iNeuron = (Neuron)e.Parameter;
            if (iNeuron == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                {
                    iNeuron = ((INeuronWrapper)iFocused.DataContext).Item;
                }
            }

            System.Diagnostics.Debug.Assert(iNeuron != null);
            WindowMain.Current.ViewFramesForNeuron(iNeuron);
        }

        #endregion

        #region attached patterns

        /// <summary>The view patterns_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private static void ViewPatterns_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iNeuron = (Neuron)e.Parameter;
            if (iNeuron == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                {
                    iNeuron = ((INeuronWrapper)iFocused.DataContext).Item;
                }
            }

            System.Diagnostics.Debug.Assert(iNeuron != null);
            WindowMain.Current.ViewPatternsForNeuron(iNeuron);
        }

        #endregion

        #region SEarch in code

        /// <summary>Handles the Executed event of the SearchInCode command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private static void SearchInCode_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iNeuron = (Neuron)e.Parameter;
            if (iNeuron == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                {
                    iNeuron = ((INeuronWrapper)iFocused.DataContext).Item;
                }
            }

            System.Diagnostics.Debug.Assert(iNeuron != null);
            var iNew = new Search.CodeEditorSearcher(iNeuron);
            iNew.DisplayResults();
        }

        #endregion

        #region Sync

        /// <summary>Handles the Executed event of the Sync control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private static void Sync_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iNeuron = (Neuron)e.Parameter;
            if (iNeuron == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                {
                    iNeuron = ((INeuronWrapper)iFocused.DataContext).Item;
                }
            }

            WindowMain.Current.SyncExplorerToNeuron(iNeuron);
        }

        #endregion

        #region commands

        /// <summary>
        ///     Command to view a neuron as a debugneuron so it's contents can be
        ///     browsed.
        /// </summary>
        public static System.Windows.Input.RoutedUICommand BrowseNeuronCmd;

        /// <summary>
        ///     Command to view the code of the selected neuron.
        /// </summary>
        public static System.Windows.Input.RoutedUICommand ViewCodeCmd;

        /// <summary>
        ///     Command to view the assets attached to the selected neuron.
        /// </summary>
        public static System.Windows.Input.RoutedUICommand ViewAttachedAssetsCmd;

        /// <summary>
        ///     Command to view the frames attached to the selected neuron.
        /// </summary>
        public static System.Windows.Input.RoutedUICommand ViewAttachedFramessCmd;

        /// <summary>
        ///     Command to view the frames attached to the selected neuron.
        /// </summary>
        public static System.Windows.Input.RoutedUICommand ViewAttachedPatternsCmd;

        /// <summary>
        ///     Command to search for the selected neuron where it is used in 'code'
        ///     for display in a code editor
        /// </summary>
        public static System.Windows.Input.RoutedUICommand SearchInCodeCmd;

        /// <summary>
        ///     Command to perform a general export. The type of export depends on the
        ///     type of object being exported. Not all can be exported.
        /// </summary>
        public static System.Windows.Input.RoutedUICommand ExportCmd;

        /// <summary>
        ///     Command to perform a general import. The type of export depends on the
        ///     type of object being exported. Not all can be exported.
        /// </summary>
        public static System.Windows.Input.RoutedUICommand ImportCmd;

        /// <summary>
        ///     Command to initiate an import from the verbNet database.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportWordListCmd;

        /// <summary>
        ///     Command to initiate an import from the verbNet database.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportClipboardWordListCmd;

        /// <summary>
        ///     Command to initiate an export of a list of words.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ExportWordListCmd;

        #endregion

        #region BrowseNeuron

        /// <summary>Handles the CanExecute event of the <see cref="BrowseNeuronCmd"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private static void BrowseNeuronCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null)
            {
                e.CanExecute = e.Parameter is Neuron;
            }
            else
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null)
                {
                    var iWrapper = iFocused.DataContext as INeuronWrapper;
                    e.CanExecute = iWrapper != null && iWrapper.Item is Neuron;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
        }

        /// <summary>Handles the Executed event of the <see cref="BrowseNeuronCmd"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private static void BrowseNeuronCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Neuron iNeuron = null;
            if (e.Parameter != null)
            {
                iNeuron = (Neuron)e.Parameter;
            }
            else
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                var iWrapper = iFocused.DataContext as INeuronWrapper;
                iNeuron = iWrapper.Item;
            }

            if (iNeuron != null)
            {
                var iItems = new System.Collections.Generic.List<DebugNeuron>();
                iItems.Add(new DebugNeuron(iNeuron));
                var iRes = new DlgInspectExpression(null, iItems);
                iRes.Title = "Browse neuron";
                iRes.Owner = System.Windows.Application.Current.MainWindow;
                iRes.Show();
            }
        }

        #endregion

        #region ViewCode

        /// <summary>Shows the code editor for the specified <see cref="Neuron"/> . If
        ///     there was already an editor opened for this item, it is made active.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private static void ViewCode_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iNeuron = (Neuron)e.Parameter;
            if (iNeuron == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                {
                    iNeuron = ((INeuronWrapper)iFocused.DataContext).Item;
                }
            }

            System.Diagnostics.Debug.Assert(iNeuron != null);
            WindowMain.Current.ViewCodeForNeuron(iNeuron);
        }

        /// <summary>Handles the CanExecute event of the ViewCode control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private static void ViewCode_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            e.CanExecute = e.Parameter is Neuron || (iFocused != null && iFocused.DataContext is INeuronWrapper);
        }

        #endregion

        #region Attached assets

        /// <summary>Shows the code editor for the specified <see cref="Neuron"/> . If
        ///     there was already an editor opened for this item, it is made active.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private static void ViewAssets_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iNeuron = (Neuron)e.Parameter;
            if (iNeuron == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                {
                    iNeuron = ((INeuronWrapper)iFocused.DataContext).Item;
                }
            }

            System.Diagnostics.Debug.Assert(iNeuron != null);
            WindowMain.Current.ViewAssetsForNeuron(iNeuron);
        }

        /// <summary>Handles the CanExecute event of the ViewCode control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private static void ViewAssets_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            if (iFocused == null || !(iFocused.DataContext is INeuronWrapper))
            {
                iFocused =
                    System.Windows.Input.FocusManager.GetFocusedElement(
                        System.Windows.Input.FocusManager.GetFocusScope((System.Windows.DependencyObject)sender)) as
                    System.Windows.FrameworkElement;
            }

            e.CanExecute = e.Parameter is Neuron || (iFocused != null && iFocused.DataContext is INeuronWrapper);
        }

        #endregion

        #region Export

        /// <summary>Handles the CanExecute event of the Export control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private static void Export_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is TextPatternEditor || e.Parameter is AssetEditor || e.Parameter is CodeEditor
                            || e.Parameter is QueryEditor;
        }

        /// <summary>Handles the Executed event of the Export control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private static void Export_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is TextPatternEditor)
            {
                var iExporter = new TopicExporter();
                iExporter.Export((TextPatternEditor)e.Parameter);
            }
            else if (e.Parameter is CodeEditor)
            {
                var iExporter = new ModuleExporter();
                iExporter.Export((CodeEditor)e.Parameter);
            }
            else if (e.Parameter is QueryEditor)
            {
                var iExporter = new QueryExporter();
                iExporter.Export((QueryEditor)e.Parameter);
            }
            else
            {
                var iExporter = new AssetExporter();
                iExporter.Export((AssetEditor)e.Parameter);
            }
        }

        #endregion
    }
}