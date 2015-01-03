// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObservingCanvas.cs" company="">
//   
// </copyright>
// <summary>
//   A Canvas that is able to observe a list of visuals
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A Canvas that is able to observe a list of visuals
    /// </summary>
    public class ObservingCanvas : System.Windows.Controls.Canvas, System.Windows.IWeakEventListener
    {
        #region IWeakEventManagner

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns><see langword="true"/> if the listener handled the event. It is
        ///     considered an error by the <see cref="System.Windows.WeakEventManager"/> handling in
        ///     WPF to register a listener for an event that the listener does not
        ///     handle. Regardless, the method should return <see langword="false"/>
        ///     if it receives an event that it does not recognize or handle.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(System.Collections.Specialized.CollectionChangedEventManager))
            {
                iList_CollectionChanged(sender, (System.Collections.Specialized.NotifyCollectionChangedEventArgs)e);
                return true;
            }

            return false;
        }

        #endregion

        #region ItemsSource

        /// <summary>
        ///     <see cref="ItemsSource" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemsSourceProperty =
            System.Windows.DependencyProperty.Register(
                "ItemsSource", 
                typeof(System.Collections.ObjectModel.ObservableCollection<System.Windows.UIElement>), 
                typeof(ObservingCanvas), 
                new System.Windows.FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        ///     Gets or sets the <see cref="ItemsSource" /> property. This dependency
        ///     property indicates This list to observe.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<System.Windows.UIElement> ItemsSource
        {
            get
            {
                return
                    (System.Collections.ObjectModel.ObservableCollection<System.Windows.UIElement>)
                    GetValue(ItemsSourceProperty);
            }

            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="ItemsSource"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnItemsSourceChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((ObservingCanvas)d).OnItemsSourceChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="ItemsSource"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnItemsSourceChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var iList = (System.Collections.ObjectModel.ObservableCollection<System.Windows.UIElement>)e.OldValue;
                System.Collections.Specialized.CollectionChangedEventManager.RemoveListener(iList, this);
                InternalChildren.Clear();
            }

            if (e.NewValue != null)
            {
                ObservingCanvas iPrevCanvas = null;
                var iList = (System.Collections.ObjectModel.ObservableCollection<System.Windows.UIElement>)e.NewValue;
                foreach (var i in iList)
                {
                    var iEl = i as System.Windows.FrameworkElement;

                        // when we start the app with a project as parameter, the ChatbotChannelView gets loaded 2 times without an unload in between. This can cause lots of problems. But we can fix this by checking if the item is on a previous panel, if so, simply remove it.
                    if (iEl != null)
                    {
                        var iPrevPanel = iEl.Parent as ObservingCanvas;
                        if (iPrevPanel != null)
                        {
                            iPrevPanel.Children.Remove(iEl);
                        }

                        if (iPrevCanvas == null)
                        {
                            iPrevCanvas = iPrevPanel;
                        }
                    }

                    InternalChildren.Add(i);
                }

                if (iPrevCanvas != null)
                {
                    iPrevCanvas.ItemsSource = null;
                }

                System.Collections.Specialized.CollectionChangedEventManager.AddListener(iList, this);
            }
        }

        /// <summary>The i list_ collection changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iList_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var iNew = (System.Windows.UIElement)e.NewItems[0];
                    if (System.Windows.Media.VisualTreeHelper.GetParent(iNew) == null)
                    {
                        // don't try to add it if it's already the child of another item, this causes a fatal crash. Can sometimes happen (not certain why yet).
                        InternalChildren.Add(iNew);
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    InternalChildren.Remove((System.Windows.UIElement)e.OldItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    InternalChildren.Clear();
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}