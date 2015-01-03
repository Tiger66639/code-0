// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrowserDataSource.cs" company="">
//   
// </copyright>
// <summary>
//   Provides a way to declare in xaml, different datasources for a
//   <see cref="NeuronDataBrowser" /> object. Inherits from
//   <see cref="frameworkElement" /> so that we have a datacontext and can use
//   proper bindings in xaml, making this object easy to use.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Provides a way to declare in xaml, different datasources for a
    ///     <see cref="NeuronDataBrowser" /> object. Inherits from
    ///     <see cref="frameworkElement" /> so that we have a datacontext and can use
    ///     proper bindings in xaml, making this object easy to use.
    /// </summary>
    public class BrowserDataSource : System.Windows.FrameworkElement
    {
        /// <summary>The f is opened.</summary>
        private bool fIsOpened;

        #region Content

        /// <summary>
        ///     Gets the object that should be displayed in the content of the
        ///     tabitem. Since this object inherits from frameworkElement
        /// </summary>
        public object Content
        {
            get
            {
                return new BDSWrapper { Data = this };
            }
        }

        #endregion

        #region IsLoaded

        /// <summary>
        ///     Gets the wether the data is loaded or not. Controlled by the
        ///     NDBrowser. Is used to determine how to return the itemsSource bound to
        ///     the UI element. This allows us to unload the data when needed, to save
        ///     resources, cause a drop down box doesn't do this automatically when
        ///     closed. It also allows us to reload the data each time when shown, so
        ///     that it is always the latest snapshot.
        /// </summary>
        public bool IsOpened
        {
            get
            {
                return fIsOpened;
            }

            internal set
            {
                fIsOpened = value;
                CoerceValue(UIItemsSourceProperty);
            }
        }

        #endregion

        #region internal types

        /// <summary>
        ///     A wrapper for the <see cref="BrowserDataSource" /> object so that we
        ///     can use it as the content of a content presenter and have proper
        ///     binding applied to it.
        /// </summary>
        public class BDSWrapper
        {
            /// <summary>
            ///     Gets or sets the data.
            /// </summary>
            /// <value>
            ///     The data.
            /// </value>
            public BrowserDataSource Data { get; set; }
        }

        #endregion

        #region PageHeader

        /// <summary>
        ///     <see cref="PageHeader" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty PageHeaderProperty =
            System.Windows.DependencyProperty.Register(
                "PageHeader", 
                typeof(object), 
                typeof(BrowserDataSource), 
                new System.Windows.FrameworkPropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the <see cref="PageHeader" /> property. This dependency
        ///     property indicates the header to use for the page.
        /// </summary>
        public object PageHeader
        {
            get
            {
                return GetValue(PageHeaderProperty);
            }

            set
            {
                SetValue(PageHeaderProperty, value);
            }
        }

        #endregion

        #region PageToolTip

        /// <summary>
        ///     <see cref="PageToolTip" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty PageToolTipProperty =
            System.Windows.DependencyProperty.Register(
                "PageToolTip", 
                typeof(object), 
                typeof(BrowserDataSource), 
                new System.Windows.FrameworkPropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the <see cref="PageToolTip" /> property. This dependency
        ///     property indicates the tooltip to use for this page.
        /// </summary>
        public object PageToolTip
        {
            get
            {
                return GetValue(PageToolTipProperty);
            }

            set
            {
                SetValue(PageToolTipProperty, value);
            }
        }

        #endregion

        #region AsTree

        /// <summary>
        ///     <see cref="AsTree" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty AsTreeProperty =
            System.Windows.DependencyProperty.Register(
                "AsTree", 
                typeof(bool), 
                typeof(BrowserDataSource), 
                new System.Windows.FrameworkPropertyMetadata(false, OnAsTreeChanged));

        /// <summary>
        ///     Gets or sets the <see cref="AsTree" /> property. This dependency
        ///     property indicates how to display this datasource: as a tree or list.
        ///     Best only to set at beginning.
        /// </summary>
        public bool AsTree
        {
            get
            {
                return (bool)GetValue(AsTreeProperty);
            }

            set
            {
                SetValue(AsTreeProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="AsTree"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnAsTreeChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((BrowserDataSource)d).OnAsTreeChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="AsTree"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnAsTreeChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            // throw new NotImplementedException();
        }

        #endregion

        #region AsDate

        /// <summary>
        ///     <see cref="AsDate" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty AsDateProperty =
            System.Windows.DependencyProperty.Register(
                "AsDate", 
                typeof(bool), 
                typeof(BrowserDataSource), 
                new System.Windows.FrameworkPropertyMetadata(false));

        /// <summary>
        ///     Gets or sets the <see cref="AsDate" /> property. This dependency
        ///     property indicates that this datasource represents a date selection
        ///     page. When set to true, their should not be an
        ///     <see cref="ItemsSource" /> defined, since it wont be used anyway.
        /// </summary>
        public bool AsDate
        {
            get
            {
                return (bool)GetValue(AsDateProperty);
            }

            set
            {
                SetValue(AsDateProperty, value);
            }
        }

        #endregion

        #region ItemsSource

        /// <summary>
        ///     <see cref="ItemsSource" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemsSourceProperty =
            System.Windows.DependencyProperty.Register(
                "ItemsSource", 
                typeof(System.Collections.IEnumerable), 
                typeof(BrowserDataSource), 
                new System.Windows.FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        ///     Gets or sets the <see cref="ItemsSource" /> property. This dependency
        ///     property indicates the list to use as itemsSource. Note, all children
        ///     should implement the
        /// </summary>
        public System.Collections.IEnumerable ItemsSource
        {
            get
            {
                return (System.Collections.IEnumerable)GetValue(ItemsSourceProperty);
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
            ((BrowserDataSource)d).UIItemsSource = (System.Collections.IEnumerable)e.NewValue;
        }

        #endregion

        #region UIItemsSource

        /// <summary>
        ///     <see cref="UIItemsSource" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty UIItemsSourceProperty =
            System.Windows.DependencyProperty.Register(
                "UIItemsSource", 
                typeof(System.Collections.IEnumerable), 
                typeof(BrowserDataSource), 
                new System.Windows.FrameworkPropertyMetadata(null, null, CoerceUIItemsSourceValue));

        /// <summary>
        ///     Gets or sets the <see cref="UIItemsSource" /> property. This dependency
        ///     property indicates the list to use by the UI.
        /// </summary>
        public System.Collections.IEnumerable UIItemsSource
        {
            get
            {
                return (System.Collections.IEnumerable)GetValue(UIItemsSourceProperty);
            }

            set
            {
                SetValue(UIItemsSourceProperty, value);
            }
        }

        /// <summary>Coerces the <see cref="UIItemsSource"/> value.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        private static object CoerceUIItemsSourceValue(System.Windows.DependencyObject d, object value)
        {
            var iSource = (BrowserDataSource)d;
            if (iSource.IsOpened == false)
            {
                return null;
            }

            var iVal = value as System.Collections.IEnumerable;
            if (iVal != null)
            {
                if (iVal is System.Collections.IList)
                {
                    // ILists always enumerate correctly and behave badly if we want to get another Enumerator, so check for this.
                    return iVal;
                }

                return iVal.GetEnumerator();

                    // we use this method so that the enumerator can get a change to creat a new object: this makes certain that the UI element always rerfresshes correctly.
            }

            return null;
        }

        #endregion
    }
}