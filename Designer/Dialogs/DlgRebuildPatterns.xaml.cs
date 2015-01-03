// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgRebuildPatterns.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgRebuildPatterns.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Interaction logic for DlgRebuildPatterns.xaml
    /// </summary>
    public partial class DlgRebuildPatterns : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgRebuildPatterns"/> class.</summary>
        public DlgRebuildPatterns()
        {
            InitializeComponent();
            CurrentPos = 0;
            fToProcess = Enumerable.ToList(BrainData.Current.Editors.AllTextPatternEditors());

            if (BrainData.Current.ChatbotProps == null)
            {
                // the chatbot props object isn't created automatically, only upon request.
                BrainData.Current.ChatbotProps = new ChatbotProperties();
                fCreatedChatbotProps = true;
            }
            else
            {
                fCreatedChatbotProps = false;
            }

            Maximum = (ulong)fToProcess.Count;
            if (BrainData.Current.ChatbotProps.FallBacks != null)
            {
                Maximum += (ulong)BrainData.Current.ChatbotProps.FallBacks.Count;
            }

            if (BrainData.Current.ChatbotProps.DoAfterStatement != null)
            {
                Maximum += (ulong)BrainData.Current.ChatbotProps.DoAfterStatement.Count;
            }

            if (BrainData.Current.ChatbotProps.ConversationStarts != null)
            {
                Maximum += (ulong)BrainData.Current.ChatbotProps.ConversationStarts.Count;
            }

            if (BrainData.Current.ChatbotProps.DoOnStartup != null)
            {
                Maximum += (ulong)BrainData.Current.ChatbotProps.DoOnStartup.Count;
            }
        }

        /// <summary>The on click start.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickStart(object sender, System.Windows.RoutedEventArgs e)
        {
            if (BtnStartStop.Tag == null)
            {
                LogItems.Clear();
                CurrentPos = 0;
                BtnStartStop.Content = "Stop";
                BtnStartStop.Tag = this; // we use a pointer, this is faster to check instead of boxing the val
                BtnStartStop.ToolTip = "Stop the process.";
                BtnClose.IsEnabled = false;
                System.Action iThread = ProcessItems;
                System.AsyncCallback iDoWhenDone = ProcessFinished;
                var iAsync = iThread.BeginInvoke(iDoWhenDone, null);
            }

            // else
            // fStopProcessing = true;
        }

        /// <summary>
        ///     Processes all the neurons. This allows us to call this function async.
        /// </summary>
        private void ProcessItems()
        {
            ProcessPatterns();
            ProcessOutputPatterns(BrainData.Current.ChatbotProps.ConversationStarts, "converstaion starts");
            ProcessOutputPatterns(BrainData.Current.ChatbotProps.FallBacks, "project fallbacks");
            ProcessDoStatements(BrainData.Current.ChatbotProps.DoAfterStatement, "Do after statement");
            ProcessDoStatements(BrainData.Current.ChatbotProps.DoOnStartup, "Do on startup");
        }

        /// <summary>The process do statements.</summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceName">The source name.</param>
        private void ProcessDoStatements(DoPatternCollection source, string sourceName)
        {
            var iErrors = new System.Collections.Generic.List<string>();
            if (source != null)
            {
                source.Rebuild(iErrors);
                if (iErrors.Count > 0)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<System.Collections.Generic.List<string>, string>(AddErrors), 
                        iErrors, 
                        sourceName);
                }

                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Send, 
                    new System.Action<int>(AddToCurPos), 
                    source.Count);
            }
        }

        /// <summary>The process output patterns.</summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceName">The source name.</param>
        private void ProcessOutputPatterns(PatternOutputsCollection source, string sourceName)
        {
            var iErrors = new System.Collections.Generic.List<string>();
            if (source != null)
            {
                source.Rebuild(iErrors);
                if (iErrors.Count > 0)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<System.Collections.Generic.List<string>, string>(AddErrors), 
                        iErrors, 
                        sourceName);
                }

                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Send, 
                    new System.Action<int>(AddToCurPos), 
                    source.Count);
            }
        }

        /// <summary>The process patterns.</summary>
        private void ProcessPatterns()
        {
            var iCount = 1;
            System.Collections.Generic.List<string> iErrors;
            foreach (var i in fToProcess)
            {
                try
                {
                    iErrors = new System.Collections.Generic.List<string>();
                    i.Rebuild(iErrors);
                    if (iErrors.Count > 0)
                    {
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<System.Collections.Generic.List<string>, string>(AddErrors), 
                            iErrors, 
                            i.Name);
                    }
                }
                catch (System.Exception e)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(LogItems.Add), 
                        string.Format("parse failed: '{0}', error: {1}.", i.Name, e.Message));
                    ProjectManager.Default.DataError = true;
                }

                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Send, 
                    new System.Action<int>(SetCurPos), 
                    iCount); // set last pos
                iCount++;
            }

            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Send, 
                new System.Action<int>(SetCurPos), 
                fToProcess.Count); // set last pos
        }

        /// <summary>Called when the async <see cref="DlgFixBrokenRefs.ProcessItems"/> is finished.  Updates the buttons and stuff
        ///     to indicate that we are done.</summary>
        /// <param name="result">The result.</param>
        private void ProcessFinished(System.IAsyncResult result)
        {
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Send, 
                new System.Action(InternalProcessFinished));
            if (fCreatedChatbotProps)
            {
                BrainData.Current.ChatbotProps = null;
            }
        }

        /// <summary>The internal process finished.</summary>
        private void InternalProcessFinished()
        {
            BtnStartStop.Content = "Start";
            BtnStartStop.ToolTip = "Start the process.";
            BtnClose.IsEnabled = true;
            BtnStartStop.Tag = null; // always let the system know we canallow a new start.
            LogItems.Add("All patterns rebuild!");
        }

        /// <summary>The add to cur pos.</summary>
        /// <param name="value">The value.</param>
        private void AddToCurPos(int value)
        {
            CurrentPos += (ulong)value;
        }

        /// <summary>Sets the current pos (for async calls).</summary>
        /// <param name="id">The id.</param>
        private void SetCurPos(int id)
        {
            CurrentPos = (ulong)id;
        }

        /// <summary>Adds the errors to the list.</summary>
        /// <param name="errors">The errors.</param>
        /// <param name="source">The source.</param>
        private void AddErrors(System.Collections.Generic.List<string> errors, string source)
        {
            foreach (var i in errors)
            {
                LogItems.Add(source + ": " + i);
            }
        }

        #region Fields

        /// <summary>The f log items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<string> fLogItems =
            new System.Collections.ObjectModel.ObservableCollection<string>();

        /// <summary>The f to process.</summary>
        private readonly System.Collections.Generic.List<TextPatternEditor> fToProcess;

        /// <summary>The f created chatbot props.</summary>
        private readonly bool fCreatedChatbotProps;

        #endregion

        #region Prop

        #region CurrentPos

        /// <summary>
        ///     CurrentPos Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CurrentPosProperty =
            System.Windows.DependencyProperty.Register(
                "CurrentPos", 
                typeof(ulong), 
                typeof(DlgRebuildPatterns), 
                new System.Windows.FrameworkPropertyMetadata((ulong)0));

        /// <summary>
        ///     Gets or sets the CurrentPos property.  This dependency property
        ///     indicates the id of the current neuron being processed.
        /// </summary>
        public ulong CurrentPos
        {
            get
            {
                return (ulong)GetValue(CurrentPosProperty);
            }

            set
            {
                SetValue(CurrentPosProperty, value);
            }
        }

        #endregion

        #region Maximum

        /// <summary>
        ///     Maximum Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty MaximumProperty =
            System.Windows.DependencyProperty.Register(
                "Maximum", 
                typeof(ulong), 
                typeof(DlgRebuildPatterns), 
                new System.Windows.FrameworkPropertyMetadata((ulong)0));

        /// <summary>
        ///     Gets or sets the Maximum property.  This dependency property
        ///     indicates how many neurons need to be processed in total.
        /// </summary>
        public ulong Maximum
        {
            get
            {
                return (ulong)GetValue(MaximumProperty);
            }

            set
            {
                SetValue(MaximumProperty, value);
            }
        }

        #endregion

        #region LogItems

        /// <summary>
        ///     Gets the list of log items that need to shown to the user.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> LogItems
        {
            get
            {
                return fLogItems;
            }
        }

        #endregion

        #endregion
    }
}