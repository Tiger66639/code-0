// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionFrameItem.cs" company="">
//   
// </copyright>
// <summary>
//   Wraps a single code item as seen by the execution frame (so we can track
//   some info about it and can do syncing and stuff).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Wraps a single code item as seen by the execution frame (so we can track
    ///     some info about it and can do syncing and stuff).
    /// </summary>
    public class ExecutionFrameItem : Data.ObservableObject, INeuronInfo, INeuronWrapper
    {
        /// <summary>The f is next statement.</summary>
        private bool fIsNextStatement;

        /// <summary>The f item.</summary>
        private readonly Expression fItem;

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ExecutionFrameItem"/> class. Initializes a new instance of the <see cref="ExecutionFrameItem"/>
        ///     class.</summary>
        /// <param name="item">The item.</param>
        public ExecutionFrameItem(Expression item)
        {
            fItem = item;
        }

        #endregion

        #region Prop

        #region IsNextStatement

        /// <summary>
        ///     Gets/sets wether this is the next statement to be executed or not.
        /// </summary>
        public bool IsNextStatement
        {
            get
            {
                return fIsNextStatement;
            }

            internal set
            {
                fIsNextStatement = value;

                // App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<string>(OnPropertyChanged), "IsNextStatement");
                OnPropertyChanged("IsNextStatement");

                    // this should be thread save cause property changed works accross threads.
            }
        }

        #endregion

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

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item
        {
            get
            {
                return fItem;
            }
        }

        #endregion

        #endregion
    }
}