// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for App.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    /// <remarks>
    ///     Run usage:
    ///     exeName [Path]
    ///     where Path is the file path to open the DESIGNERFILE from.
    /// </remarks>
    public partial class App : System.Windows.Application
    {
        #region Fields

        /// <summary>The f auto save timer.</summary>
        private System.Windows.Threading.DispatcherTimer fAutoSaveTimer;

        /// <summary>The f undo count at last save.</summary>
        private int fUndoCountAtLastSave;

        // bool fIsLoaded = false;                                                             //for fatal errors, we need to know if the app is ful

        /// <summary>The framenetpath.</summary>
        private static readonly string FRAMENETPATH = "NND\\Data\\FrameNet";

                                       // the default installation path of framenet.

        /// <summary>The datatpath.</summary>
        private static readonly string DATATPATH = "NND\\Data"; // the default installation path of other data.

        /// <summary>The verbnetpath.</summary>
        private static readonly string VERBNETPATH = "NND\\Data\\VerbNet"; // the default installation path of VerbNet.

        /// <summary>The defaulttemplateproject.</summary>
        private static readonly string DEFAULTTEMPLATEPROJECT = "NND\\Templates\\Default."
                                                                + ProjectManager.PROJECT_EXTENTION;

                                       // the  installation path of default template.

        // static string CURCHARTPATH = "NND\\Data\\CurrentCharacter";                                 //the default path to extract the current character too.
        /// <summary>The characterstpath.</summary>
        private static readonly string CHARACTERSTPATH = "NND\\Characters";

                                       // the default installation path of all the available characters.

        /// <summary>
        ///     The filename of the Avalondock layoutfile for design mode.
        /// </summary>
        public const string DESIGNLAYOUTFILE = "Design_Layout.xml";

        /// <summary>
        ///     The filename of the Avalondock layoutfile for pro design mode.
        /// </summary>
        public const string PRODESIGNLAYOUTFILE = "DesignPRO_Layout.xml";

        /// <summary>
        ///     The filename of the Avalondock layoutfile for basic design mode.
        /// </summary>
        public const string BASICDESIGNLAYOUTFILE = "DesignBASIC_Layout.xml";

        /// <summary>
        ///     The filename of the Avalondock layoutfile for viewer mode.
        /// </summary>
        public const string VIEWERLAYOUTFILE = "Viewer_Layout.xml";

        /// <summary>
        ///     The filename of the Avalondock layoutfile for sandbox mode.
        /// </summary>
        public const string SANDBOXLAYOUTFILE = "Sandbox_Layout.xml";

        #endregion

        #region Commands

        #region Editing commands

        /// <summary>
        ///     Command to add a link to the selected item, which uses the same meaning and points to the same value as the object
        ///     that triggered the command.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ShareLinkCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to paste the content of the clipboard, with more possibilities for the user.
        /// </summary>
        public static System.Windows.Input.RoutedCommand PasteSpecialCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to delete the selected items, with the user able to select more options, like how neurons should
        ///     be deleted: only removed from the editor, always delete, delete related items,...
        /// </summary>
        public static System.Windows.Input.RoutedCommand DeleteSpecialCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to view the code of the selected neuron.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ViewCodeCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to rename an item.
        /// </summary>
        public static System.Windows.Input.RoutedCommand RenameCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to remove an item from a list, without actually deleting it.
        /// </summary>
        public static System.Windows.Input.RoutedCommand RemoveCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to sync the currently selected neuron with the explorer so that it  is also selected there.
        /// </summary>
        public static System.Windows.Input.RoutedUICommand SyncCmd;

        /// <summary>
        ///     Command to change an item from one type to another.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ChangeToCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to find the next item
        /// </summary>
        public static System.Windows.Input.RoutedCommand FindNextCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to create a new object.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewObjectCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to merge 2 neurons into 1.
        /// </summary>
        public static System.Windows.Input.RoutedCommand MergeNeuronsCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to toggle an expander's Expanded state.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ToggleExpanderCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new item in a list.
        /// </summary>
        public static System.Windows.Input.RoutedCommand InsertItemCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new item in a list.
        /// </summary>
        public static System.Windows.Input.RoutedCommand InsertItemAfterCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region Debug commands

        /// <summary>
        ///     Command to let the debug processor continue to the next step or breakpoint.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ContinueDebugCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to let the debug processor pause at the next step.
        /// </summary>
        public static System.Windows.Input.RoutedCommand PauseDebugCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to let the debug processor go to the next steps.
        /// </summary>
        public static System.Windows.Input.RoutedCommand StepNextDebugCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to stop all the processors that are currently running
        /// </summary>
        public static System.Windows.Input.RoutedCommand StopProcessorsCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to stop a single processor (but leave the rest running.
        /// </summary>
        public static System.Windows.Input.RoutedCommand KillProcessorCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to stop all processors but 1.
        /// </summary>
        public static System.Windows.Input.RoutedCommand KillAllButProcessorCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to copy the current project into a sandbox dir and start it in a new designer.
        /// </summary>
        public static System.Windows.Input.RoutedCommand SandboxDebugCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to get the value of an expression from the currently selected processor.
        /// </summary>
        public static System.Windows.Input.RoutedCommand InspectExpressionCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to view a neuron as a debugneuron so it's contents can be browsed.
        /// </summary>
        public static System.Windows.Input.RoutedCommand BrowseNeuronCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to attach the selected neuron to the currently selected processor, so that any changes
        ///     performed to this neuron, on another processor, will trigger a breakpoint.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AttachToCurProcessorCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to Remove all the breakpoints.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ClearBreakPointsCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to store the split path of a processor into the project so it can be used for later debug sessions.
        /// </summary>
        public static System.Windows.Input.RoutedCommand StoreSplitPathCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to open the currently opened debugger view in a code editor.
        /// </summary>
        public static System.Windows.Input.RoutedCommand OpenDebuggerInEditorCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to let the memprofiler display the start point (in a code editor) of the leak.
        /// </summary>
        public static System.Windows.Input.RoutedCommand MemProfilerGotoStartCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to let the memprofiler display the location (in a code editor) at wich the neuron got unfrozen and caused a
        ///     leak.
        /// </summary>
        public static System.Windows.Input.RoutedCommand MemProfilerGotoUnfreezeCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to let the memprofiler display the end point (in a code editor) of the leak = where the processor died.
        /// </summary>
        public static System.Windows.Input.RoutedCommand MemProfilerGotoEndCmd =
            new System.Windows.Input.RoutedCommand();

        #endregion

        #region TestCases

        /// <summary>
        ///     Command to create a new testcase
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewTestCaseCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a new testcase item
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddTestCaseItemCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a new testcase item
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddChildTestCaseItemCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to run a testcase
        /// </summary>
        public static System.Windows.Input.RoutedCommand RunTestCaseCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to stop running a testcase
        /// </summary>
        public static System.Windows.Input.RoutedCommand StopTestCaseCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region Import/export

        /// <summary>
        ///     Command to initiate an import from the framenet database.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportFrameNetDataCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to initiate an import from the verbNet database.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportVerbNetDataCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to initiate an import of a topic
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportTopicCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to initiate an export of a topic
        /// </summary>
        public static System.Windows.Input.RoutedCommand ExportTopicCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to export all the topics(pattern editors) that are declared in the brain.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ExportAllTopicsCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to export the properties of a chatbot (fallbacks, opening statements,...
        /// </summary>
        public static System.Windows.Input.RoutedCommand ExportglobalPatternsCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to initiate an import of the global patterns like fallbacks,...
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportGlobalPatternsCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to export the selected asset data.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ExportAssetCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to import the selected asset data.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportAssetCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to import the selected asset data.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportGenericDataCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to import the custom data through a  dll.
        /// </summary>
        public static System.Windows.Input.RoutedCommand CustomConduitCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to export the selected asset data.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ExportQueryCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to import the selected asset data.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportQueryCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to export the selected asset data.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ExportChatLogHistCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to import the selected asset data.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportChatLogHistCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to export a library ref to the clipboard.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ExportLibRefToClipboardCmd =
            new System.Windows.Input.RoutedCommand();

        #endregion

        #region moduels

        /// <summary>
        ///     Command to initiate an import from a modules
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportModuleCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to initiate an import from a modules
        /// </summary>
        public static System.Windows.Input.RoutedCommand ExportModuleCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region project management

        /// <summary>
        ///     Command to create a new mindmap.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewMindMapCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to create a new frame editor.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewFrameEditorCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to create a new frame editor.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewVisualEditorCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to create a new frame editor.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewCodeClusterCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to create a new flow.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewFlowEditorCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to create a new flow.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewAssetEditorCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to create a new folder.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewFolderCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to create a new neuron.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewNeuronCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to create a new text pattern editor.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewPatternsEditorCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to create a new text pattern editor.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewQueryEditorCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region Flow commands

        /// <summary>
        ///     Command to insert a new Flow option.
        /// </summary>
        public static System.Windows.Input.RoutedCommand InsertFlowOptionCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new Flow loop.
        /// </summary>
        public static System.Windows.Input.RoutedCommand InsertFlowLoopCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to display a dialog for selecting a static to insert.
        /// </summary>
        public static System.Windows.Input.RoutedCommand InsertFlowStaticCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to display a dialog for a new object to insert in a flow.
        /// </summary>
        public static System.Windows.Input.RoutedCommand InsertFlowNewObjectCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to display a dialog for a new neuron to insert to a flow.
        /// </summary>
        public static System.Windows.Input.RoutedCommand InsertFlowNewNeuronCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to display a dialog for selecting a static to insert.
        /// </summary>
        public static System.Windows.Input.RoutedCommand InsertFlowCondPartCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new Flow option.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFlowOptionCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new Flow loop.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFlowLoopCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to change an option into a loop on a Flow.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ChangeFlowOptionToLoopCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to change an option into a loop on a Flow.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ChangeFlowLoopToOptionCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to change the requirement of an option/loop to have a part selected during parsing.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ToggleFlowLoopSelectionRequirementCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to display a dialog for selecting a static to insert.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFlowStaticCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to display a dialog for a new object to add to a flow.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFlowNewObjectCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to display a dialog for selecting a static to insert.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFlowCondPartCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to display a dialog for a new neuron to add to a flow.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFlowNewNeuronCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a new flow to the currently openeed flow editor.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFlowCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region Asset commands

        /// <summary>
        ///     Command to add a record to the asset cluster.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddAssetRecordCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a record to the asset cluster.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddAssetSubRecordCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a record to the asset cluster.
        /// </summary>
        public static System.Windows.Input.RoutedCommand CreateSubAssetCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add an 'or' cluster as value
        /// </summary>
        public static System.Windows.Input.RoutedCommand CreateOrClusterCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add an 'and' cluster as value
        /// </summary>
        public static System.Windows.Input.RoutedCommand CreateAndClusterCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add an 'and' cluster as value
        /// </summary>
        public static System.Windows.Input.RoutedCommand ChangeToAssetItemValueCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add an 'and' cluster as value
        /// </summary>
        public static System.Windows.Input.RoutedCommand ChangeToAssetItemListCmd =
            new System.Windows.Input.RoutedCommand();

        #endregion

        #region Frames

        /// <summary>
        ///     Command to add a new frame.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFrameCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a frame element to a frame.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFrameElementCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a sequence to a frame.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFrameSequenceCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to move a sequence up in the list
        /// </summary>
        public static System.Windows.Input.RoutedCommand MoveElementUpCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to move a sequence down in the list
        /// </summary>
        public static System.Windows.Input.RoutedCommand MoveElementDownCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add an element to a sequence.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddElementToSequenceCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to remove an element from a sequence.
        /// </summary>
        public static System.Windows.Input.RoutedCommand RemoveElementFromSequenceCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a frame element filter
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFrameElementFilterCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a frame element filter group
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFrameElementFilterGroupCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a frame element filter segment
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFEFilterSegmentCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a frame element custom filter.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFECustomFilterCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a frame element custom filter.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddFEBoolFilterCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region Wordnet commands

        /// <summary>
        ///     Command to import all data for a single word from wordnet into the network.
        /// </summary>
        public static System.Windows.Input.RoutedCommand SearchInWordNetCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to import all data for a single word from wordnet into the network.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportFromWordNetCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to import all data for a all the words in wordnet into the network.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ImportAllFromWordNetCmd =
            new System.Windows.Input.RoutedCommand();

        #endregion

        #region Thesaurus

        /// <summary>
        ///     Command to add a new relationship to the thesuarus view.
        /// </summary>
        public static System.Windows.Input.RoutedCommand NewSynonymCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a new relationship to the thesuarus view.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddThesaurusRelationshipCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a new relationship to the thesuarus view.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddNoRecursiveThesRelCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a new relationship to the thesuarus view.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddConjugationCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a new relationship to the thesuarus view.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddPosRelatedCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a new relationship to the thesuarus view.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddObjectRelatedCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region Help

        /// <summary>
        ///     Command to show an internet explorer for a specified page.
        /// </summary>
        public static System.Windows.Input.RoutedCommand OpenInternetPageCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region Resource manager

        /// <summary>
        ///     Command to add a new resource to a resource manager.
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddResourceCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a new resource to a resource manager.
        /// </summary>
        public static System.Windows.Input.RoutedCommand SendResourceCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to add a new resource to a resource manager.
        /// </summary>
        public static System.Windows.Input.RoutedCommand SendAllResourcesCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region text patterns

        /// <summary>
        ///     Command to toggle an expander's Expanded state.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ToggleDoPatternCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to toggle an expander's Expanded state.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ToggleAllDoPatternCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to toggle to the statements tab.
        /// </summary>
        public static System.Windows.Input.RoutedCommand TogglePatternStatementsCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to toggle to the statements tab.
        /// </summary>
        public static System.Windows.Input.RoutedCommand TogglePatternQuestionsCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to toggle to the topic's filters tab.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ToggleTopicFiltersCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new conditional
        /// </summary>
        public static System.Windows.Input.RoutedCommand InsertConditionalCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new conditional
        /// </summary>
        public static System.Windows.Input.RoutedCommand RebuildProjectCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new conditional
        /// </summary>
        public static System.Windows.Input.RoutedCommand RebuildAllTopicsCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new conditional
        /// </summary>
        public static System.Windows.Input.RoutedCommand RebuildTopicCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new conditional
        /// </summary>
        public static System.Windows.Input.RoutedCommand UnloadTopicsCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to insert a new conditional
        /// </summary>
        public static System.Windows.Input.RoutedCommand ReplaceTopicCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region online

        /// <summary>
        ///     Command to sync by downloading a project
        /// </summary>
        public static System.Windows.Input.RoutedCommand DonwloadCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to sync a project (user selects which parts go where)
        /// </summary>
        public static System.Windows.Input.RoutedCommand SynchronizeCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to sync a project by uploading the project (force new online version)
        /// </summary>
        public static System.Windows.Input.RoutedCommand UploadCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to install the online version.
        /// </summary>
        public static System.Windows.Input.RoutedCommand InstallCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        /// <summary>The install android cmd.</summary>
        public static System.Windows.Input.RoutedCommand InstallAndroidCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>The upload android cmd.</summary>
        public static System.Windows.Input.RoutedCommand UploadAndroidCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region Colors

        /// <summary>The f color list.</summary>
        private System.Collections.Generic.List<System.Windows.Media.Color> fColorList;

        /// <summary>
        ///     Gets the list of available colors.
        /// </summary>
        public System.Collections.Generic.List<System.Windows.Media.Color> ColorList
        {
            get
            {
                if (fColorList == null)
                {
                    fColorList = new System.Collections.Generic.List<System.Windows.Media.Color>();
                    var iType = typeof(System.Windows.Media.Colors);
                    foreach (var i in iType.GetProperties())
                    {
                        // get all the predifined colors out of the class.
                        if (i.PropertyType == typeof(System.Windows.Media.Color))
                        {
                            fColorList.Add((System.Windows.Media.Color)i.GetValue(null, null));
                        }
                    }

                    fColorList.Sort(CompareColor);
                }

                return fColorList;
            }
        }

        /// <summary>Compares 2 colors so they can be properly sorted.</summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The <see cref="int"/>.</returns>
        private int CompareColor(System.Windows.Media.Color a, System.Windows.Media.Color b)
        {
            // return (a.R - b.R) + (a.G - b.G) + (a.G - b.G);
            if (a.R == b.R)
            {
                if (a.G == b.G)
                {
                    return a.B - b.B;
                }

                return a.G - b.G;
            }

            return a.R - b.R;
        }

        #endregion

        #region Functions

        #region app Start/stop

        /// <summary>
        ///     Main entry point.
        /// </summary>
        [System.STAThread]
        private static void Main()
        {
            try
            {
                var iArgs = System.Environment.GetCommandLineArgs();
                var iFound = (from i in iArgs where i == "silent" select i).FirstOrDefault();
                if (iFound == null)
                {
                    System.Windows.SplashScreen iSplash;
#if BASIC 
               iSplash = new SplashScreen("Images/splash/splash blue.jpg");
#elif PRO
                    iSplash = new System.Windows.SplashScreen("Images/splash/splash green.jpg");
#else
               iSplash = new SplashScreen("Images/splash/splash red.jpg");
#endif
                    iSplash.Show(true);
                    var app = new App();
                    app.InitializeComponent();
                    app.Run();
                }
                else
                {
                    SyncData();
                }
            }
            catch
            {
                Current.Shutdown(-1);
            }
        }

        /// <summary>
        ///     Used to perform sync operations
        /// </summary>
        private static void SyncData()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Called when the application starts</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <remarks>Performs the various starup tasks such as loading the default data.
        /// <para>
        ///         possible arguments:
        ///         0: the path to open the data from.</para>
        /// </remarks>
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            CheckSettingsVersion();
            CheckAppAge();
            string iFileName; // in case there is a filename in the Startup args, we extract it from the startupargs.
            CheckStartupArgs(e.Args, out iFileName);
            LogService.Log.Service = WPFLog.WPFLog.Default;

                // we need to attach the wpf log to our generic service in order to get logging to work.
            CheckInitUserSettings();

            System.Windows.Media.Animation.Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(System.Windows.Media.Animation.Timeline), 
                new System.Windows.FrameworkPropertyMetadata { DefaultValue = 30 });

                // make certain that animation doesn't consume to much CPU
            MindMapCluster.AutoAddItemsToMindMapCluter =
                Designer.Properties.Settings.Default.AutoAddItemsToMindMapCluter;

                // we copy this value so that the mindmap is an independent unit.
            SetSettings();
            if (string.IsNullOrEmpty(iFileName))
            {
                ProjectManager.Default.CreateNew();
            }
            else if (System.IO.File.Exists(iFileName))
            {
                ProjectManager.Default.LoadProjectForSandbox(iFileName);

                    // this does a synchronous load, which is ok, cause we are starting the app.
            }
            else
            {
                LogService.Log.LogError("Application.Startup", "Invalid path, can't open  " + iFileName);
            }

            if (BrainData.Current == null)
            {
                // we check if the braindata was correctly loaded, if not, we create a new one and reset the load location.  This will allow the app to function without the loaded data (empty mem) withtout overwriting the bad data.
                ProjectManager.Default.DataError = true;
                try
                {
                    BrainData.New();

                        // we put this in a try/catch cause a create can also fail if the load failed, which we need to take care off.
                    LogService.Log.LogError("App.Application_Startup", "Failed to load BrainData, Created new.");
                }
                catch (System.Exception ex)
                {
                    LogService.Log.LogError(
                        "App.Application_Startup", 
                        string.Format(
                            "Failed to load brainData, tried to create new which also failed with the error: {0}.", 
                            ex));
                }
            }

            UpdateAutoSave();
            DebugProcessor.Init();
            StartupUri = new System.Uri("WindowMain.xaml", System.UriKind.Relative); // so we start the app correctly.
        }

        /// <summary>
        ///     shows an empty transparent window, so the messagebox remains visible.
        /// </summary>
        internal static void ShowEmptyWindow()
        {
            var iTemp = new System.Windows.Window();
            iTemp.ShowInTaskbar = false;
            iTemp.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            iTemp.WindowStyle = System.Windows.WindowStyle.None;
            iTemp.AllowsTransparency = true;
            iTemp.Background = System.Windows.Media.Brushes.Transparent;
            iTemp.Show();
        }

        /// <summary>
        ///     Checks if there are settings from a previous version. If so, these are imported.
        /// </summary>
        private void CheckSettingsVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            if (version.ToString() != Designer.Properties.Settings.Default.SettingsVersion)
            {
                Designer.Properties.Settings.Default.Upgrade();
                Designer.Properties.Settings.Default.SettingsVersion = version.ToString();

                // Designer.Properties.Settings.Default.Save();                                            //don't save, only save on exit.
            }
        }

        /// <summary>
        ///     Checks the age of the app.
        /// </summary>
        private void CheckAppAge()
        {
            // #if !DEBUG && !BASIC
            // DateTime iTime = DateTime.Now;
            // DateTime iEnd = new DateTime(2012, 2, 1);
            // if (iTime >= iEnd)
            // MessageBox.Show("Trial period has expired, please update to a new version.", "demo");
            // #endif
        }

        /// <summary>The set settings.</summary>
        private void SetSettings()
        {
            Settings.SinAssemblies = CreateAssemblies();
            Settings.StorageMode = NeuronStorageMode.StreamWhenPossible;

                // we want to work in a project based fashion, this means no auto store neurons, but only at specific request.
            Settings.LogNeuronNotFoundInLongTermMem = false;
            Settings.TrackNeuronAccess = false;
            Settings.BufferFreeBlocksFiles = true;
            Settings.BufferIndexFiles = true;
            Settings.SetMinMaxProc(
                Designer.Properties.Settings.Default.MinReservedBlockedProcessors, 
                Designer.Properties.Settings.Default.MaxConcurrentProcessors);
            Settings.InitProcessorStackSize = Designer.Properties.Settings.Default.InitProcessorStackSize;
            Settings.DuplicatePatternLogMethod = Designer.Properties.Settings.Default.DuplicatePatternLogMethod;

            if (Designer.Properties.Settings.Default.DesignerTriggersNetworkEvents)
            {
                Settings.RaiseNetworkEvents = true;
            }
            else
            {
                Settings.RaiseNetworkEvents = ProjectManager.Default.IsSandBox || !ProjectManager.Default.IsNotViewer;

                    // during a sandbox op or when in viewer mode, we want to have the events firing cause we are debugging. Whend designing, we generally don't want this.
            }

            ProcessorFactory.Factory = ProcessorManager.Current;
            CharacterEngine.SpeechEngine.EngineMode = Designer.Properties.Settings.Default.SpeechEngineMode;

            // Brain.Current.IsEditMode = true;                                     //allow for better re-use of deleted ids
            Parsers.NNLModuleCompiler.NetworkDict = CompilerRenderDict.Default;

                // create a link between the compilers and the designer's dictionary.
        }

        /// <summary>
        ///     Checks if all the user settings are properly initialized and if not, provides a correct value.
        /// </summary>
        private void CheckInitUserSettings()
        {
            if (string.IsNullOrEmpty(Designer.Properties.Settings.Default.FrameNetPath))
            {
                Designer.Properties.Settings.Default.FrameNetPath =
                    System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                        FRAMENETPATH);
            }

            if (string.IsNullOrEmpty(Designer.Properties.Settings.Default.DefaultTemplatePath))
            {
                Designer.Properties.Settings.Default.DefaultTemplatePath =
                    System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                        DEFAULTTEMPLATEPROJECT);
            }

            if (string.IsNullOrEmpty(Designer.Properties.Settings.Default.VerbNetPath))
            {
                Designer.Properties.Settings.Default.VerbNetPath =
                    System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                        VERBNETPATH);
            }

            if (string.IsNullOrEmpty(Designer.Properties.Settings.Default.RegExFile))
            {
                var iTemp =
                    System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                        DATATPATH);
                Designer.Properties.Settings.Default.RegExFile = System.IO.Path.Combine(iTemp, "RegexMorphs.xml");
            }

            // if(string.IsNullOrEmpty(Designer.Properties.Settings.Default.CharacterExtractionPath) == true)
            // Designer.Properties.Settings.Default.CharacterExtractionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), CURCHARTPATH);
            if (string.IsNullOrEmpty(Designer.Properties.Settings.Default.CharactersPath))
            {
                Designer.Properties.Settings.Default.CharactersPath =
                    System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                        CHARACTERSTPATH);
            }

            if (string.IsNullOrEmpty(Designer.Properties.Settings.Default.CustomSpellingDictsPath))
            {
                var iTemp =
                    System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                        DATATPATH);
                Designer.Properties.Settings.Default.CustomSpellingDictsPath = iTemp;
            }
            else if (System.IO.Directory.Exists(Designer.Properties.Settings.Default.CustomSpellingDictsPath))
            {
                System.IO.File.Create(
                    System.IO.Path.Combine(
                        Designer.Properties.Settings.Default.CustomSpellingDictsPath, 
                        Designer.Properties.Resources.IgnoreAllDict));

                    // make certain that the IgnoreAllDict list is empty each time we restart the application
                var iSpellDictPath = System.IO.Path.Combine(
                    Designer.Properties.Settings.Default.CustomSpellingDictsPath, 
                    Designer.Properties.Resources.CustomSpellingDict);
                if (System.IO.File.Exists(iSpellDictPath) == false)
                {
                    // also need to make certain that there is a custom dict, otherwise, we can't load the controls properly.
                    System.IO.File.Create(iSpellDictPath);
                }
            }

            if (Designer.Properties.Settings.Default.FirstRun)
            {
                Designer.Properties.Settings.Default.FirstRun = false;
                if (System.Environment.OSVersion.Platform == System.PlatformID.Win32Windows
                    && System.Environment.OSVersion.Version.Major == 5)
                {
                    // it's xp, so make certain that we are using hte managed speech system cause the unmanaged doens't work on xp.
                    Designer.Properties.Settings.Default.SpeechEngineMode = CharacterEngine.SpeechEngineMode.Managed;
                }
            }
        }

        /// <summary>Checks if the application is currently the sandbox for another project.</summary>
        /// <param name="p">The p.</param>
        /// <param name="file">The file.</param>
        private void CheckStartupArgs(string[] p, out string file)
        {
            ProjectManager.Default.IsSandBox = false;
            ProjectManager.Default.IsViewerVisibility = System.Windows.Visibility.Visible;
            file = null;

            // string iFound = (from i in p where i.ToLower() == "sandbox" select i).FirstOrDefault();
            foreach (var i in p)
            {
                var iLower = i.ToLower();
                if (iLower == "sandbox")
                {
                    ProjectManager.Default.IsSandBox = true;
                }
                else if (iLower == "viewer")
                {
                    ProjectManager.Default.IsViewerVisibility = System.Windows.Visibility.Collapsed;
                }
                else if (string.IsNullOrEmpty(file))
                {
                    file = i;
                }
                else if (iLower == "silent")
                {
                    // already handled in main.
                    continue;
                }
                else
                {
                    throw new System.InvalidOperationException(string.Format("Unknown parameter in arguments: {0}", i));
                }
            }
        }

        /// <summary>Save all the state info + flushes the current brain to disk.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Application_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            ProjectManager.Default.SaveSettings();
            Designer.Properties.Settings.Default.Save();
            CharacterEngine.SpeechEngine.AvailableVoices.Save();
            if (ProjectManager.Default.IsSandBox)
            {
                ProjectManager.Default.CleanSandbox();
            }
        }

        #endregion

        /// <summary>
        ///     Creates the list of assemblies that contain extra sins used by the designer.
        /// </summary>
        /// <returns>A list of assebmlies, currently this list only contains the 'Sensory Interfaces' and wordnet assembly.</returns>
        private System.Collections.Generic.List<System.Reflection.Assembly> CreateAssemblies()
        {
            var iRes = new System.Collections.Generic.List<System.Reflection.Assembly>();
            iRes.Add(System.Reflection.Assembly.GetAssembly(typeof(ImageSin)));
            iRes.Add(System.Reflection.Assembly.GetAssembly(typeof(WordNetSin)));
            iRes.Add(System.Reflection.Assembly.GetAssembly(typeof(Queries.Query)));
            return iRes;
        }

        #region Timer

        /// <summary>
        ///     Checks the user settings for the auto save option and updates the timer accordingly.
        /// </summary>
        public void UpdateAutoSave()
        {
            if (Designer.Properties.Settings.Default.AutoSave)
            {
                if (fAutoSaveTimer == null)
                {
                    fAutoSaveTimer =
                        new System.Windows.Threading.DispatcherTimer(
                            System.Windows.Threading.DispatcherPriority.Background);
                    fAutoSaveTimer.Tick += fAutoSaveTimer_Tick;
                }

                fAutoSaveTimer.Interval = Designer.Properties.Settings.Default.AutoSaveInterval;
                fAutoSaveTimer.Start();
            }
            else if (fAutoSaveTimer != null)
            {
                fAutoSaveTimer.Tick -= fAutoSaveTimer_Tick;
                fAutoSaveTimer.Stop();
                fAutoSaveTimer = null;
            }
        }

        /// <summary>Try to save the brain when auto save is on.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void fAutoSaveTimer_Tick(object sender, System.EventArgs e)
        {
            if (fUndoCountAtLastSave != WindowMain.UndoStore.UndoData.Count)
            {
                // only try to save if something changed.
                if (string.IsNullOrEmpty(ProjectManager.Default.Location) == false)
                {
                    // we only try to save if we have saved it before.
                    var iSaver = new ProjectSaver();
                    iSaver.Save(); // performs an async save.
                    fUndoCountAtLastSave = WindowMain.UndoStore.UndoData.Count;
                }
            }
        }

        #endregion

        // StringBuilder iStr = new StringBuilder();

        /// <summary>Handles the DispatcherUnhandledException event of the Application control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs"/> instance containing
        ///     the event data.</param>
        private void Application_DispatcherUnhandledException(
            object sender, 
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            SendMailTo(e.Exception.ToString(), "exceptions@bragisoft.com");
            if (Current == null || Current.MainWindow == null || Current.MainWindow.IsLoaded == false)
            {
                ShowEmptyWindow();

                    // when there is an exception before any window is visible, the messagebox doesn't remain visible. The solution is to create a temp window and show this so that the messagebox remains visible.
                System.Windows.MessageBox.Show(
                    "Fatal error while trying to start the application: " + e.Exception.Message, 
                    "Startup", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
                Current.Shutdown(-1);
                return;
            }

            System.Windows.MessageBox.Show(
                e.Exception.ToString(), 
                "Unhandled exception", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
            e.Handled = true;
            if (e.Exception is System.NullReferenceException)
            {
                Current.Shutdown(-1);
            }
        }

        /// <summary>Creates a mail and sends it to the specified location.</summary>
        /// <param name="message">The message.</param>
        /// <param name="to">To.</param>
        /// <param name="title">The title.</param>
        private void SendMailTo(string message, string to, string title = null)
        {
            // MailMessage iM = new MailMessage();
            // iM.To.Add(new MailAddress(to));
            // string iFrom = Regex.Replace(Environment.UserName, @"[^\w\.@-]", "");
            // if (string.IsNullOrEmpty(iFrom) == false)
            // {
            // iM.From = new MailAddress(iFrom + "@nnd.com");
            // iM.Subject = title;
            // iM.Body = message;

            // SmtpClient iClient = new SmtpClient("smtp.bragisoft.com");
            // iClient.Credentials = new System.Net.NetworkCredential("exceptions@bragisoft.com", "Funkmaster1#");
            // iClient.Send(iM);
            // }
        }

        #endregion
    }
}