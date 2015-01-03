// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CharacterWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CharacterWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CharacterWindow.xaml
    /// </summary>
    public partial class CharacterWindow : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="CharacterWindow"/> class.</summary>
        public CharacterWindow()
        {
            InitializeComponent();
        }

        /// <summary>The thumb window mover_ drag delta.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ThumbWindowMover_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Left += e.HorizontalChange;
            Top += e.VerticalChange;
        }
    }
}