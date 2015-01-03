// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlCodeEditorPage.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlCodeEditorPage.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for CtrlCodeEditorPage.xaml
    /// </summary>
    public partial class CtrlCodeEditorPage : System.Windows.Controls.UserControl
    {
        #region Fields

        // List<NeuronToolBoxItem> fRegisteredToolboxItems;
        /// <summary>The f page.</summary>
        private CodeEditorPage fPage;

        #endregion                                                //required because datacontext is lost when unloaded.

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CtrlCodeEditorPage"/> class.</summary>
        public CtrlCodeEditorPage()
        {
            InitializeComponent();
            DataContextChanged += CtrlCodeEditorPage_DataContextChanged;
        }

        #endregion

        #region Event handlers

        /// <summary>The ctrl code editor page_ data context changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CtrlCodeEditorPage_DataContextChanged(
            object sender, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            fPage = DataContext as CodeEditorPage;
        }

        ///// <summary>
        ///// When this control gets loaded, and there are no toolbox items created yet for the list of variables on this
        ///// page, do so now.
        ///// </summary>
        // private void UserControl_Loaded(object sender, RoutedEventArgs e)
        // {
        // CreateToolBoxItems();
        // }

        ///// <summary>
        ///// If the list of toolbox items is not yet removed from the braindata, do so now
        ///// </summary>
        // private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        // {
        // RemoveToolBoxItems();
        // }

        // void RegisteredVariables_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        // {
        // CodeEditorPage iPage = (CodeEditorPage)DataContext;
        // switch (e.Action)
        // {
        // case NotifyCollectionChangedAction.Add:
        // foreach (Variable i in e.NewItems)
        // AddVariable(i, iPage);
        // break;
        // case NotifyCollectionChangedAction.Remove:
        // foreach (Variable i in e.OldItems)
        // RemoveVariable(i, iPage);
        // break;
        // case NotifyCollectionChangedAction.Replace:
        // break;
        // case NotifyCollectionChangedAction.Reset:
        // foreach (ToolBoxItem i in fRegisteredToolboxItems)
        // BrainData.Current.ToolBoxItems.Remove(i);
        // fRegisteredToolboxItems.Clear();
        // break;
        // default:
        // break;
        // }
        // }
        #endregion

        /// <summary>Handles the TiltWheel event of the CPPanel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The<see cref="JaStDev.HAB.Designer.WPF.Controls.MouseTiltEventArgs"/>
        ///     instance containing the event data.</param>
        private void CPPanel_TiltWheel(object sender, WPF.Controls.MouseTiltEventArgs e)
        {
            ScrollHor.Value += e.Tilt;
        }

        /// <summary>Handles the MouseLeave event of the Slider control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void Slider_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ToggleZoom.IsChecked = false;
        }

        #region Commands

        #region Order

        /// <summary>The move up_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveUp_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iPage = (CodeEditorPage)DataContext;
            e.CanExecute = iPage.CanMoveUpFor(iPage.Items);
        }

        /// <summary>The move down_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveDown_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iPage = (CodeEditorPage)DataContext;
            e.CanExecute = iPage.CanMoveDownFor(iPage.Items);
        }

        /// <summary>The move up_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveUp_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iPage = (CodeEditorPage)DataContext;
            iPage.MoveUpFor(iPage.Items);
        }

        /// <summary>The move down_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveDown_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iPage = (CodeEditorPage)DataContext;
            iPage.MoveDownFor(iPage.Items);
        }

        /// <summary>The move to end_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveToEnd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iPage = (CodeEditorPage)DataContext;
            iPage.MoveToEndFor(iPage.Items);
        }

        /// <summary>The move to home_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveToHome_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iPage = (CodeEditorPage)DataContext;
            iPage.MoveToStartFor(iPage.Items);
        }

        #endregion

        #region ViewCode

        /// <summary>Handles the CanExecute event of the ViewCodeCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ViewCodeCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = fPage != null && fPage.SelectedItems.Count > 0;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the RemoveCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ViewCodeCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iSelected = fPage.SelectedItem;
            if (iSelected != null)
            {
                ((WindowMain)System.Windows.Application.Current.MainWindow).ViewCodeForNeuron(iSelected.Item);
            }
        }

        #endregion

        #region Zoom

        /// <summary>Handles the CanExecute event of the ViewCodeCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Zoom_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = fPage != null && e.Parameter is double;
        }

        /// <summary>Handles the Executed event of the RemoveCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Zoom_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            fPage.ZoomProcent = (double)e.Parameter;
        }

        #endregion

        #region Sync

        /// <summary>Handles the Executed event of the SyncCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void SyncCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iSelected = fPage.SelectedItem;
            if (iSelected != null)
            {
                ((WindowMain)System.Windows.Application.Current.MainWindow).SyncExplorerToNeuron(iSelected.Item);
            }
        }

        #endregion

        #region Rename

        /// <summary>Handles the Executed event of the RenameCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void RenameCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iPage = (CodeEditorPage)DataContext;
            System.Diagnostics.Debug.Assert(iPage != null);
            EditorsHelper.RenameItem(iPage.SelectedItem.Item, "Change code item title");
        }

        #endregion

        #region ChangeTo

        /// <summary>Handles the Executed event of the ChangeTo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ChangeTo_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iFound = (from i in fPage.SelectedItems select i.Item).ToArray();
            if (iFound.Length > 1)
            {
                EditorsHelper.TryChangeTypeTo(iFound, (System.Type)e.Parameter);
            }
            else if (iFound.Length == 1)
            {
                EditorsHelper.TryChangeTypeTo(iFound[0], (System.Type)e.Parameter);
            }
        }

        #endregion

        #endregion

        #region functions

        // private void CreateToolBoxItems()
        // {
        // if (fRegisteredToolboxItems == null && fPage != null)
        // {
        // fRegisteredToolboxItems = new List<NeuronToolBoxItem>();
        // foreach (Variable i in fPage.RegisteredVariables)
        // AddVariable(i, fPage);
        // fPage.RegisteredVariables.CollectionChanged += new NotifyCollectionChangedEventHandler(RegisteredVariables_CollectionChanged);
        // }
        // }

        ///// <summary>
        ///// Adds a variable to the list of toolbox items of the brain so they can appear on any toolbox controls.
        ///// </summary>
        ///// <param name="variable"></param>
        ///// <param name="page"></param>
        // private void AddVariable(Variable variable, CodeEditorPage page)
        // {
        // NeuronToolBoxItem iNew = new NeuronToolBoxItem();
        // iNew.Item = variable;
        // iNew.Category = string.Format("Variables for {0} - {1}", BrainData.Current.NeuronInfo[page.Item.ID].DisplayTitle, page.Title);
        // BrainData.Current.ToolBoxItems.Add(iNew);
        // fRegisteredToolboxItems.Add(iNew);
        // }

        // private void RemoveToolBoxItems()
        // {
        // if (fRegisteredToolboxItems != null && fPage != null)
        // {
        // fPage.RegisteredVariables.CollectionChanged -= new NotifyCollectionChangedEventHandler(RegisteredVariables_CollectionChanged);
        // foreach (NeuronToolBoxItem i in fRegisteredToolboxItems)
        // BrainData.Current.ToolBoxItems.Remove(i);
        // fRegisteredToolboxItems = null;
        // }
        // }

        // private void RemoveVariable(Variable i, CodeEditorPage iPage)
        // {
        // NeuronToolBoxItem iToolboxItem = (from iReg in fRegisteredToolboxItems
        // where iReg.Item == i
        // select iReg).FirstOrDefault();
        // if (iToolboxItem != null)
        // {
        // BrainData.Current.ToolBoxItems.Remove(iToolboxItem);
        // fRegisteredToolboxItems.Remove(iToolboxItem);
        // }
        // }
        #endregion
    }
}