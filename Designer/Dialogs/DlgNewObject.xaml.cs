// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgNewObject.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgNewObject.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for DlgNewObject.xaml
    /// </summary>
    public partial class DlgNewObject : System.Windows.Window
    {
        #region Fields

        /// <summary>The f prev values.</summary>
        private System.Collections.ObjectModel.ObservableCollection<Conjugation> fPrevValues;

        #endregion

        /// <summary>Initializes a new instance of the <see cref="DlgNewObject"/> class.</summary>
        public DlgNewObject()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Click event of the Ok control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Ok_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Description = System.Windows.Markup.XamlWriter.Save(RtfDesc.Document);
            if (PosGroupsEnabled == false)
            {
                CreatePOSGroup = false;
            }

            DialogResult = true;
        }

        #region internal types

        /// <summary>The conjugation.</summary>
        public class Conjugation
        {
            /// <summary>
            ///     Gets or sets the name of the conjugation. Ex (for verb swim): swam
            /// </summary>
            /// <value>
            ///     The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            ///     Gets or sets the meaning, or conjugation to create. ex (for verb
            ///     swim): past
            /// </summary>
            /// <value>
            ///     The meaning.
            /// </value>
            public ulong Meaning { get; set; }
        }

        #endregion

        #region Prop

        #region Name

        /// <summary>
        ///     <see cref="Name" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty NameProperty =
            System.Windows.DependencyProperty.Register(
                "Name", 
                typeof(string), 
                typeof(DlgNewObject), 
                new System.Windows.FrameworkPropertyMetadata(null, OnNameChanged));

        /// <summary>
        ///     Gets or sets the <see cref="Name" /> property. This dependency property
        ///     indicates the name that should be used for the object. This is also
        ///     the value for the textneuron.
        /// </summary>
        public string Name
        {
            get
            {
                return (string)GetValue(NameProperty);
            }

            set
            {
                SetValue(NameProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="Name"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnNameChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((DlgNewObject)d).OnNameChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="Name"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnNameChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iPos = POS;
            if (iPos != null
                && (iPos.ID == (ulong)PredefinedNeurons.Verb || iPos.ID == (ulong)PredefinedNeurons.Adjective
                    || iPos.ID == (ulong)PredefinedNeurons.Noun))
            {
                PreFillConjugations();
            }
        }

        #endregion

        #region IncludeMeaning

        /// <summary>
        ///     <see cref="IncludeMeaning" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IncludeMeaningProperty =
            System.Windows.DependencyProperty.Register(
                "IncludeMeaning", 
                typeof(bool), 
                typeof(DlgNewObject), 
                new System.Windows.FrameworkPropertyMetadata(false));

        /// <summary>
        ///     Gets or sets the <see cref="IncludeMeaning" /> property. This
        ///     dependency property indicates if the object should have a meaning
        ///     cluster.
        /// </summary>
        public bool IncludeMeaning
        {
            get
            {
                return (bool)GetValue(IncludeMeaningProperty);
            }

            set
            {
                SetValue(IncludeMeaningProperty, value);
            }
        }

        #endregion

        #region Conjugations

        /// <summary>
        ///     <see cref="Conjugations" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ConjugationsProperty =
            System.Windows.DependencyProperty.Register(
                "Conjugations", 
                typeof(System.Collections.ObjectModel.ObservableCollection<Conjugation>), 
                typeof(DlgNewObject), 
                new System.Windows.FrameworkPropertyMetadata(
                    new System.Collections.ObjectModel.ObservableCollection<Conjugation>()));

        /// <summary>
        ///     Gets or sets the <see cref="Conjugations" /> property. This dependency
        ///     property indicates the list of conjugations
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<Conjugation> Conjugations
        {
            get
            {
                return (System.Collections.ObjectModel.ObservableCollection<Conjugation>)GetValue(ConjugationsProperty);
            }

            set
            {
                SetValue(ConjugationsProperty, value);
            }
        }

        #endregion

        #region AsDummy

        /// <summary>
        ///     <see cref="AsDummy" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty AsDummyProperty =
            System.Windows.DependencyProperty.Register(
                "AsDummy", 
                typeof(bool), 
                typeof(DlgNewObject), 
                new System.Windows.FrameworkPropertyMetadata(false, OnAsDummyChanged));

        /// <summary>
        ///     Gets or sets the <see cref="AsDummy" /> property. This dependency
        ///     property indicates if the object should contain a textneuron or not.
        /// </summary>
        public bool AsDummy
        {
            get
            {
                return (bool)GetValue(AsDummyProperty);
            }

            set
            {
                SetValue(AsDummyProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="AsDummy"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnAsDummyChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((DlgNewObject)d).OnAsDummyChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="AsDummy"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnAsDummyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            CoerceValue(PosGroupsEnabledProperty);
        }

        #endregion

        #region CreatePOSGroup

        /// <summary>
        ///     <see cref="CreatePOSGroup" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CreatePOSGroupProperty =
            System.Windows.DependencyProperty.Register(
                "CreatePOSGroup", 
                typeof(bool), 
                typeof(DlgNewObject), 
                new System.Windows.FrameworkPropertyMetadata(true));

        /// <summary>
        ///     Gets or sets the <see cref="CreatePOSGroup" /> property. This
        ///     dependency property indicates if a posgroup needs to be created or
        ///     not.
        /// </summary>
        public bool CreatePOSGroup
        {
            get
            {
                return (bool)GetValue(CreatePOSGroupProperty);
            }

            set
            {
                SetValue(CreatePOSGroupProperty, value);
            }
        }

        #endregion

        #region POS

        /// <summary>
        ///     <see cref="POS" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty POSProperty =
            System.Windows.DependencyProperty.Register(
                "POS", 
                typeof(Neuron), 
                typeof(DlgNewObject), 
                new System.Windows.FrameworkPropertyMetadata(null, OnPOSChanged));

        /// <summary>
        ///     Gets or sets the <see cref="POS" /> property. This dependency property
        ///     indicates the neuron that indicates the part of speech of the new
        ///     object.
        /// </summary>
        public Neuron POS
        {
            get
            {
                return (Neuron)GetValue(POSProperty);
            }

            set
            {
                SetValue(POSProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="POS"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnPOSChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((DlgNewObject)d).OnPOSChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="POS"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnPOSChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iNew = e.NewValue as Neuron;
            var iOld = e.OldValue as Neuron;
            if (iNew != null
                && (iNew.ID == (ulong)PredefinedNeurons.Noun || iNew.ID == (ulong)PredefinedNeurons.Verb
                    || iNew.ID == (ulong)PredefinedNeurons.Adjective))
            {
                ConjugationVisibility = System.Windows.Visibility.Visible;
                if (fPrevValues != null)
                {
                    Conjugations = fPrevValues;
                }
                else if (Conjugations.Count == 0)
                {
                    PreFillConjugations();
                }

                CoerceValue(PosGroupsEnabledProperty);
            }
            else if (iOld != null
                     && (iOld.ID == (ulong)PredefinedNeurons.Noun || iOld.ID == (ulong)PredefinedNeurons.Verb
                         || iOld.ID == (ulong)PredefinedNeurons.Adjective))
            {
                ConjugationVisibility = System.Windows.Visibility.Collapsed;
                fPrevValues = Conjugations;

                    // we keep track of the previous values, but reset the current conjugations so that the caller of the dialog doesn't get confused.
                Conjugations = null;
                CoerceValue(PosGroupsEnabledProperty);
            }
        }

        #endregion

        #region ConjugationVisibility

        /// <summary>
        ///     <see cref="ConjugationVisibility" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ConjugationVisibilityProperty =
            System.Windows.DependencyProperty.Register(
                "ConjugationVisibility", 
                typeof(System.Windows.Visibility), 
                typeof(DlgNewObject), 
                new System.Windows.FrameworkPropertyMetadata(System.Windows.Visibility.Collapsed));

        /// <summary>
        ///     Gets or sets the <see cref="ConjugationVisibility" /> property. This
        ///     dependency property indicates the visibility of the conjugations
        ///     section.
        /// </summary>
        public System.Windows.Visibility ConjugationVisibility
        {
            get
            {
                return (System.Windows.Visibility)GetValue(ConjugationVisibilityProperty);
            }

            set
            {
                SetValue(ConjugationVisibilityProperty, value);
            }
        }

        #endregion

        #region PosGroupsEnabled

        /// <summary>
        ///     <see cref="PosGroupsEnabled" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty PosGroupsEnabledProperty =
            System.Windows.DependencyProperty.Register(
                "PosGroupsEnabled", 
                typeof(bool), 
                typeof(DlgNewObject), 
                new System.Windows.FrameworkPropertyMetadata(true, null, CoercePosGroupsEnabledValue));

        /// <summary>
        ///     Gets or sets the <see cref="PosGroupsEnabled" /> property. This
        ///     dependency property indicates if the posgroup should be enabled or
        ///     not.
        /// </summary>
        public bool PosGroupsEnabled
        {
            get
            {
                return (bool)GetValue(PosGroupsEnabledProperty);
            }

            set
            {
                SetValue(PosGroupsEnabledProperty, value);
            }
        }

        /// <summary>Coerces the <see cref="PosGroupsEnabled"/> value.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        private static object CoercePosGroupsEnabledValue(System.Windows.DependencyObject d, object value)
        {
            var iSender = d as DlgNewObject;
            var iPos = iSender.POS;
            if (iSender.AsDummy || (iPos != null && iPos.ID == (ulong)PredefinedNeurons.Verb))
            {
                return false;
            }

            return value;
        }

        #endregion

        #region Description

        /// <summary>
        ///     Gets the description that was provided by the user.
        /// </summary>
        public string Description { get; internal set; }

        #endregion

        #endregion

        #region render Conjugations

        /// <summary>
        ///     Tries to create all the conjugations, based on some rules and the
        ///     existence of some links.
        /// </summary>
        private void PreFillConjugations()
        {
            Conjugations.Clear();
            if (string.IsNullOrEmpty(Name) == false)
            {
                var iPos = POS;
                if (iPos != null)
                {
                    if (iPos.ID == (ulong)PredefinedNeurons.Verb)
                    {
                        RenderPresentParticle();
                        RenderPastParticle();
                        RenderPastTense();
                        RenderThirdPerson();
                    }
                    else if (iPos.ID == (ulong)PredefinedNeurons.Adjective)
                    {
                        RenderComparetive();
                        RenderSuperlative();
                    }
                    else if (iPos.ID == (ulong)PredefinedNeurons.Noun)
                    {
                        RenderPlural();
                    }
                }
            }
        }

        /// <summary>The render plural.</summary>
        private void RenderPlural()
        {
            var iFound = (from i in BrainData.Current.Thesaurus.ConjugationMeanings
                          where i.NeuronInfo.DisplayTitle.ToLower() == "plural"
                          select i.Item).FirstOrDefault();
            if (iFound != null)
            {
                var iNew = new Conjugation();
                iNew.Meaning = iFound.ID;
                var iName = Name;
                if (iName.EndsWith("y"))
                {
                    iNew.Name = iName.TrimEnd('y') + "ies";
                }
                else if (iName.EndsWith("ch"))
                {
                    iNew.Name = iName + "es";
                }
                else if (iName.EndsWith("s") == false)
                {
                    iNew.Name = iName + "s";
                }
                else
                {
                    iNew.Name = Name;
                }

                Conjugations.Add(iNew);
            }
        }

        /// <summary>The render superlative.</summary>
        private void RenderSuperlative()
        {
            var iFound = (from i in BrainData.Current.Thesaurus.ConjugationMeanings
                          where i.NeuronInfo.DisplayTitle.ToLower() == "superlative"
                          select i.Item).FirstOrDefault();
            if (iFound != null)
            {
                var iNew = new Conjugation();
                iNew.Meaning = iFound.ID;
                var iName = Name;
                if (iName.EndsWith("y"))
                {
                    iNew.Name = iName.TrimEnd('y') + "iest";
                }
                else if (iName.EndsWith("e"))
                {
                    iNew.Name = iName + "st";
                }
                else
                {
                    iNew.Name = Name + "est";
                }

                Conjugations.Add(iNew);
            }
        }

        /// <summary>The render comparetive.</summary>
        private void RenderComparetive()
        {
            var iFound = (from i in BrainData.Current.Thesaurus.ConjugationMeanings
                          where i.NeuronInfo.DisplayTitle.ToLower() == "comparative"
                          select i.Item).FirstOrDefault();
            if (iFound != null)
            {
                var iNew = new Conjugation();
                iNew.Meaning = iFound.ID;
                var iName = Name;
                if (iName.EndsWith("y"))
                {
                    iNew.Name = iName.TrimEnd('y') + "ier";
                }
                else if (iName.EndsWith("e"))
                {
                    iNew.Name = iName + "r";
                }
                else
                {
                    iNew.Name = iName + "er";
                }

                Conjugations.Add(iNew);
            }
        }

        /// <summary>The render third person.</summary>
        private void RenderThirdPerson()
        {
            string[] iNames = { "third person present", "thirdpersonpresent" };
            var iFound = (from i in BrainData.Current.Thesaurus.ConjugationMeanings
                          where iNames.Contains(i.NeuronInfo.DisplayTitle.ToLower())
                          select i.Item).FirstOrDefault();
            if (iFound != null)
            {
                var iNew = new Conjugation();
                iNew.Meaning = iFound.ID;
                var iName = Name;
                var iVowels = new System.Collections.Generic.HashSet<char> { 'a', 'u', 'e', 'o', 'i' };
                if (iName.EndsWith("ss") || iName.EndsWith("ch") || iName.EndsWith("sh") || iName.EndsWith("x"))
                {
                    iNew.Name = Name + "es";
                }
                else if (iName.EndsWith("y"))
                {
                    // && iVowels.Contains(iName[iName.Length - 2]) == false
                    iNew.Name = iName.TrimEnd('y') + "ies";
                }
                else
                {
                    iNew.Name = iName + "s";
                }

                Conjugations.Add(iNew);
            }
        }

        /// <summary>The render past tense.</summary>
        private void RenderPastTense()
        {
            string[] iNames = { "past tense", "pasttense" };
            var iFound = (from i in BrainData.Current.Thesaurus.ConjugationMeanings
                          where iNames.Contains(i.NeuronInfo.DisplayTitle.ToLower())
                          select i.Item).FirstOrDefault();
            if (iFound != null)
            {
                var iNew = new Conjugation();
                iNew.Meaning = iFound.ID;
                var iName = Name;
                var iVowels = new System.Collections.Generic.HashSet<char> { 'a', 'u', 'e', 'o', 'i' };
                if (iName.EndsWith("e"))
                {
                    // if (iVowels.Contains(iName[iName.Length - 2]) == true)
                    iNew.Name = iName + "d";

                    // else
                    // iNew.Name = Name + "ed";
                }
                else if (iName.EndsWith("y") && iVowels.Contains(iName[iName.Length - 2]) == false)
                {
                    iNew.Name = iName.TrimEnd('y') + "ied";
                }
                else
                {
                    iNew.Name = Name + "ed";
                }

                Conjugations.Add(iNew);
            }
        }

        /// <summary>The render past particle.</summary>
        private void RenderPastParticle()
        {
            string[] iNames = { "past particle", "pastparticle" };
            var iFound = (from i in BrainData.Current.Thesaurus.ConjugationMeanings
                          where iNames.Contains(i.NeuronInfo.DisplayTitle.ToLower())
                          select i.Item).FirstOrDefault();
            if (iFound != null)
            {
                var iNew = new Conjugation();
                iNew.Meaning = iFound.ID;
                var iName = Name;
                var iVowels = new System.Collections.Generic.HashSet<char> { 'a', 'u', 'e', 'o', 'i' };
                if (iName.EndsWith("e"))
                {
                    // if (iVowels.Contains(iName[iName.Length - 2]) == true)
                    iNew.Name = iName + "d";

                    // else
                    // iNew.Name = Name + "ed";
                }
                else if (iName.EndsWith("y"))
                {
                    // && iVowels.Contains(iName[iName.Length - 2]) == false
                    iName = iName.TrimEnd('y');
                    iNew.Name = iName + "ied";
                }
                else
                {
                    iNew.Name = iName + "ed";
                }

                Conjugations.Add(iNew);
            }
        }

        /// <summary>The render present particle.</summary>
        private void RenderPresentParticle()
        {
            string[] iNames = { "present particle", "presentparticle" };
            var iFound = (from i in BrainData.Current.Thesaurus.ConjugationMeanings
                          where iNames.Contains(i.NeuronInfo.DisplayTitle.ToLower())
                          select i.Item).FirstOrDefault();
            if (iFound != null)
            {
                var iNew = new Conjugation();
                iNew.Meaning = iFound.ID;
                var iName = Name;
                if (iName.EndsWith("e"))
                {
                    iName = iName.TrimEnd('e');
                }

                iNew.Name = iName + "ing";
                Conjugations.Add(iNew);
            }
        }

        #endregion
    }
}