// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionFrame.cs" company="">
//   
// </copyright>
// <summary>
//   Contains the data of a function that is active in a
//   <see cref="Processor" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains the data of a function that is active in a
    ///     <see cref="Processor" /> .
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <para>This contains:</para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     the neuron who's code is being executed (usually the '
    ///                     <see cref="JaStDev.HAB.Link.Meaning" /> ' part of a link.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>The code list being executed (immutable).</description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     the next item to execute (stored in the code items).
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Because there are a lot of calls to other execution list being made, we
    ///         need to keep track of the different values. That's what this class does.
    ///     </para>
    /// </remarks>
    public class ExecutionFrame : Data.OwnedObject, INeuronInfo, INeuronWrapper, IEditorSelection
    {
        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        public NeuronData NeuronInfo
        {
            get
            {
                return BrainData.Current.NeuronInfo[Item.ID];
            }
        }

        #endregion

        #region Fields

        /// <summary>The f code.</summary>
        private CodeItemCollection fCode;

        /// <summary>The f next index.</summary>
        private int fNextIndex = -1;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f conditional type.</summary>
        private DebugNeuron fConditionalType;

        /// <summary>The f has conditional type.</summary>
        private bool fHasConditionalType;

        /// <summary>The f selected code items.</summary>
        private readonly EditorItemSelectionList<CodeItem> fSelectedCodeItems = new EditorItemSelectionList<CodeItem>();

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ExecutionFrame"/> class.</summary>
        /// <param name="frame">The frame.</param>
        public ExecutionFrame(CallFrame frame)
        {
            var iFrame = frame as ExpressionBlockFrame;
            if (iFrame != null)
            {
                Item = iFrame.Block;
            }
            else
            {
                Item = frame.ExecSource;
            }

            CodeListType = frame.CodeListType;
            Frame = frame;
        }

        /// <summary>Initializes a new instance of the <see cref="ExecutionFrame"/> class,
        ///     used for the 'if' and case statement'</summary>
        /// <param name="conditionsCluster">The conditions cluster.</param>
        public ExecutionFrame(NeuronCluster conditionsCluster)
        {
            fCode = new CodeItemCollection(this, conditionsCluster);

            // fConditionalType =
        }

        #endregion

        #region Prop

        /// <summary>
        ///     Gets the frame.
        /// </summary>
        /// <value>
        ///     The frame.
        /// </value>
        public CallFrame Frame { get; private set; }

        #region NextIndex

        /// <summary>
        ///     Gets/sets the index of the next expression that will be executed.
        /// </summary>
        public int NextIndex
        {
            get
            {
                return fNextIndex;
            }

            set
            {
                if (value != fNextIndex)
                {
                    if (fCode != null && fNextIndex > -1 && fNextIndex < fCode.Count)
                    {
                        fCode[fNextIndex].IsNextStatement = false;
                    }

                    fNextIndex = value;
                    if (fCode != null && fNextIndex > -1 && fNextIndex < fCode.Count)
                    {
                        fCode[fNextIndex].IsNextStatement = true;
                    }

                    OnPropertyChanged("NextIndex");

                        // this should be thread save cause property changed works accross threads.
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets the index of the currently executing statemnt. This is
        ///     different from 'NextIndex', which isn't always filled in. This is used
        ///     by the debugger to build a display path to the currently executing
        ///     statement.
        /// </summary>
        /// <value>
        ///     The index of the current.
        /// </value>
        public int CurrentIndex { get; set; }

        #region IsSelected

        /// <summary>
        ///     Gets if this item is selected.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            internal set
            {
                fIsSelected = value;

                // App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<string>(OnPropertyChanged), "IsSelected");
                OnPropertyChanged("IsSelected");

                    // this should be thread save cause property changed works accross threads.
            }
        }

        #endregion

        #region Code

        /// <summary>
        ///     Gets the list of execution items being performed by the processor.
        /// </summary>
        public CodeItemCollection Code
        {
            get
            {
                if (fCode == null)
                {
                    switch (Frame.CodeListType)
                    {
                        case ExecListType.Rules:
                            fCode = new CodeItemCollection(this, Frame.ExecSource.RulesCluster, false);
                            break;
                        case ExecListType.Actions:
                            NeuronCluster iActionsData = null;
                            if (Frame.ExecSource != null)
                            {
                                iActionsData = Frame.ExecSource.ActionsCluster;
                            }

                            if (iActionsData == null)
                            {
                                iActionsData = NeuronFactory.GetCluster();

                                    // this is to make certain we have a result, even if something went seriously wrong.
                            }

                            fCode = new CodeItemCollection(this, iActionsData, false);
                            break;
                        case ExecListType.Children:
                            fCode = new CodeItemCollection(this, Frame.ExecSource as NeuronCluster, false);
                            break;
                        case ExecListType.Conditional:
                            if (Frame is ConditionsFrame)
                            {
                                fCode = new CodeItemCollection(this, ((ConditionsFrame)Frame).ConditionsCluster, false);
                            }
                            else
                            {
                                fCode = new CodeItemCollection(this, Frame.ExecSource as NeuronCluster, false);
                            }

                            break;
                        default:
                            break;
                    }

                    if (fCode.Count > 0)
                    {
                        if (fNextIndex <= -1)
                        {
                            fCode[0].IsNextStatement = true;
                        }
                        else if (fNextIndex < fCode.Count)
                        {
                            fCode[fNextIndex].IsNextStatement = true;
                        }
                        else
                        {
                            fCode[fCode.Count - 1].IsNextStatement = true;
                        }
                    }
                }

                return fCode;
            }
        }

        #endregion

        #region SelectedItems

        /// <summary>
        ///     gets the list with all the selected items.
        /// </summary>
        public System.Collections.Generic.IList<CodeItem> SelectedCodeItems
        {
            get
            {
                return fSelectedCodeItems;
            }
        }

        #endregion

        #region SelectedItem

        /// <summary>
        ///     gets the first selected item.
        /// </summary>
        /// <remarks>
        ///     Used by the ContextMenu of the <see cref="CodeEditorPage" /> to find
        ///     the codeItem's IsBreakPoint property (and possibly others).
        /// </remarks>
        public CodeItem SelectedCodeItem
        {
            get
            {
                if (fSelectedCodeItems.Count > 0)
                {
                    return fSelectedCodeItems[0];
                }

                return null;
            }
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets the <see cref="Neuron" /> who's code list is being executed.
        /// </summary>
        public Neuron Item { get; private set; }

        #endregion

        #region CodeListType

        /// <summary>
        ///     Identifies the type of the code list for the
        ///     <see cref="JaStDev.HAB.Designer.ExecutionFrame.Item" /> . Possible
        ///     values: Rules, Actions, Children, conditional.
        /// </summary>
        /// <remarks>
        ///     A string is easy to display.
        /// </remarks>
        public ExecListType CodeListType { get; private set; }

        #endregion

        #region ConditionalType

        /// <summary>
        ///     Gets/sets the type of the conditional statement (if/case/...)
        /// </summary>
        public DebugNeuron ConditionalType
        {
            get
            {
                if (Frame.CodeListType == ExecListType.Conditional && fConditionalType == null)
                {
                    // we create it delayed (so only when needed) cause this consumed a lot of processing time.
                    fConditionalType = new DebugNeuron(((ConditionalFrame)Frame).ConditionType);
                }

                return fConditionalType;
            }

            set
            {
                if (value != fConditionalType)
                {
                    fConditionalType = value;
                    HasConditionalType = fConditionalType != null;
                    OnPropertyChanged("ConditionalType");
                }
            }
        }

        #endregion

        #region HasConditionalType

        /// <summary>
        ///     Gets wether this item has a conditional type assigned or not.
        /// </summary>
        public bool HasConditionalType
        {
            get
            {
                return fHasConditionalType;
            }

            private set
            {
                fHasConditionalType = value;
                OnPropertyChanged("HasConditionalType");
            }
        }

        #endregion

        #endregion

        #region IEditorSelection Members

        /// <summary>Gets the selected items.</summary>
        public System.Collections.IList SelectedItems
        {
            get
            {
                return fSelectedCodeItems;
            }
        }

        /// <summary>Gets the selected item.</summary>
        public object SelectedItem
        {
            get
            {
                return SelectedCodeItem;
            }
        }

        #endregion
    }
}