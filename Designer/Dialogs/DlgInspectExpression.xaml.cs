// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgInspectExpression.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgInspectExpression.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgInspectExpression.xaml
    /// </summary>
    public partial class DlgInspectExpression : System.Windows.Window
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DlgInspectExpression"/> class. Initializes a new instance of the <see cref="DlgInspectExpression"/>
        ///     class.</summary>
        /// <param name="expression">The expression.</param>
        /// <param name="items">The items.</param>
        public DlgInspectExpression(ResultExpression expression, System.Collections.Generic.List<DebugNeuron> items)
        {
            if (expression != null)
            {
                // can be used to display a debugneuron, in which case we don't have an expression.
                Expression = new DebugNeuron(expression);
            }

            Items = items;
            InitializeComponent();
            GlobalCommands.Register(this);
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of items that were returned by the expression.
        /// </summary>
        public System.Collections.Generic.List<DebugNeuron> Items { get; internal set; }

        #endregion

        #region Expression

        /// <summary>
        ///     Gets the expression for whom we display all the data.
        /// </summary>
        public DebugNeuron Expression { get; internal set; }

        #endregion

        /// <summary>Handles the Closing event of the self control.</summary>
        /// <remarks>We close all the debug neurons by assign <see langword="null"/> to
        ///     them. This makes certain that they are unloaded from the designer as
        ///     fast as possible, releasing resources.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        ///     containing the event data.</param>
        private void self_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Expression != null)
            {
                Expression.Item = null;
            }

            foreach (var i in Items)
            {
                i.Item = null;
            }

            Expression = null; // release all resources as soon as possible
            Items = null; // release all resources as soon as possible
        }
    }
}