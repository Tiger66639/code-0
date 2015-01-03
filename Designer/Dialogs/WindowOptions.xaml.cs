// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowOptions.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for WindowOptions.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for WindowOptions.xaml
    /// </summary>
    public partial class WindowOptions : System.Windows.Window
    {
        /// <summary>The f voices.</summary>
        private readonly System.Collections.Generic.List<CharacterEngine.Voice> fVoices;

        /// <summary>Initializes a new instance of the <see cref="WindowOptions"/> class.</summary>
        public WindowOptions()
        {
            InitializeComponent();
            TxtStackSize.Text = Settings.InitProcessorStackSize.ToString();
            TxtTemplateFile.Text = Properties.Settings.Default.DefaultTemplatePath;
            CheckAutoSave.IsChecked = Properties.Settings.Default.AutoSave;
            CheckTriggerEvents.IsChecked = Properties.Settings.Default.DesignerTriggersNetworkEvents;
            CheckEditingSpellcheck.IsChecked = Properties.Settings.Default.EditorsUseSpellcheck;
            TxtAutoSaveInterval.Text = Properties.Settings.Default.AutoSaveInterval.Minutes.ToString();
            TxtSandboxPath.Text = ProjectManager.Default.SandboxLocation;
            TxtFrameNetPath.Text = Properties.Settings.Default.FrameNetPath;
            TxtVerbNetPath.Text = Properties.Settings.Default.VerbNetPath;
            TxtCharactersPath.Text = Properties.Settings.Default.CharactersPath;
            TxtMaxConcurrentProcessors.Text = Properties.Settings.Default.MaxConcurrentProcessors.ToString();
            TxtMinBlockedProcessors.Text = Properties.Settings.Default.MinReservedBlockedProcessors.ToString();
            TxtTNrOfUndo.Text = Properties.Settings.Default.MaxNrOfUndo.ToString();

            CmbEditorsFont.ItemsSource = DescriptionView.AvailableFontsStatic;

                // we reuse the liset of the description editor, saves some mem + time.
            CmbEditorsFont.SelectedItem = Properties.Settings.Default.PatternEditorsFont;

            SetSpeechEngine();

            fVoices = new System.Collections.Generic.List<CharacterEngine.Voice>();
            foreach (var i in CharacterEngine.SpeechEngine.AvailableVoices)
            {
                var iNew = new CharacterEngine.Voice
                               {
                                   DisplayName = i.DisplayName, 
                                   Name = i.Name, 
                                   PreferedCharacter = i.PreferedCharacter, 
                                   SendFormatString = i.SendFormatString, 
                                   SSMLEnabled = i.SSMLEnabled
                               };
                fVoices.Add(i);
            }

            GrdVoices.ItemsSource = fVoices;
            if (WindowMain.Current.DesignerVisibility != System.Windows.Visibility.Visible)
            {
                Row1.Height = new System.Windows.GridLength(0);
                Row2.Height = new System.Windows.GridLength(0);
            }
        }

        /// <summary>The set speech engine.</summary>
        private void SetSpeechEngine()
        {
            switch (Properties.Settings.Default.SpeechEngineMode)
            {
                case CharacterEngine.SpeechEngineMode.NonManaged:
                    CmbEngineType.SelectedIndex = 0;
                    break;
                case CharacterEngine.SpeechEngineMode.Managed:
                    CmbEngineType.SelectedIndex = 1;
                    break;
                case CharacterEngine.SpeechEngineMode.Synth:
                    CmbEngineType.SelectedIndex = 2;
                    break;
                default:
                    break;
            }
        }

        /// <summary>The on click more.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickMore(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.Controls.Button;

            if (iSender != null)
            {
                var iText = iSender.Tag as System.Windows.Controls.TextBox;
                if (iText != null)
                {
                    var iDialog = new System.Windows.Forms.FolderBrowserDialog();

                    iDialog.Description = iText.Tag as string;
                    iDialog.SelectedPath = iText.Text;
                    var iRes = iDialog.ShowDialog();
                    if (iRes == System.Windows.Forms.DialogResult.OK)
                    {
                        iText.Text = iDialog.SelectedPath;
                    }
                }
            }
        }

        /// <summary>The on click more file.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickMoreFile(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.Controls.Button;

            if (iSender != null)
            {
                var iText = iSender.Tag as System.Windows.Controls.TextBox;
                if (iText != null)
                {
                    var iDialog = ProjectLoader.GetOpenDlg();
                    if (string.IsNullOrEmpty(iText.Text) == false)
                    {
                        iDialog.FileName = iText.Text;
                    }

                    if (iDialog.ShowDialog() == true)
                    {
                        iText.Text = iDialog.FileName;
                    }
                }
            }
        }

        /// <summary>The on click ok.</summary>
        /// <param name="aSender">The a sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickOk(object aSender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>update all bindings + save all values back to the<see langword="static"/> fields (to which you can't bind). If there is
        ///     an invalid value somewhere, cancel the close operation.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult == true)
            {
                var iStackSize = 0;
                var iMaxConcurrent = 0;
                var iInterval = 0;
                var iUndoCount = 0;
                var iMinReserved = 0;
                if (GetInitProcStackSize(out iStackSize) == false
                    || GetMaxConcurrentProcessors(out iMaxConcurrent) == false
                    || GetMinReservedProcessors(out iMinReserved) == false
                    || GetAutoSaveInterval(out iInterval) == false || GetUndoCount(out iUndoCount) == false)
                {
                    DialogResult = false;
                    e.Cancel = true;
                }

                UpdateSpeechEngine();
                StoreVoiceInfo(); // needs to be done after updating the speech engine, otherwise we loose the info

                Properties.Settings.Default.AutoSaveInterval = new System.TimeSpan(0, iInterval, 0);

                Settings.InitProcessorStackSize = iStackSize;
                Properties.Settings.Default.InitProcessorStackSize = iStackSize;

                Settings.SetMinMaxProc(iMinReserved, iMaxConcurrent);
                Properties.Settings.Default.MinReservedBlockedProcessors = iMinReserved;
                Properties.Settings.Default.MaxConcurrentProcessors = Settings.MaxConcurrentProcessors;

                ProjectManager.Default.SandboxLocation = TxtSandboxPath.Text;
                Properties.Settings.Default.DefaultTemplatePath = TxtTemplateFile.Text;
                Properties.Settings.Default.FrameNetPath = TxtFrameNetPath.Text;
                Properties.Settings.Default.VerbNetPath = TxtVerbNetPath.Text;
                if (Properties.Settings.Default.CharactersPath != TxtCharactersPath.Text)
                {
                    ChatBotChannel.Characters = null;
                    Properties.Settings.Default.CharactersPath = TxtCharactersPath.Text;
                }

                Properties.Settings.Default.AutoSave = CheckAutoSave.IsChecked.HasValue
                                                        && CheckAutoSave.IsChecked == true;
                Properties.Settings.Default.DesignerTriggersNetworkEvents = CheckTriggerEvents.IsChecked.HasValue
                                                                             && CheckTriggerEvents.IsChecked == true;
                Properties.Settings.Default.EditorsUseSpellcheck = CheckEditingSpellcheck.IsChecked.HasValue
                                                                    && CheckEditingSpellcheck.IsChecked.Value;
                Properties.Settings.Default.MaxNrOfUndo = iUndoCount;
                Properties.Settings.Default.PatternEditorsFont =
                    (System.Windows.Media.FontFamily)CmbEditorsFont.SelectedItem;
                WindowMain.UndoStore.MaxUndoLevel = iUndoCount;

                ((App)System.Windows.Application.Current).UpdateAutoSave();
            }
        }

        /// <summary>
        ///     Checks is the engines need to be updated in all the chatbot channels
        ///     and updates accordingly.
        /// </summary>
        private void UpdateSpeechEngine()
        {
            CharacterEngine.SpeechEngineMode iNew;
            if (CmbEngineType.SelectedIndex == 0)
            {
                iNew = CharacterEngine.SpeechEngineMode.NonManaged;
            }
            else if (CmbEngineType.SelectedIndex == 1)
            {
                iNew = CharacterEngine.SpeechEngineMode.Managed;
            }
            else
            {
                iNew = CharacterEngine.SpeechEngineMode.Synth;
            }

            if (Properties.Settings.Default.SpeechEngineMode != iNew)
            {
                Properties.Settings.Default.SpeechEngineMode = iNew;
                CharacterEngine.SpeechEngine.EngineMode = iNew;
                var iChatbots = from i in BrainData.Current.CommChannels
                                where i is ChatBotChannel
                                select (ChatBotChannel)i;
                foreach (var i in iChatbots)
                {
                    i.ChangeSpeechEngine(iNew);
                }
            }
        }

        /// <summary>The store voice info.</summary>
        private void StoreVoiceInfo()
        {
            if (CharacterEngine.SpeechEngine.AvailableVoicesLoaded)
            {
                // when the app never loaded a project, it could be that there are no voices loaded. for some OS's it isn't a good idea to do this when the app closes.
                CharacterEngine.Voice iVoice;
                for (var i = 0; i < fVoices.Count; i++)
                {
                    iVoice = CharacterEngine.SpeechEngine.AvailableVoices[i];
                    iVoice.DisplayName = fVoices[i].DisplayName;
                    iVoice.Name = fVoices[i].Name;
                    iVoice.PreferedCharacter = fVoices[i].PreferedCharacter;
                    iVoice.SendFormatString = fVoices[i].SendFormatString;
                    iVoice.SSMLEnabled = fVoices[i].SSMLEnabled;
                }

                var iChatbots = from i in BrainData.Current.CommChannels
                                where i is ChatBotChannel
                                select (ChatBotChannel)i;
                foreach (var i in iChatbots)
                {
                    // need to let each chatbot channel reload the list of available voices.
                    i.ResetAvailableVoices();
                }
            }
        }

        /// <summary>The get auto save interval.</summary>
        /// <param name="iMinutes">The i minutes.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetAutoSaveInterval(out int iMinutes)
        {
            if (int.TryParse(TxtAutoSaveInterval.Text, out iMinutes) == false)
            {
                System.Windows.MessageBox.Show("Please provide a valid integer for 'auto save time interval'!");
                return false;
            }

            return true;
        }

        /// <summary>The get undo count.</summary>
        /// <param name="iUndoCount">The i undo count.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetUndoCount(out int iUndoCount)
        {
            if (int.TryParse(TxtTNrOfUndo.Text, out iUndoCount) == false)
            {
                System.Windows.MessageBox.Show("Please provide a valid integer for 'Maximum undo size'!");
                return false;
            }

            return true;
        }

        /// <summary>The get init proc stack size.</summary>
        /// <param name="iFound">The i found.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetInitProcStackSize(out int iFound)
        {
            if (int.TryParse(TxtStackSize.Text, out iFound) == false)
            {
                System.Windows.MessageBox.Show("Please provide a valid integer for 'Initial stack size'!");
                return false;
            }

            return true;
        }

        /// <summary>The get max concurrent processors.</summary>
        /// <param name="iFound">The i found.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetMaxConcurrentProcessors(out int iFound)
        {
            if (int.TryParse(TxtMaxConcurrentProcessors.Text, out iFound) == false)
            {
                System.Windows.MessageBox.Show("Please provide a valid integer for 'Max nr of processor threads'!");
                return false;
            }

            return true;
        }

        /// <summary>The get min reserved processors.</summary>
        /// <param name="iMinReserved">The i min reserved.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetMinReservedProcessors(out int iMinReserved)
        {
            if (int.TryParse(TxtMinBlockedProcessors.Text, out iMinReserved) == false)
            {
                System.Windows.MessageBox.Show(
                    "Please provide a valid integer for 'Min nr of supported blocked processors'!");
                return false;
            }

            return true;
        }

        /// <summary>Handles the Click event of the BtnFrameNetPath control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event
        ///     data.</param>
        private void BtnTemplateFile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Filter = "Xml files(*.xml)|*.xml|All(*.*)|*.*";
            iDlg.FileName = TxtTemplateFile.Text;
            iDlg.Multiselect = false;
            var iRes = iDlg.ShowDialog(this);
            if (iRes.HasValue && iRes.Value)
            {
                TxtTemplateFile.Text = iDlg.FileName;
            }
        }
    }
}