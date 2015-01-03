// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgStringQuestion.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgStringQuestion.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgStringQuestion.xaml
    /// </summary>
    public partial class DlgStringQuestion : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgStringQuestion"/> class.</summary>
        public DlgStringQuestion()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the answer.</summary>
        public string Answer { get; set; }

        /// <summary>The calculate answer.</summary>
        private void CalculateAnswer()
        {
            var iNew = TxtAnswer.Text;
            if (iNew != null)
            {
                var iBuild = new System.Text.StringBuilder();
                var iRes = iNew.Split('\\');
                var iPrevEscape = false;
                foreach (var i in iRes)
                {
                    if (i.Length == 0)
                    {
                        if (iPrevEscape)
                        {
                            iBuild.Append("\\");
                            iPrevEscape = false;
                        }
                        else
                        {
                            iPrevEscape = true;
                        }
                    }
                    else
                    {
                        if (iPrevEscape)
                        {
                            int iVal;
                            if (int.TryParse(i, out iVal))
                            {
                                iBuild.Append((char)iVal);
                            }
                            else
                            {
                                iBuild.Append("\\");
                                iBuild.Append(i);
                            }
                        }
                        else
                        {
                            iBuild.Append(i);
                        }
                    }
                }

                Answer = iBuild.ToString();
            }
            else
            {
                Answer = iNew;
            }
        }

        /// <summary>The on click ok.</summary>
        /// <param name="aSender">The a sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickOk(object aSender, System.EventArgs e)
        {
            CalculateAnswer();
            DialogResult = true;
        }

        /// <summary>The the window_ loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TheWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtAnswer.Text = Answer;
        }

        #region Question

        /// <summary>
        ///     Identifies the <see cref="Question" /> Dependency property.
        /// </summary>
        public static readonly System.Windows.DependencyProperty QuestionProperty =
            System.Windows.DependencyProperty.Register(
                "Question", 
                typeof(string), 
                typeof(DlgStringQuestion), 
                new System.Windows.FrameworkPropertyMetadata(null));

        /// <summary>
        ///     Gets/Sets the Question.
        /// </summary>
        public string Question
        {
            get
            {
                return (string)GetValue(QuestionProperty);
            }

            set
            {
                SetValue(QuestionProperty, value);
            }
        }

        #endregion
    }
}