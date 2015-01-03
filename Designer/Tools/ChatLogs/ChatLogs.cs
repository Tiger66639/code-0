// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatLogs.cs" company="">
//   
// </copyright>
// <summary>
//   entry point for the chatlogs overview.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     entry point for the chatlogs overview.
    /// </summary>
    public class ChatLogs : Data.ObservableObject, INeuronWrapper, INeuronInfo, IDocumentInfo, IDocOpener
    {
        /// <summary>The f chat logs.</summary>
        private static ChatLogs fChatLogs;

        /// <summary>The f is open.</summary>
        private bool fIsOpen;

        /// <summary>The f selected item.</summary>
        private ChatLog fSelectedItem;

        /// <summary>Prevents a default instance of the <see cref="ChatLogs"/> class from being created. 
        ///     Prevents a default instance of the <see cref="ChatLogs"/> class from
        ///     being created.</summary>
        private ChatLogs()
        {
        }

        /// <summary>
        ///     Gets the entry point.
        /// </summary>
        public static ChatLogs Current
        {
            get
            {
                if (fChatLogs == null)
                {
                    fChatLogs = new ChatLogs();
                }

                return fChatLogs;
            }
        }

        #region Logs

        /// <summary>
        ///     Gets the list of logs currently stored in the project.
        /// </summary>
        public ChatlogsCollection Logs { get; internal set; }

        #endregion

        #region SelectedItem

        /// <summary>
        ///     Gets/sets the currently selected item.
        /// </summary>
        public ChatLog SelectedItem
        {
            get
            {
                return fSelectedItem;
            }

            set
            {
                if (fSelectedItem != value)
                {
                    fSelectedItem = value;
                    OnPropertyChanged("SelectedItem");
                }
            }
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        public NeuronData NeuronInfo
        {
            get
            {
                return BrainData.Current.NeuronInfo[Item];
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        public Neuron Item { get; private set; }

        #endregion

        /// <summary>The delete all.</summary>
        internal void DeleteAll()
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iList = (from i in Logs select i.Item).ToList();

                    // make a local copy cause we are going to change the content of the list
                foreach (var i in iList)
                {
                    EditorsHelper.DeleteLogItem(i);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #region IsOpen (IDocOpener

        /// <summary>
        ///     Gets/sets the if this comm channel is curently visible or not. This
        ///     prop is controlled from the back-end (when it gets closed, this is
        ///     always called.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return fIsOpen;
            }

            set
            {
                if (fIsOpen != value)
                {
                    SetIsOpen(value);
                }
            }
        }

        /// <summary>The set is open.</summary>
        /// <param name="value">The value.</param>
        private void SetIsOpen(bool value)
        {
            if (value)
            {
                // load data only when needed.
                Item = Brain.Current[(ulong)PredefinedNeurons.ConversationLogHistory];
                Logs = new ChatlogsCollection(this, (NeuronCluster)Item);
                if (Logs.Count > 0)
                {
                    // select the first item upon opening, if there is one
                    SelectedItem = Logs[0];
                }
            }
            else
            {
                Item = null;
                Logs = null;
                SelectedItem = null;
            }

            fIsOpen = value;
            OnPropertyChanged("IsOpen");
            OnPropertyChanged("IsVisible");
        }

        /// <summary>
        ///     same prop as <see cref="JaStDev.HAB.Designer.ChatLogs.IsOpen" /> ,
        ///     this is controlled from the UI, so when changed, the document still
        ///     needs to be made visible/hidden.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return fIsOpen;
            }

            set
            {
                if (fIsOpen != value)
                {
                    ChangeOpenDocuments();
                }
            }
        }

        /// <summary>The change open documents.</summary>
        protected internal virtual void ChangeOpenDocuments()
        {
            if (BrainData.Current != null && BrainData.Current.DesignerData != null)
            {
                // when designerData is not set, not all the data is loaded yet. + this is also called when the project is cleared, so that comm channels get a change of clearing any 'global stuff', like the character windows of the chatbots.
                if (IsOpen == false)
                {
                    // when not visible -> make it visible.
                    var iMain = (WindowMain)System.Windows.Application.Current.MainWindow;
                    if (iMain != null)
                    {
                        iMain.AddItemToOpenDocuments(this);
                    }
                    else if (BrainData.Current != null && BrainData.Current.OpenDocuments != null)
                    {
                        BrainData.Current.OpenDocuments.Add(this); // this actually shows the item
                    }
                }
                else
                {
                    BrainData.Current.OpenDocuments.Remove(this);
                }
            }
        }

        #endregion

        #region IDocumentInfo Members

        /// <summary>Gets the document title.</summary>
        public string DocumentTitle
        {
            get
            {
                return "Chatlog history";
            }
        }

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>
        ///     The document info.
        /// </value>
        public string DocumentInfo
        {
            get
            {
                return "All the chatlogs currerently stored in the network.";
            }
        }

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>
        ///     The type of the document.
        /// </value>
        public string DocumentType
        {
            get
            {
                return "chatlog history";
            }
        }

        /// <summary>
        ///     Gets or sets the document icon.
        /// </summary>
        /// <value>
        ///     The document icon.
        /// </value>
        public object DocumentIcon
        {
            get
            {
                return "/Images/Tools/chatlogHistory.png";
            }
        }

        #endregion
    }
}