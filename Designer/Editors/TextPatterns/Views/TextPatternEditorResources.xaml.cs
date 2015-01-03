// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextPatternEditorResources.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for TextPatternEditorResources.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using JaStDev.ControlFramework.Utility;
using JaStDev.HAB.Designer.WPF.Controls;
using JaStDev.HAB.Parsers;

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for TextPatternEditorResources.xaml
    /// </summary>
    public partial class TextPatternEditorResources : ResourceDictionary
    {
        /// <summary>
        ///     Used to indicate if focus needs to be set or not. This is used to put
        ///     focus on ui elements from the back end. By default, they always return
        ///     false, except when this prop is true, after wich they reset this back
        ///     to false. This allows us to bind the UI elements to various bools and
        ///     only have them selected when required + don't need sto store 'focus'
        ///     info on every object, but only global.
        /// </summary>
        public static bool NeedsFocus = false;

        /// <summary>
        ///     when the control is loading, all the bound properties call the
        ///     getters, in that case, we still need an extra <see langword="switch" />
        ///     to help figure out which object extactly needs focus (all props use
        ///     the same field 'NeedsFocus' + when there is a list, we also need to
        ///     find the correct item).
        /// </summary>
        internal static FocusPointer FocusOn;

        /// <summary>
        ///     Used for the textboxes: they are bound using an update 'OnLostFocus'
        ///     which doesn't work when the user switches tab page. In that case, we
        ///     only have the 'Unloaded' event, but at that point, the data point is
        ///     lost, so we need to keep track of this behind the scenes. so we can do
        ///     a correct update when the UI element gets unloaded without updating
        ///     it's backing source
        /// </summary>
        private ParsableTextPatternBase fLastDataObject;

        /// <summary>Initializes a new instance of the <see cref="TextPatternEditorResources"/> class. 
        ///     Initializes a new instance of the<see cref="TextPatternEditorResources"/> class.</summary>
        public TextPatternEditorResources()
        {
            InitializeComponent();
        }

        /// <summary>Handles the TextChanged event of the TxtSubPattern control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtSubPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iDef = iSender.DataContext as PatternRule;
                Debug.Assert(iDef != null);
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                Debug.Assert(iView != null);
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    iView.CurrentEditMode = EditMode.AddPattern;
                    AddNewTextPattern(iDef.TextPatterns, iSender.Text);
                    iSender.Text = string.Empty;
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>from 'input' button: add at the top.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddInput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as Button;
            Debug.Assert(iSender != null);
            var iDef = iSender.DataContext as PatternRule;
            Debug.Assert(iDef != null);
            var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
            Debug.Assert(iView != null);
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                iView.CurrentEditMode = EditMode.AddPattern;
                AddNewTextPattern(iDef.TextPatterns, string.Empty, 0);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the TextChanged event of the TxtPattern control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        iView.CurrentEditMode = EditMode.AddPattern;
                        var iNew = AddNewRule((FrameworkElement)iView);
                        Debug.Assert(iNew != null);
                        AddNewTextPattern(iNew.TextPatterns, iSender.Text);
                        iSender.Text = string.Empty;
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>Handles the TextChanged event of the TxtPattern control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtQuestion_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iView = TreeHelper.FindInTree<TextPatternEditorView>(iSender);
                if (iView != null)
                {
                    var iEditor = (TextPatternEditor)iView.DataContext;
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        iView.CurrentEditMode = EditMode.AddOutput;
                        var iNew = EditorsHelper.AddNewConditionalToPattern(iEditor.Questions);
                        AddNewOutput(iNew.Outputs, iSender.Text);
                        EditorsHelper.AddNewCondition(iNew, string.Empty);

                            // we create an empty condition, so that there always is one, otherwise, the editor wont have one and cant create one
                        iSender.Text = string.Empty;
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>Handles the TextChanged event of the TxtPattern control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtCondOfQuestion_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iView = TreeHelper.FindInTree<TextPatternEditorView>(iSender);
                if (iView != null)
                {
                    var iEditor = (TextPatternEditor)iView.DataContext;
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        iView.CurrentEditMode = EditMode.AddConditional;
                        var iNew = EditorsHelper.AddNewConditionalToPattern(iEditor.Questions);
                        EditorsHelper.AddNewCondition(iNew, iSender.Text);
                        iSender.Text = string.Empty;
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>Handles the TextChanged event of the TxtPattern control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtCondOfRepeat_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iList = iSender.DataContext as ConditionalOutputsCollection;
                Debug.Assert(iList != null);
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                Debug.Assert(iView != null);
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    iView.CurrentEditMode = EditMode.AddConditional;

                        // this is for the UI, it's captured by the binding, to place the cursor position correctly
                    var iNew = EditorsHelper.AddNewConditionalToPattern(iList);
                    EditorsHelper.AddNewCondition(iNew, iSender.Text);
                    iSender.Text = string.Empty;
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>Handles the TextChanged event of the TxtPattern control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtRepeat_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iList = iSender.DataContext as ConditionalOutputsCollection;
                Debug.Assert(iList != null);
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                Debug.Assert(iView != null);
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    iView.CurrentEditMode = EditMode.AddOutput;

                        // this is for the UI, it's captured by the binding, to place the cursor position correctly
                    var iNew = EditorsHelper.AddNewConditionalToPattern(iList);
                    AddNewOutput(iNew.Outputs, iSender.Text);
                    EditorsHelper.AddNewCondition(iNew, string.Empty);

                        // we create an empty condition, so that there always is one, otherwise, the editor wont have one and cant create one
                    iSender.Text = string.Empty;
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>Handles the Click event of the BtnAddConditional control. Adds a new
        ///     conditional section.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnAddRepeatBlock_Click(object sender, RoutedEventArgs e)
        {
            var iSender = sender as Button;
            Debug.Assert(iSender != null);
            var iCond = iSender.DataContext as PatternRuleOutput;
            var iEditor = iCond.Owner as ChatbotProperties;
            var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                iView.CurrentEditMode = EditMode.AddConditional;

                    // this is for the UI, it's captured by the binding, to place the cursor position correctly
                var iIndex = iEditor.ResponsesOnRepeat.IndexOf(iCond);
                var iNew = EditorsHelper.AddNewConditionalToPattern(iEditor.ResponsesOnRepeat, iIndex);

                    // we insert at this pos, so we allow for correction in the order.
                EditorsHelper.AddNewCondition(iNew, string.Empty);

                    // we create an empty condition, so that there always is one, otherwise, the editor wont have one and cant create one
                iNew.IsSelected = true;
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the Click event of the BtnAddConditional control. Adds a new
        ///     conditional section.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnAddCondQuestion_Click(object sender, RoutedEventArgs e)
        {
            var iSender = sender as Button;
            Debug.Assert(iSender != null);
            var iCond = iSender.DataContext as PatternRuleOutput;
            var iEditor = iCond.Owner as TextPatternEditor;
            var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                iView.CurrentEditMode = EditMode.AddConditional;

                    // this is for the UI, it's captured by the binding, to place the cursor position correctly
                var iIndex = iEditor.Questions.IndexOf(iCond);
                var iNew = EditorsHelper.AddNewConditionalToPattern(iEditor.Questions, iIndex);

                    // we insert at this pos, so we allow for correction in the order.
                EditorsHelper.AddNewCondition(iNew, string.Empty);

                    // we create an empty condition, so that there always is one, otherwise, the editor wont have one and cant create one
                iNew.IsSelected = true;
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The insert condition_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void InsertCondition_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            Debug.Assert(iSender != null);
            var iCond = iSender.DataContext as PatternRuleOutput;
            var iOwner = iCond.Owner as IConditionalOutputsCollectionOwner;
            var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                iView.CurrentEditMode = EditMode.AddConditional;

                    // this is for the UI, it's captured by the binding, to place the cursor position correctly
                var iIndex = iOwner.Conditionals.IndexOf(iCond);
                var iNew = EditorsHelper.AddNewConditionalToPattern(iOwner.Conditionals, iIndex);
                EditorsHelper.AddNewCondition(iNew, string.Empty);

                    // we create an empty condition, so that there always is one, otherwise, the editor wont have one and cant create one
                iNew.IsSelected = true;
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The btn remove conditional_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnRemoveConditional_Click(object sender, RoutedEventArgs e)
        {
            var iSender = sender as Button;
            Debug.Assert(iSender != null);
            var iCond = iSender.DataContext as PatternRuleOutput;
            var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                EditorsHelper.DeleteConditionalPattern((NeuronCluster)iCond.Item);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the TextChanged event of the TxtSubOutput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtSubOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iDef = iSender.DataContext as PatternRuleOutput;
                Debug.Assert(iDef != null);
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                Debug.Assert(iView != null);
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    iView.CurrentEditMode = EditMode.AddOutput;

                        // this is for the UI, it's captured by the binding, to place the cursor position correctly
                    AddNewOutput(iDef.Outputs, iSender.Text);
                    iSender.Text = string.Empty;
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>from 'input' button: add at the top.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddOutput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            Debug.Assert(iSender != null);
            var iDef = iSender.DataContext as PatternRuleOutput;
            Debug.Assert(iDef != null);
            var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
            Debug.Assert(iView != null);
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                iView.CurrentEditMode = EditMode.AddOutput;

                    // this is for the UI, it's captured by the binding, to place the cursor position correctly
                AddNewOutput(iDef.Outputs, string.Empty, 0);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the TextChanged event of the TxtOutput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        iView.CurrentEditMode = EditMode.AddOutput;

                            // this is for the UI, it's captured by the binding, to place the cursor position correctly
                        var iNew = AddNewRule((FrameworkElement)iView);
                        Debug.Assert(iNew != null);
                        AddNewOutput(iNew.Outputs, iSender.Text);
                        iSender.Text = string.Empty;
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>Handles the TextChanged event of the TxtInvalidResponse control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtInvalidResponse_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iOut = iSender.DataContext as OutputPattern;
                Debug.Assert(iOut != null);
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                Debug.Assert(iView != null);
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    iView.CurrentEditMode = EditMode.AddInvalid;
                    AddNewInvalid(iOut.InvalidResponses, iSender.Text);
                    iSender.Text = string.Empty;
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>The add invalid_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddInvalid_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as Button;
            Debug.Assert(iSender != null);
            var iOut = iSender.DataContext as OutputPattern;
            Debug.Assert(iOut != null);
            var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
            Debug.Assert(iView != null);
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                iView.CurrentEditMode = EditMode.AddInvalid;
                AddNewInvalid(iOut.InvalidResponses, string.Empty, 0);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the TextChanged event of the TxtNewFallback control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtNewFallback_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                Debug.Assert(iView != null);
                var iCol = iSender.DataContext as PatternOutputsCollection;
                if (iCol != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        iView.CurrentEditMode = EditMode.AddInvalid;
                        var iNew = EditorsHelper.AddNewOutputPattern(iCol, iSender.Text);
                        iNew.IsSelected = true;
                        iSender.Text = string.Empty;
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>Handles the TextChanged event of the TxtNewTopicFilter control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtNewTopicFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                Debug.Assert(iView != null);
                var iEditor = (TextPatternEditor)((FrameworkElement)iView).DataContext;
                if (iEditor != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        iView.CurrentEditMode = EditMode.AddTopicFilter;
                        var iNew = EditorsHelper.AddNewTopicFilter(iEditor.TopicFilters, iSender.Text);
                        iNew.IsSelected = true;
                        iSender.Text = string.Empty;
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>Handles the Executed event of the AddTopicFilter control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddTopicFilter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            Debug.Assert(iSender != null);
            var iEditor = iSender.DataContext as TextPatternEditor;
            Debug.Assert(iEditor != null);
            var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
            Debug.Assert(iView != null);
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                iView.CurrentEditMode = EditMode.AddTopicFilter;

                    // this is for the UI, it's captured by the binding, to place the cursor position correctly
                var iNew = EditorsHelper.AddNewTopicFilter(iEditor.TopicFilters, string.Empty);
                iNew.IsSelected = true;
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the TextChanged event of the TxtNewResponseFor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtNewResponseFor_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            Debug.Assert(iSender != null);
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                Debug.Assert(iView != null);

                var iRule = iSender.DataContext as PatternRule;
                Debug.Assert(iRule != null);
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    iView.CurrentEditMode = EditMode.AddResponseFor;
                    var iGrp = EditorsHelper.AddNewResponsesGroup(iRule.ResponsesFor, 0);
                    var iNew = EditorsHelper.AddNewPatternStyleResponseFor(iGrp.ResponseFor, iSender.Text);
                    iNew.IsSelected = true;
                    iSender.Text = string.Empty;
                    EditorsHelper.AddNewConditionalToPattern(iGrp.Conditionals);

                        // also need to prepare a new conditional for the group. always insert at the top cause that makes most sense from an editing point of view.
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary><para>Handles the TextChanged event of the TxtSubDo control.</para>
        /// <para>Handles the TextChanged event of the textbox on the chatbot's property
        ///         page.</para>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtChatbotDo_TextChanged(object sender, TextChangedEventArgs e)
        {
            throw new NotImplementedException();

            // TextBox iSender = sender as TextBox;
            // Debug.Assert(iSender != null);
            // if (string.IsNullOrEmpty(iSender.Text) == false)
            // {
            // DoPatternCollection iList = iSender.DataContext as DoPatternCollection;
            // Debug.Assert(iList != null);
            // ITextPatternEditorView iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
            // Debug.Assert(iView != null);
            // WindowMain.UndoStore.BeginUndoGroup();
            // try
            // {
            // iView.CurrentEditMode = EditMode.AddDo;                                            //this is for the UI, it's captured by the binding, to place the cursor position correctly
            // DoPattern iNew = EditorsHelper.AddNewDoPattern(iList, iSender.Text);
            // iNew.IsSelected = true;
            // iSender.Text = string.Empty;
            // }
            // finally
            // {
            // WindowMain.UndoStore.EndUndoGroup();
            // }
            // }
        }

        /// <summary>The drop down ns selector_ selected neuron changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DropDownNSSelector_SelectedNeuronChanged(object sender, RoutedPropertyChangedEventArgs<Neuron> e)
        {
            var iSender = sender as DropDownNSSelector;
            Debug.Assert(iSender != null);
            var iNew = e.NewValue as TextNeuron;
            if (iNew != null)
            {
                var iOut = iSender.DataContext as ResponsesForGroup;
                Debug.Assert(iOut != null);
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    iOut.ResponseFor.Add(new ResponseForOutput(iNew)); // this generates undo data.
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }

            iSender.SelectedNeuron = null;
        }

        /// <summary>The response for_ selection changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ResponseFor_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<Neuron> e)
        {
            var iSender = sender as DropDownNSSelector;
            Debug.Assert(iSender != null);
            var iNew = e.NewValue as TextNeuron;

            var iOut = iSender.DataContext as ResponseForOutput;
            Debug.Assert(iOut != null);
            var iOwner = iOut.Owner as IResponseForOutputOwner;
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                if (iNew != null)
                {
                    var iVal = new ResponseForOutput(iNew);
                    var iIndex = iOwner.ResponseFor.IndexOf(iOut);
                    iOwner.ResponseFor[iIndex] = iVal;
                }
                else
                {
                    iOut.Delete();
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The new responses for group_ selected neuron changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NewResponsesForGroup_SelectedNeuronChanged(object sender, RoutedPropertyChangedEventArgs<Neuron> e)
        {
            var iSender = sender as DropDownNSSelector;
            Debug.Assert(iSender != null);
            var iNew = e.NewValue as TextNeuron;
            if (iNew != null)
            {
                var iRule = iSender.DataContext as PatternRule;
                Debug.Assert(iRule != null);
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    var iGrp = EditorsHelper.AddNewResponsesGroup(iRule.ResponsesFor, 0);
                    iGrp.ResponseFor.Add(new ResponseForOutput(iNew));
                    EditorsHelper.AddNewConditionalToPattern(iGrp.Conditionals);

                        // also need to prepare a new conditional for the group. always insert at the top cause that makes most sense from an editing point of view.
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }

            iSender.SelectedNeuron = null;
        }

        /// <summary>adds a new <see langword="ref"/> to an output pattern to the list of
        ///     responsesFor. This uses a dummy ' ' (space) pattern as new value, to
        ///     imitate an empty field.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddResponseFor_Click(object sender, RoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            var iOut = iSender.DataContext as ResponsesForGroup;
            Debug.Assert(iOut != null);
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                TextNeuron iNew;
                if (Properties.Settings.Default.ResponseForDefaultStyleAsPattern == false)
                {
                    iNew = TextNeuron.GetFor(" ");
                }
                else
                {
                    iNew = NeuronFactory.GetText(string.Empty);
                    Brain.Current.Add(iNew);
                    var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                    Debug.Assert(iView != null);
                    iView.CurrentEditMode = EditMode.AddResponseFor;
                }

                if (iNew != null)
                {
                    iOut.ResponseFor.Add(new ResponseForOutput(iNew)); // this generates undo data.
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Adds the new invalid.</summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        private void AddNewInvalid(InvalidPatternResponseCollection list, string value, int index = -1)
        {
            var iNew = EditorsHelper.AddNewInvalidPatternResponse(list, value, index);
            iNew.IsSelected = true;
        }

        /// <summary></summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        private void AddNewTextPattern(InputPatternCollection list, string value, int index = -1)
        {
            var iNew = EditorsHelper.AddNewTextPattern(list, value, index);
            iNew.IsSelected = true;
        }

        /// <summary>The add new output.</summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        private void AddNewOutput(PatternOutputsCollection list, string value, int index = -1)
        {
            var iNew = EditorsHelper.AddNewOutputPattern(list, value, index);
            iNew.IsSelected = true;
        }

        /// <summary>Creates a new text pattern and Adds it to the list.</summary>
        /// <param name="view">The view.</param>
        /// <returns>The <see cref="PatternRule"/>.</returns>
        private PatternRule AddNewRule(FrameworkElement view)
        {
            var iEditor = (TextPatternEditor)view.DataContext;
            if (iEditor != null)
            {
                var iRes = EditorsHelper.MakePatternRule(iEditor, -1);
                iRes.NeuronInfo.DisplayTitle = TopicsDictionary.GetUniqueRuleName(
                    "new rule ", 
                    ((INeuronWrapper)iRes.Owner).Item);
                iRes.IsSelected = true;
                iEditor.SelectedRuleIndex = iEditor.Items.Count - 1; // make certain that the item is also selected
                return iRes;
            }

            return null;
        }

        /// <summary>Handles the Loaded event of the TxtPattern control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtPattern_Loaded(object sender, RoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    if (iView.CurrentEditMode == EditMode.AddPattern)
                    {
                        iSender.Focus();
                        iSender.CaretIndex = iSender.Text.Length;
                        iView.CurrentEditMode = EditMode.Normal;
                    }
                    else
                    {
                        var iBind = iSender.GetBindingExpression(TextBox.TextProperty);
                        if (iBind != null)
                        {
                            iBind.UpdateSource();
                        }

                        var iPattern = iSender.DataContext as RangedPatternEditorItem;
                        if (iPattern.Selectionrange != null)
                        {
                            ApplySelectionRangeTo(iSender, iPattern);
                        }
                    }
                }
            }
        }

        /// <summary>Handles the Loaded event of the TxtPattern control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtResponseFor_Loaded(object sender, RoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    if (iView.CurrentEditMode == EditMode.AddResponseFor)
                    {
                        iSender.Focus();
                        iSender.CaretIndex = iSender.Text.Length;
                        iView.CurrentEditMode = EditMode.Normal;
                    }
                    else
                    {
                        var iBind = iSender.GetBindingExpression(TextBox.TextProperty);
                        if (iBind != null)
                        {
                            iBind.UpdateSource();
                        }

                        var iPattern = iSender.DataContext as RangedPatternEditorItem;
                        if (iPattern.Selectionrange != null)
                        {
                            ApplySelectionRangeTo(iSender, iPattern);
                        }
                    }
                }
            }
        }

        /// <summary>Handles the Loaded event of the TxtOutput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtOutput_Loaded(object sender, RoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    if (iView.CurrentEditMode == EditMode.AddOutput)
                    {
                        iSender.Focus();
                        iSender.CaretIndex = iSender.Text.Length;
                        iView.CurrentEditMode = EditMode.Normal;
                    }
                    else
                    {
                        var iBind = iSender.GetBindingExpression(TextBox.TextProperty);
                        if (iBind != null)
                        {
                            iBind.UpdateSource();
                        }

                        var iPattern = iSender.DataContext as RangedPatternEditorItem;
                        if (iPattern.Selectionrange != null)
                        {
                            ApplySelectionRangeTo(iSender, iPattern);
                        }
                    }
                }
            }
        }

        /// <summary>Handles the Loaded event of the TxtInvalid control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtInvalid_Loaded(object sender, RoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    if (iView.CurrentEditMode == EditMode.AddInvalid)
                    {
                        iSender.Focus();
                        iSender.CaretIndex = iSender.Text.Length;
                        iView.CurrentEditMode = EditMode.Normal;
                    }
                    else
                    {
                        var iBind = iSender.GetBindingExpression(TextBox.TextProperty);
                        if (iBind != null)
                        {
                            iBind.UpdateSource();
                        }

                        var iPattern = iSender.DataContext as TextPatternBase;
                        if (iPattern.Selectionrange != null)
                        {
                            ApplySelectionRangeTo(iSender, iPattern);
                        }
                    }
                }
            }
        }

        /// <summary>Handles the Loaded event of the TxtInvalid control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtTopicFilter_Loaded(object sender, RoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    if (iView.CurrentEditMode == EditMode.AddTopicFilter)
                    {
                        iSender.Focus();
                        iSender.CaretIndex = iSender.Text.Length;
                        iView.CurrentEditMode = EditMode.Normal;
                    }
                    else
                    {
                        var iBind = iSender.GetBindingExpression(TextBox.TextProperty);
                        if (iBind != null)
                        {
                            iBind.UpdateSource();
                        }

                        var iPattern = iSender.DataContext as TextPatternBase;
                        if (iPattern.Selectionrange != null)
                        {
                            ApplySelectionRangeTo(iSender, iPattern);
                        }
                    }
                }
            }
        }

        /// <summary>Handles the Loaded event of the TxtDo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtDo_Loaded(object sender, RoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iPattern = iSender.DataContext as DoPattern;
                if (TextPatternEditorResources.NeedsFocus == true && TextPatternEditorResources.FocusOn.Item == iPattern)
                {
                    iSender.Focus();
                    iSender.CaretIndex = iSender.Text.Length;
                    TextPatternEditorResources.NeedsFocus = false;
                    TextPatternEditorResources.FocusOn.Item = null;
                }
                else if (iPattern != null)
                {
                    // iPattern can be null for the chatbotprops, when there isn't an object yet.
                    var iBind = iSender.GetBindingExpression(TextBox.TextProperty);
                    if (iBind != null)
                    {
                        iBind.UpdateSource();
                    }

                    if (iPattern.Selectionrange != null)
                    {
                        ApplySelectionRangeTo(iSender, iPattern);
                    }
                }
            }
        }

        /// <summary>Handles the Loaded event of the TxtCondition control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtCondition_Loaded(object sender, RoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    if (iView.CurrentEditMode == EditMode.AddConditional)
                    {
                        iSender.Focus();
                        iSender.CaretIndex = iSender.Text.Length;
                        iView.CurrentEditMode = EditMode.Normal;
                    }
                    else
                    {
                        var iBind = iSender.GetBindingExpression(TextBox.TextProperty);
                        if (iBind != null)
                        {
                            iBind.UpdateSource();
                        }

                        var iPattern = iSender.DataContext as TextPatternBase;
                        if (iPattern != null && iPattern.Selectionrange != null)
                        {
                            ApplySelectionRangeTo(iSender, iPattern);
                        }
                    }
                }
            }
        }

        /// <summary>Need to unselect all the previous data when an 'AddNew' textbox gets
        ///     focus, this is cleaner editing.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtNewItem_GotFocus(object sender, RoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            var iView = TreeHelper.FindInTree<UserControl>(iSender);
            if (iView != null)
            {
                var iEditor = iView.DataContext as IEditorSelection;
                if (iEditor != null)
                {
                    iEditor.SelectedItems.Clear();
                }
            }
        }

        /// <summary>The txt item_ got focus.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtItem_GotFocus(object sender, RoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            var iItem = iSender.DataContext as EditorItem;
            if (iItem != null)
            {
                iItem.IsSelected = true;
            }
        }

        /// <summary>Handles the MouseDoubleClick event of the GrpPatterns control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void GrpPatterns_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var iSender = sender as Control;
            var iView = TreeHelper.FindInTree<UserControl>(iSender);
            Debug.Assert(iSender != null);
            var iEditor = iView.DataContext as IEditorSelection;
            var iDef = iSender.DataContext as PatternRule;
            if (iDef != null && iEditor != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    // if the ctrl is pressed, add, otherwise clear the selection islt.
                    iEditor.SelectedItems.Clear();
                }

                foreach (object i in iDef.TextPatterns)
                {
                    iEditor.SelectedItems.Add(i);
                }
            }

            if (iEditor.SelectedItem != null)
            {
                var iSelected = (TextPatternBase)iEditor.SelectedItem;
                if (iSelected.Selectionrange != null)
                {
                    // we make certain that the selectionrange don't contain any chars, cause then the copy doesn't behave as expected.
                    iSelected.Selectionrange = new SelectionRange()
                                                   {
                                                       Length = 0, 
                                                       Start = iSelected.Selectionrange.Start
                                                   };
                }

                iSelected.Focus(); // make certain that the first item is focused so that the copy paste works correctly.
            }

            e.Handled = true;
        }

        /// <summary>Handles the MouseDoubleClick event of the GrpInvalidResponse control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void GrpInvalidResponse_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var iSender = sender as Control;
            var iView = TreeHelper.FindInTree<TextPatternEditorView>(iSender);
            Debug.Assert(iSender != null);
            var iEditor = iView.DataContext as TextPatternEditor;
            var iOut = iSender.DataContext as OutputPattern;
            if (iOut != null && iEditor != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    // if the ctrl is pressed, add, otherwise clear the selection islt.
                    iEditor.SelectedItems.Clear();
                }

                foreach (object i in iOut.InvalidResponses)
                {
                    iEditor.SelectedItems.Add(i);
                }
            }

            if (iEditor.SelectedItem != null)
            {
                var iSelected = (TextPatternBase)iEditor.SelectedItem;
                if (iSelected.Selectionrange != null)
                {
                    // we make certain that the selectionrange don't contain any chars, cause then the copy doesn't behave as expected.
                    iSelected.Selectionrange = new SelectionRange()
                                                   {
                                                       Length = 0, 
                                                       Start = iSelected.Selectionrange.Start
                                                   };
                }

                iSelected.Focus(); // make certain that the first item is focused so that the copy paste works correctly.
            }

            e.Handled = true;
        }

        /// <summary>Handles the MouseDoubleClick event of the GrpOutputs control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void GrpOutputs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var iSender = sender as Control;
            var iView = TreeHelper.FindInTree<TextPatternEditorView>(iSender);
            Debug.Assert(iSender != null);
            var iEditor = iView.DataContext as TextPatternEditor;
            var iCond = iSender.DataContext as PatternRuleOutput;
            if (iCond != null && iCond.Outputs != null && iEditor != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    // if the ctrl is pressed, add, otherwise clear the selection islt.
                    iEditor.SelectedItems.Clear();
                }

                foreach (object i in iCond.Outputs)
                {
                    iEditor.SelectedItems.Add(i);
                }
            }

            if (iEditor.SelectedItem != null)
            {
                var iSelected = (TextPatternBase)iEditor.SelectedItem;
                if (iSelected.Selectionrange != null)
                {
                    // we make certain that the selectionrange don't contain any chars, cause then the copy doesn't behave as expected.
                    iSelected.Selectionrange = new SelectionRange()
                                                   {
                                                       Length = 0, 
                                                       Start = iSelected.Selectionrange.Start
                                                   };
                }

                iSelected.Focus(); // make certain that the first item is focused so that the copy paste works correctly.
            }

            e.Handled = true;
        }

        /// <summary>Handles the MouseDoubleClick event of the GrpTopicFilters control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void GrpTopicFilters_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var iSender = sender as Control;
            var iView = TreeHelper.FindInTree<TextPatternEditorView>(iSender);
            Debug.Assert(iSender != null);
            var iEditor = iView.DataContext as TextPatternEditor;
            if (iEditor != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    // if the ctrl is pressed, add, otherwise clear the selection islt.
                    iEditor.SelectedItems.Clear();
                }

                foreach (object i in iEditor.TopicFilters)
                {
                    iEditor.SelectedItems.Add(i);
                }
            }

            if (iEditor.SelectedItem != null)
            {
                var iSelected = (TextPatternBase)iEditor.SelectedItem;
                if (iSelected.Selectionrange != null)
                {
                    // we make certain that the selectionrange don't contain any chars, cause then the copy doesn't behave as expected.
                    iSelected.Selectionrange = new SelectionRange()
                                                   {
                                                       Length = 0, 
                                                       Start = iSelected.Selectionrange.Start
                                                   };
                }

                iSelected.Focus(); // make certain that the first item is focused so that the copy paste works correctly.
            }

            e.Handled = true;
        }

        /// <summary>Handles the MouseDoubleClick event of the GrpDoPatterns control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void GrpDoPatterns_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var iSender = sender as Control;
            var iView = TreeHelper.FindInTree<TextPatternEditorView>(iSender);
            Debug.Assert(iSender != null);
            var iEditor = iView.DataContext as TextPatternEditor;
            var iCond = iSender.DataContext as PatternRuleOutput;
            if (iCond != null && iCond.Outputs != null && iEditor != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    // if the ctrl is pressed, add, otherwise clear the selection islt.
                    iEditor.SelectedItems.Clear();
                }

                foreach (object i in iCond.DoPatterns)
                {
                    iEditor.SelectedItems.Add(i);
                }
            }

            if (iEditor.SelectedItem != null)
            {
                var iSelected = (TextPatternBase)iEditor.SelectedItem;
                if (iSelected.Selectionrange != null)
                {
                    // we make certain that the selectionrange don't contain any chars, cause then the copy doesn't behave as expected.
                    iSelected.Selectionrange = new SelectionRange()
                                                   {
                                                       Length = 0, 
                                                       Start = iSelected.Selectionrange.Start
                                                   };
                }

                iSelected.Focus(); // make certain that the first item is focused so that the copy paste works correctly.
            }

            e.Handled = true;
        }

        /// <summary>The grp to calculate_ mouse double click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void GrpToCalculate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var iSender = sender as Control;
            var iView = TreeHelper.FindInTree<TextPatternEditorView>(iSender);
            Debug.Assert(iSender != null);
            var iEditor = iView.DataContext as TextPatternEditor;
            var iRule = iSender.DataContext as PatternRule;
            if (iRule != null && iRule.ToCalculate != null && iEditor != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    // if the ctrl is pressed, add, otherwise clear the selection islt.
                    iEditor.SelectedItems.Clear();
                }

                foreach (object i in iRule.ToCalculate)
                {
                    iEditor.SelectedItems.Add(i);
                }
            }

            if (iEditor.SelectedItem != null)
            {
                var iSelected = (TextPatternBase)iEditor.SelectedItem;
                if (iSelected.Selectionrange != null)
                {
                    // we make certain that the selectionrange don't contain any chars, cause then the copy doesn't behave as expected.
                    iSelected.Selectionrange = new SelectionRange()
                                                   {
                                                       Length = 0, 
                                                       Start = iSelected.Selectionrange.Start
                                                   };
                }

                iSelected.Focus(); // make certain that the first item is focused so that the copy paste works correctly.
            }

            e.Handled = true;
        }

        /// <summary>The grp to evaluate_ mouse double click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void GrpToEvaluate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var iSender = sender as Control;
            var iView = TreeHelper.FindInTree<TextPatternEditorView>(iSender);
            Debug.Assert(iSender != null);
            var iEditor = iView.DataContext as TextPatternEditor;
            var iRule = iSender.DataContext as PatternRule;
            if (iRule != null && iRule.ToEvaluate != null && iEditor != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    // if the ctrl is pressed, add, otherwise clear the selection islt.
                    iEditor.SelectedItems.Clear();
                }

                foreach (object i in iRule.ToEvaluate)
                {
                    iEditor.SelectedItems.Add(i);
                }
            }

            if (iEditor.SelectedItem != null)
            {
                var iSelected = (TextPatternBase)iEditor.SelectedItem;
                if (iSelected.Selectionrange != null)
                {
                    // we make certain that the selectionrange don't contain any chars, cause then the copy doesn't behave as expected.
                    iSelected.Selectionrange = new SelectionRange()
                                                   {
                                                       Length = 0, 
                                                       Start = iSelected.Selectionrange.Start
                                                   };
                }

                iSelected.Focus(); // make certain that the first item is focused so that the copy paste works correctly.
            }

            e.Handled = true;
        }

        /// <summary>Handles the MouseDoubleClick event of the GrpResponsesFor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void GrpResponsesFor_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var iSender = sender as Control;
            var iView = TreeHelper.FindInTree<TextPatternEditorView>(iSender);
            Debug.Assert(iSender != null);
            var iEditor = iView.DataContext as TextPatternEditor;
            var iGrp = iSender.DataContext as ResponsesForGroup;
            if (iGrp != null && iEditor != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    // if the ctrl is pressed, add, otherwise clear the selection islt.
                    iEditor.SelectedItems.Clear();
                }

                foreach (object i in iGrp.ResponseFor)
                {
                    iEditor.SelectedItems.Add(i);
                }
            }

            if (iEditor.SelectedItem != null)
            {
                var iSelected = (TextPatternBase)iEditor.SelectedItem;
                if (iSelected.Selectionrange != null)
                {
                    // we make certain that the selectionrange don't contain any chars, cause then the copy doesn't behave as expected.
                    iSelected.Selectionrange = new SelectionRange()
                                                   {
                                                       Length = 0, 
                                                       Start = iSelected.Selectionrange.Start
                                                   };
                }

                iSelected.Focus(); // make certain that the first item is focused so that the copy paste works correctly.
            }

            e.Handled = true;
        }

        /// <summary>When user presses delete or backspace on an empty item: delete the
        ///     record.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void TxtEditItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                var iSender = sender as TextBox;
                if (iSender != null && string.IsNullOrEmpty(iSender.Text) == true)
                {
                    var iText = iSender.DataContext as TextPatternBase;
                    Debug.Assert(iText != null);
                    var iParsable = iText as ParsableTextPatternBase;
                    if (iParsable != null)
                    {
                        // we call the parse to make certain that all the data is removed. doesn't need to be in undo recompile gets done after focus shift.
                        iParsable.Parse();
                    }

                    if (Neuron.IsEmpty(iText.Item.ID) == false)
                    {
                        iText.Delete();

                            // use delete, this makes certain that any 'atached' data like 'do' sections also is deleted (for output patterns).
                    }

                    if (e.Key == Key.Delete)
                    {
                        iSender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    }
                    else
                    {
                        iSender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>The nav from button_ preview key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NavFromButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (e.Key == Key.Up)
            {
                iSender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                iSender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                iSender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                iSender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                e.Handled = true;
            }
        }

        /// <summary>The navigation_ preview key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Navigation_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                var iSender = sender as TextBox;
                var iCurLine = iSender.GetLineIndexFromCharacterIndex(iSender.CaretIndex);
                if (iCurLine == 0)
                {
                    iSender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Down)
            {
                var iSender = sender as TextBox;
                var iCurLine = iSender.GetLineIndexFromCharacterIndex(iSender.CaretIndex);
                if (iCurLine == iSender.LineCount - 1)
                {
                    iSender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Left)
            {
                var iSender = sender as TextBox;
                if (iSender.CaretIndex == 0)
                {
                    iSender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Right)
            {
                var iSender = sender as TextBox;
                if (iSender.CaretIndex == iSender.Text.Length)
                {
                    iSender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    e.Handled = true;
                }
            }
        }

        /// <summary>Handles the Unloaded event of the PatterEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PatterEditor_Unloaded(object sender, EventArgs e)
        {
            if (fLastDataObject != null && fLastDataObject.IsDirty == true
                && // when we switch tabs, make certain that the last edited item is saved.
                fLastDataObject.Item != null && fLastDataObject.Item.ID != Neuron.EmptyId)
            {
                // we need to make certain that we don't try to assign a null string to the expression when it got deleted. During the deletion of an editor, it can be that it is unloaded. the unload gets called after the delete. If we would assign the expression, we would get invalid undo data, so don't
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    fLastDataObject.ParseWithUndo();
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }

            fLastDataObject = null; // can't have mem leaks.
        }

        /// <summary>The txt item_ is visible changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                if (fLastDataObject != null && fLastDataObject.IsDirty == true
                    && // when we collaps a list, make certain that the last edited item is saved.
                    fLastDataObject.Item != null && fLastDataObject.Item.ID != Neuron.EmptyId

                    // we need to make certain that we don't try to assign a null string to the expression when it got deleted. During the deletion of an editor, it can be that it is unloaded. the unload gets called after the delete. If we would assign the expression, we would get invalid undo data, so don't
                    && fLastDataObject.Item.IsDeleted == false && WindowMain.UndoStore.UndoStateStack.Count == 0)
                {
                    // when undoing/redoing don't try to do the parse, this is done automatically through the undo.
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        fLastDataObject.ParseWithUndo();
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>The pattern editor_ got keyboard focus.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void PatternEditor_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var iLastTextBox = sender as TextBox;
            if (iLastTextBox != null)
            {
                fLastDataObject = iLastTextBox.DataContext as ParsableTextPatternBase;
            }
            else
            {
                fLastDataObject = null;
            }
        }

        /// <summary>Handles the PreviewLostFocus event of the PatternEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyboardFocusChangedEventArgs"/> instance containing
        ///     the event data.</param>
        private void PatternEditor_PreviewLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            var iObj = iSender.DataContext as ParsableTextPatternBase;
            if (string.IsNullOrEmpty(iSender.Text) == false && WindowMain.Current.IsLoaded == true)
            {
                // when Main window is unloaded, don't want to reparse again, we are unloading.
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    var iBind = iSender.GetBindingExpression(TextBox.TextProperty);
                    if (iBind != null)
                    {
                        iBind.UpdateSource(); // we do this to make certain that the validation rule is run
                    }

                    if (iObj != null)
                    {
                        iObj.ParseWithUndo();
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
            else if (iObj != null && WindowMain.UndoStore.CurrentState == UndoSystem.UndoState.none)
            {
                // when only delete the item after loosing focus when we are in normal mode, when in add mode, we are trying to add some data
                iObj.Delete();
            }
        }

        /// <summary>A special LogFocus implemntation for the conditionals. main difference
        ///     with the regular lostFocus: we don't delete the object if it doesn't
        ///     have a value.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConditionPattern_PreviewLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var iSender = sender as TextBox;
            var iObj = iSender.DataContext as ParsableTextPatternBase;
            if (string.IsNullOrEmpty(iSender.Text) == false)
            {
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    var iBind = iSender.GetBindingExpression(TextBox.TextProperty);
                    if (iBind != null)
                    {
                        iBind.UpdateSource(); // we do this to make certain that the validation rule is run
                    }

                    if (iObj != null)
                    {
                        iObj.ParseWithUndo();
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>Handles the Executed event of the OutpuExpanderToggleCollaps control.
        ///     Toggles the output expander open and closed, so you can use the
        ///     keyboard.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void OutpuExpanderToggleCollaps_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iCond = iSender.DataContext as OutputPattern;
                if (iCond != null)
                {
                    iCond.HasInvalidResponses = !iCond.HasInvalidResponses;
                }
            }
        }

        /// <summary>The output do toggle collaps_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OutputDoToggleCollaps_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iCond = iSender.DataContext as OutputPattern;
                if (iCond != null)
                {
                    iCond.HasDo = !iCond.HasDo;
                }
            }
        }

        /// <summary>The invalid responses vis toggle collaps_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void InvalidResponsesVisToggleCollaps_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iCond = iSender.DataContext as OutputPattern;
                if (iCond != null)
                {
                    iCond.IsExpanded = !iCond.IsExpanded;
                }
            }
        }

        /// <summary>When the <see cref="SelectionRange"/> has changed for the object, we
        ///     need to adjust the UI so that the correct values are also selected.
        ///     Also, when this property is changed, we want to bring the item into
        ///     view. It's the best location to do this without creating extra
        ///     overhead.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataTransferEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtPattern_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Property == TextBox.TagProperty || e.Property == RuleNameTextBox.SelRangeProperty)
            {
                var iSender = sender as TextBox;
                if (iSender.IsLoaded == true)
                {
                    // if it's not yet loaded, the event wont be triggered, we handle this on the loaded event.
                    var iText = iSender.DataContext as RangedPatternEditorItem;
                    ApplySelectionRangeTo(iSender, iText);
                    iSender.BringIntoView();
                }
            }
        }

        /// <summary>The apply selection range to.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="pattern">The pattern.</param>
        private static void ApplySelectionRangeTo(TextBox sender, RangedPatternEditorItem pattern)
        {
            if (pattern != null)
            {
                if (pattern.Selectionrange != null)
                {
                    sender.Select(pattern.Selectionrange.Start, pattern.Selectionrange.Length);
                }
                else
                {
                    sender.Select(sender.CaretIndex, 0); // reset the selection null
                }

                var iRoot = pattern.Root;
                try
                {
                    // put this between a try-catch cause the 'focus' function can throw an objec == null exception when the window is no longer visible (like when it is being dragged.
                    if (iRoot != null && pattern.Root.SelectedItem == pattern)
                    {
                        // if it is the first item, make it focused. root can be null for REsponseForOutput items, when a new value is selected from dropdown box
                        sender.Focus();
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>When the selection range of the UI item has changed, we need to update
        ///     the data object as well.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtPattern_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender.IsLoaded == true)
            {
                var iText = iSender.DataContext as RangedPatternEditorItem;
                if (iText != null)
                {
                    if (iText.Selectionrange != null)
                    {
                        iText.Selectionrange.Start = iSender.SelectionStart;
                        iText.Selectionrange.Length = iSender.SelectionLength;
                    }
                    else
                    {
                        var iRange = new SelectionRange()
                                         {
                                             Length = iSender.SelectionLength, 
                                             Start = iSender.SelectionStart
                                         };
                        iText.SetSelectionrange(iRange);

                            // set without propertychanged, this would trigger this function again.
                    }
                }
            }
        }

        /// <summary>Handles the Executed event of the DoPatternsToggleCollaps control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DoPatternsToggleCollaps_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iCond = iSender.DataContext as PatternRuleOutput;
                if (iCond != null)
                {
                    iCond.IsDoPatternVisible = !iCond.IsDoPatternVisible;
                }
            }
        }

        /// <summary>Handles the Executed event of the DoPatternsToggleCollaps control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void OutputDoPatternsToggleCollaps_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iCond = iSender.DataContext as OutputPattern;
                if (iCond != null)
                {
                    iCond.IsDoExpanded = !iCond.IsDoExpanded;
                }
            }
        }

        /// <summary>Handles the Executed event of the DoPatternsToggleCollaps control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void HasDoPatterns_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iCond = iSender.DataContext as PatternRuleOutput;
                if (iCond != null)
                {
                    iCond.HasDo = !iCond.HasDo;
                }
            }
        }

        /// <summary>The to calculate toggle collaps_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ToCalculateToggleCollaps_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iCond = iSender.DataContext as PatternRule;
                if (iCond != null)
                {
                    iCond.IsToCalculateVisible = !iCond.IsToCalculateVisible;
                }
            }
        }

        /// <summary>The togle has to calculate_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TogleHasToCalculate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iCond = iSender.DataContext as PatternRule;
                if (iCond != null)
                {
                    iCond.HasToCal = !iCond.HasToCal;
                }
            }
        }

        /// <summary>The has to evaluate toggle collaps_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void HasToEvaluateToggleCollaps_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iCond = iSender.DataContext as PatternRule;
                if (iCond != null)
                {
                    iCond.HasToEval = !iCond.HasToEval;
                }
            }
        }

        /// <summary>The to evaluate toggle collaps_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ToEvaluateToggleCollaps_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iCond = iSender.DataContext as PatternRule;
                if (iCond != null)
                {
                    iCond.IsToEvaluateVisible = !iCond.IsToEvaluateVisible;
                }
            }
        }

        /// <summary>Called whenever the text is about to be changed in one of the input
        ///     boxes. When the text is different from the underlying object (so in
        ///     the midst of editing), we store undo data.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iSender = e.OriginalSource as TextBox;
            if (iSender != null)
            {
                var iObj = iSender.DataContext as TextPatternBase;
                if (iObj != null && iObj.Expression != iSender.Text)
                {
                    iObj.NeuronInfo.DisplayTitle = iSender.Text;

                        // this generates undo data, stores the info, but doesn't force a compile
                }
            }
        }

        /// <summary>Handles the CanExecute event of the ConditionDelete control. This is
        ///     to prevent deletion of the conditional neuron, which causes errors.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ConditionDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        /// <summary>Handles the ContextMenuOpening event of the txtEdit control. Fills the
        ///     menu item with all possible suggestions.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ContextMenuEventArgs"/> instance containing the event
        ///     data.</param>
        private void txtEdit_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var index = 0;
            var iTxtBox = sender as TextBox;
            if (iTxtBox != null)
            {
                var iMenu = iTxtBox.ContextMenu;
                var iSuggestions = iMenu.Items[0] as MenuItem;

                    // get the menu item that contains all the spelling suggestions.
                iSuggestions.Items.Clear(); // Clearing the existing items
                var iErrors = iTxtBox.GetSpellingError(iTxtBox.CaretIndex);
                if (iErrors != null && iErrors.Suggestions.Count() >= 1)
                {
                    iSuggestions.IsEnabled = true;
                    if (iMenu.Items.Count > 3)
                    {
                        // 'add to dict' and 'ignore all' should also not be enabled when no errors. 
                        ((MenuItem)iMenu.Items[1]).IsEnabled = true;
                        ((MenuItem)iMenu.Items[2]).IsEnabled = true;
                    }

                    // Creating the suggestions menu items.
                    foreach (var suggestion in iErrors.Suggestions)
                    {
                        var menuItem = new MenuItem();
                        menuItem.Header = suggestion;
                        menuItem.Command = EditingCommands.CorrectSpellingError;
                        menuItem.CommandParameter = suggestion;
                        menuItem.CommandTarget = iTxtBox;
                        iSuggestions.Items.Insert(index, menuItem);
                        index++;
                    }
                }
                else
                {
                    iSuggestions.IsEnabled = false; // only enable menu item if there really are suggestions.
                    if (iErrors == null && iMenu.Items.Count > 3)
                    {
                        // 'add to dict' and 'ignore all' should also not be enabled when no errors. 
                        ((MenuItem)iMenu.Items[1]).IsEnabled = false;
                        ((MenuItem)iMenu.Items[2]).IsEnabled = false;
                    }
                }
            }
        }

        /// <summary>Handles the Click event of the IgnoreAll control. called when a word
        ///     needs to be ignored, which is done by adding it to the ignore list
        ///     (file).</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void IgnoreAll_Click(object sender, RoutedEventArgs e)
        {
            var iSender = sender as MenuItem;
            var iMenu = iSender.Parent as ContextMenu;
            var iTxtBox = iMenu.PlacementTarget as TextBox;
            if (iTxtBox != null)
            {
                var iStart = iTxtBox.GetSpellingErrorStart(iTxtBox.CaretIndex);
                var iText = iTxtBox.Text.Substring(iStart, iTxtBox.GetSpellingErrorLength(iTxtBox.CaretIndex));
                using (
                    var iWrite =
                        new StreamWriter(
                            System.IO.Path.Combine(
                                Designer.Properties.Settings.Default.CustomSpellingDictsPath, 
                                Designer.Properties.Resources.IgnoreAllDict), 
                            true)) iWrite.WriteLine(iText);
                var iView = TreeHelper.FindInTree<TextPatternEditorView>(iTxtBox);
                if (iView != null)
                {
                    iView.RequestUpdateSpelling();
                }
            }
        }

        /// <summary>Handles the Click event of the AddToDict control. called when a word
        ///     needs to be added to the custom dict</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AddToDict_Click(object sender, RoutedEventArgs e)
        {
            var iSender = sender as MenuItem;
            var iMenu = iSender.Parent as ContextMenu;
            var iTxtBox = iMenu.PlacementTarget as TextBox;
            if (iTxtBox != null)
            {
                var iStart = iTxtBox.GetSpellingErrorStart(iTxtBox.CaretIndex);
                var iText = iTxtBox.Text.Substring(iStart, iTxtBox.GetSpellingErrorLength(iTxtBox.CaretIndex));
                using (
                    var iWrite =
                        new StreamWriter(
                            System.IO.Path.Combine(
                                Designer.Properties.Settings.Default.CustomSpellingDictsPath, 
                                Designer.Properties.Resources.CustomSpellingDict), 
                            true)) iWrite.WriteLine(iText);
                var iView = TreeHelper.FindInTree<TextPatternEditorView>(iTxtBox);
                if (iView != null)
                {
                    iView.RequestUpdateSpelling();
                }
            }
        }

        /// <summary>Lists all the 'ResponseFor' items that link to the selected output
        ///     pattern.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FindReferences_Click(object sender, RoutedEventArgs e)
        {
            var iSender = sender as MenuItem;
            var iMenu = iSender.Parent as ContextMenu;
            var iTxtBox = iMenu.PlacementTarget as FrameworkElement;
            var iPattern = iTxtBox.DataContext as OutputPattern;
            if (iPattern != null)
            {
                TextFindHelpers.ListAllResponseForRefsForOutput(iPattern.Item, iPattern.Expression);
            }
        }

        /// <summary>Lists all the output patterns that the selected 'REsponseFor' refers
        ///     to.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FindRefsToResponse_Click(object sender, RoutedEventArgs e)
        {
            var iSender = sender as MenuItem;
            var iMenu = iSender.Parent as ContextMenu;
            var iCombo = iMenu.PlacementTarget as FrameworkElement;
            var iPattern = iCombo.DataContext as ResponseForOutput;
            if (iPattern != null)
            {
                TextFindHelpers.ListAllOutputsForResponseForRef(iPattern.Item, iPattern.Expression);
            }
        }

        /// <summary>The go to output of response_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void GoToOutputOfResponse_Click(object sender, RoutedEventArgs e)
        {
            var iSender = sender as MenuItem;
            var iMenu = iSender.Parent as ContextMenu;
            var iCombo = iMenu.PlacementTarget as FrameworkElement;
            var iPattern = iCombo.DataContext as ResponseForOutput;
            if (iPattern != null)
            {
                TextFindHelpers.GoToOutput(iPattern.Item);
            }
        }

        /// <summary>The focus pointer.</summary>
        internal struct FocusPointer
        {
            /// <summary>
            ///     a <see langword="ref" /> to the object (so we can distinguish
            ///     between the same field in a list of items).
            /// </summary>
            public object Item;

            /// <summary>
            ///     the name of the property that needs to get focus
            /// </summary>
            public string PropName;
        }

        #region paste on ResponseFor

        /// <summary>Handles the Executed event of the PasteResponseFor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void PasteResponseFor_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iOut = iSender.DataContext as ResponsesForGroup;
                if (iOut != null)
                {
                    EditorsHelper.PasteOutputsToResonponseFor(iOut, null);
                }
                else
                {
                    var iResponse = iSender.DataContext as ResponseForOutput;
                    if (iResponse != null)
                    {
                        EditorsHelper.PasteOutputsToResonponseFor((ResponsesForGroup)iResponse.Owner, iResponse);
                    }
                }
            }
        }

        /// <summary>Handles the CanExecute event of the PasteResponseFor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void PasteResponseFor_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT);
            e.Handled = true;
        }

        #endregion

        #region Paste on new input pattern

        /// <summary>Handles the Executed event of the PasteResponseFor control. Before we
        ///     can do the paste, we need to create a new rule.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewInputPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        var iNew = AddNewRule((FrameworkElement)iView);

                            // this will select it, so that the paste of the editor works.
                        var iEditor = iSender.DataContext as TextPatternEditor;
                        if (iEditor == null)
                        {
                            // when in master-detail view, this can be null.
                            iEditor = (TextPatternEditor)((FrameworkElement)iView).DataContext;
                        }

                        Debug.Assert(iEditor != null);
                        iView.CurrentEditMode = EditMode.AddPattern;

                            // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                        iEditor.PasteFromClipboard();
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>Handles paste command on existing input patterns.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InputPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iInput = iSender.DataContext as InputPattern;
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null && iInput != null && iInput.Rule != null)
                {
                    var iPasteHandler = ((FrameworkElement)iView).DataContext as ITextPatternPasteHandler;
                    if (iPasteHandler != null)
                    {
                        iView.CurrentEditMode = EditMode.AddPattern;

                            // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                        iPasteHandler.PasteFromClipboardToList(iInput.Rule.TextPatterns, iInput);
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>The output paste_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void OutputPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iOutput = iSender.DataContext as OutputPattern;
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null && iOutput != null)
                {
                    var iPasteHandler = ((FrameworkElement)iView).DataContext as ITextPatternPasteHandler;
                    if (iPasteHandler != null)
                    {
                        if (iOutput.RuleOutput != null)
                        {
                            iView.CurrentEditMode = EditMode.AddOutput;

                                // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                            iPasteHandler.PasteFromClipboardToList(iOutput.RuleOutput.Outputs, iOutput);
                        }
                        else
                        {
                            var iProps = iPasteHandler as ChatbotProperties;
                            if (iProps != null)
                            {
                                iView.CurrentEditMode = EditMode.AddInvalid;

                                    // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                                if (iProps.ConversationStarts.Contains(iOutput) == true)
                                {
                                    iPasteHandler.PasteFromClipboardToList(iProps.ConversationStarts, iOutput);
                                }
                                else if (iProps.FallBacks.Contains(iOutput) == true)
                                {
                                    iPasteHandler.PasteFromClipboardToList(iProps.FallBacks, iOutput);
                                }
                                else if (iProps.Context.Contains(iOutput) == true)
                                {
                                    iPasteHandler.PasteFromClipboardToList(iProps.Context, iOutput);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Can't find the list to paste the data in.");
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException("Can't find the list to paste the data in.");
                            }
                        }

                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>The invalid paste_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void InvalidPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iInvalid = iSender.DataContext as InvalidPatternResponse;
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null && iInvalid != null && iInvalid.Output != null)
                {
                    var iPasteHandler = ((FrameworkElement)iView).DataContext as ITextPatternPasteHandler;
                    if (iPasteHandler != null)
                    {
                        iView.CurrentEditMode = EditMode.AddInvalid;

                            // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                        iPasteHandler.PasteFromClipboardToList(iInvalid.Output.InvalidResponses, iInvalid);
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>Handles the Executed event of the NewOutputPaste control. Before we
        ///     can do the paste, we need to create a new rule. Also need to make
        ///     certain that we are pasting to the outputs list of the new rule.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewOutputPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        var iNew = AddNewRule((FrameworkElement)iView);

                            // this will select it, so that the paste of the editor works.
                        var iEditor = iSender.DataContext as TextPatternEditor;
                        Debug.Assert(iEditor != null);
                        iView.CurrentEditMode = EditMode.AddOutput;

                            // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                        iEditor.PasteFromClipboardToList(iNew.Outputs);
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>Handles the Executed event of the NewCondOutputPaste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewCondOutputPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    var iRuleOutput = iSender.DataContext as PatternRuleOutput;
                    Debug.Assert(iRuleOutput != null);
                    var iPasteHandler = ((FrameworkElement)iView).DataContext as ITextPatternPasteHandler;
                    if (iPasteHandler != null)
                    {
                        iView.CurrentEditMode = EditMode.AddOutput;

                            // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                        iPasteHandler.PasteFromClipboardToList(iRuleOutput.Outputs);
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>The new condition paste_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NewConditionPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        iView.CurrentEditMode = EditMode.AddConditional;

                            // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                        var iCond = iSender.DataContext as PatternEditorItem;
                        if (iCond == null)
                        {
                            iCond =
                                ((FrameworkElement)((FrameworkElement)iSender.TemplatedParent).Parent).DataContext as
                                PatternEditorItem;
                        }

                        if (iCond != null)
                        {
                            // conditionals (on questions) sometimes don't have any data (null), in which case we need to go to the parent object.
                            var iEditor = iCond.Root as TextPatternEditor;
                            if (iCond.Rule != null)
                            {
                                // if there is no rule but there is an editor,  it could be for the questions
                                if (iEditor != null)
                                {
                                    iEditor.PasteFromClipboardToListAsCond(iCond.Rule.Conditionals, iCond.RuleOutput);
                                }
                                else
                                {
                                    var iProps = ((FrameworkElement)iSender).DataContext as ChatbotProperties;
                                    iProps.PasteFromClipboardToListAsCond();
                                }
                            }
                            else
                            {
                                if (iEditor != null)
                                {
                                    iEditor.PasteFromClipboardToListAsCond(iEditor.Questions, iCond.RuleOutput);

                                        // we add the extra parameter, so we insert at the correct position.
                                }
                                else
                                {
                                    var iProps = iCond.Root as ChatbotProperties;
                                    iProps.PasteFromClipboardToListAsCond(iCond.RuleOutput);
                                }
                            }
                        }
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>The new invalid paste_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NewInvalidPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    var iPasteHandler = ((FrameworkElement)iView).DataContext as ITextPatternPasteHandler;
                    if (iPasteHandler != null)
                    {
                        var iRuleOutput = iSender.DataContext as OutputPattern;
                        Debug.Assert(iRuleOutput != null);
                        iView.CurrentEditMode = EditMode.AddInvalid;

                            // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                        iPasteHandler.PasteFromClipboardToList(iRuleOutput.InvalidResponses);
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>Handles the CanExecute event of the PasteResponseFor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewInputPaste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iEditor = iSender.DataContext as TextPatternEditor;
                if (iEditor != null)
                {
                    e.CanExecute = iEditor.CanPasteFromClipboard();
                }
                else
                {
                    var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                    if (iView != null)
                    {
                        var iProps = ((FrameworkElement)iView).DataContext as ChatbotProperties;
                        e.CanExecute = iProps.CanPasteFromClipboard();
                    }
                    else
                    {
                        e.CanExecute = false;
                    }
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>The new sub paste_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NewSubPaste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var iSender = sender as FrameworkElement;
            if (iSender != null)
            {
                var iItem = iSender.DataContext as PatternEditorItem;
                if (iItem != null)
                {
                    // this is for the conditionals in the questions, they can be null, in which case we need to use a slower technique.
                    var iEditor = iItem.Root as TextPatternEditor;
                    if (iEditor != null)
                    {
                        e.CanExecute = iEditor.CanPasteFromClipboard();
                        return;
                    }
                }

                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    var iPasteHandler = ((FrameworkElement)iView).DataContext as ITextPatternPasteHandler;
                    if (iPasteHandler != null)
                    {
                        e.CanExecute = iPasteHandler.CanPasteFromClipboard();

                        // e.Handled = true;
                        return;
                    }
                }
            }

            e.CanExecute = false;
        }

        /// <summary>Handles the Executed event of the NewFallbackPaste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewFallbackPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    var iProps = ((FrameworkElement)iView).DataContext as ChatbotProperties;
                    if (iProps != null)
                    {
                        iView.CurrentEditMode = EditMode.AddInvalid;

                            // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                        iProps.PasteFromClipboardToList(iProps.FallBacks);
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>The new topic filter paste_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NewTopicFilterPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    var iProps = ((FrameworkElement)iView).DataContext as TextPatternEditor;
                    if (iProps != null)
                    {
                        iView.CurrentEditMode = EditMode.AddTopicFilter;

                            // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                        iProps.PasteFromClipboardToList(iProps.TopicFilters);
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>Handles the Executed event of the NewStartPaste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewStartPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    iView.CurrentEditMode = EditMode.AddInvalid;

                        // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                    var iProps = ((FrameworkElement)iView).DataContext as ChatbotProperties;
                    if (iProps != null)
                    {
                        iProps.PasteFromClipboardToList(iProps.ConversationStarts);
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>Handles the Executed event of the NewStartPaste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewContextPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    iView.CurrentEditMode = EditMode.AddInvalid;

                        // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                    var iProps = ((FrameworkElement)iView).DataContext as ChatbotProperties;
                    if (iProps != null)
                    {
                        if (iProps.Context == null)
                        {
                            throw new InvalidOperationException(
                                "Can't extract the context cluster from the network, please specify the chatbot-mappings.");
                        }

                        iProps.PasteFromClipboardToList(iProps.Context);
                    }

                    e.Handled = true;
                }
            }
        }

        ///// <summary>
        ///// Handles the Executed event of the NewPropsDoPaste control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        // void NewPropsDoPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        // {
        // TextBox iSender = sender as TextBox;
        // if (iSender != null)
        // {
        // ITextPatternEditorView iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
        // if (iView != null)
        // {
        // iView.CurrentEditMode = EditMode.AddDo;                                      //this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
        // ChatbotProperties iProps = ((FrameworkElement)iView).DataContext as ChatbotProperties;
        // DoPatternCollection iList = iSender.DataContext as DoPatternCollection;
        // if (iProps != null)
        // iProps.PasteFromClipboardToList(iList);
        // e.Handled = true;
        // }
        // }
        // }

        /// <summary>The new repeat paste_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NewRepeatPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        var iProps = ((FrameworkElement)iView).DataContext as ChatbotProperties;
                        var iNew = EditorsHelper.AddNewConditionalToPattern(iProps.ResponsesOnRepeat);
                        EditorsHelper.AddNewCondition(iNew, string.Empty);

                            // we create an empty condition, so that there always is one, otherwise, the editor wont have one and cant create one
                        Debug.Assert(iProps != null);
                        iProps.PasteFromClipboardToList(iNew.Outputs);
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>The new question paste_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NewQuestionPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        var iEditor = iSender.DataContext as TextPatternEditor;
                        var iNew = EditorsHelper.AddNewConditionalToPattern(iEditor.Questions);
                        EditorsHelper.AddNewCondition(iNew, string.Empty);

                            // we create an empty condition, so that there always is one, otherwise, the editor wont have one and cant create one
                        Debug.Assert(iEditor != null);
                        iEditor.PasteFromClipboardToList(iNew.Outputs);
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>The new question cond paste_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NewQuestionCondPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        var iEditor = iSender.DataContext as TextPatternEditor;
                        if (iEditor != null)
                        {
                            iEditor.PasteFromClipboardToListAsCond(iEditor.Questions);
                        }
                        else
                        {
                            var iProps = ((FrameworkElement)iView).DataContext as ChatbotProperties;
                            iProps.PasteFromClipboardToListAsCond();
                        }
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>Handles the Executed event of the NewDoPaste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewSubInputPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var iSender = sender as TextBox;
            if (iSender != null)
            {
                var iView = TreeHelper.FindInTree<ITextPatternEditorView>(iSender);
                if (iView != null)
                {
                    iView.CurrentEditMode = EditMode.AddPattern;

                        // this is required for moving focus, so that the first inserted item gets focus. If we don't do this, the selected item gets out of whack cause the wrong object is focused.
                    var iRule = iSender.DataContext as PatternRule;
                    Debug.Assert(iRule != null);
                    var iEditor = iRule.Root as TextPatternEditor;
                    if (iEditor != null)
                    {
                        iEditor.PasteFromClipboardToList(iRule.TextPatterns);
                    }

                    e.Handled = true;
                }
            }
        }

        #endregion
    }
}