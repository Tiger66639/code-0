// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatLog.cs" company="">
//   
// </copyright>
// <summary>
//   contains all the data for a single chatlog session.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     contains all the data for a single chatlog session.
    /// </summary>
    public class ChatLog : Data.NamedObject, INeuronWrapper, INeuronInfo
    {
        /// <summary>The f conversation log.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<string> fConversationLog =
            new System.Collections.ObjectModel.ObservableCollection<string>();

        /// <summary>Initializes a new instance of the <see cref="ChatLog"/> class.</summary>
        /// <param name="item">The item.</param>
        public ChatLog(Neuron item)
        {
            Item = item;
            LoadData();
        }

        #region ConversationLog

        /// <summary>
        ///     Gets the conversation log in plain text
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> ConversationLog
        {
            get
            {
                return fConversationLog;
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

        /// <summary>The load data.</summary>
        private void LoadData()
        {
            var iTime = Item.FindFirstOut((ulong)PredefinedNeurons.Time) as NeuronCluster;
            if (iTime != null)
            {
                var iDT = Time.GetTime(iTime);
                if (iDT.HasValue)
                {
                    Name = iDT.Value.ToString();
                }
            }

            if (string.IsNullOrEmpty(Name))
            {
                Name = "unknown date";
            }

            var iItem = Item as NeuronCluster;
            if (iItem != null)
            {
                System.Collections.Generic.List<NeuronCluster> iChildren;
                using (var iList = iItem.Children) iChildren = iList.ConvertTo<NeuronCluster>();
                var iConvHistory = Brain.Current[(ulong)PredefinedNeurons.ConversationLogHistory];
                foreach (var i in iChildren)
                {
                    PrintLine(i, iConvHistory);
                }

                Factories.Default.CLists.Recycle(iChildren);
            }
            else
            {
                fConversationLog.Add("Not a chat-log!");
            }
        }

        /// <summary>The print line.</summary>
        /// <param name="line">The line.</param>
        /// <param name="iConvHistory">The i conv history.</param>
        private void PrintLine(NeuronCluster line, Neuron iConvHistory)
        {
            var iRes = new System.Text.StringBuilder();
            if (line.Meaning == (ulong)PredefinedNeurons.In)
            {
                iRes.Append(ChatBotChannel.USER);
            }
            else
            {
                iRes.Append(ChatBotChannel.BOT);
            }

            var iLink = Link.Find(Item, line, iConvHistory);
            if (iLink != null)
            {
                var iLineData = iLink.Info.ConvertTo<Neuron>();
                foreach (var i in iLineData)
                {
                    iRes.Append(BrainHelper.GetTextFrom(i));
                }

                fConversationLog.Add(iRes.ToString());
            }
        }

        /// <summary>
        ///     Deletes this instance from the network.
        /// </summary>
        internal void Delete()
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                EditorsHelper.DeleteLogItem(Item);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }
    }
}