// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionChannelView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for ReflectionChannelView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for ReflectionChannelView.xaml
    /// </summary>
    public partial class ReflectionChannelView : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="ReflectionChannelView"/> class.</summary>
        public ReflectionChannelView()
        {
            InitializeComponent();
#if BASIC || PRO
            ColSplitter.Width = new System.Windows.GridLength(0);
            ColOpCodes.Width = new System.Windows.GridLength(0);

#endif
        }

        /// <summary>Handles the Click event of the BtnLoadCach control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnLoadCach_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgAssemblyFromGac();
            iDlg.Owner = System.Windows.Application.Current.MainWindow;
            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes.Value)
            {
                LoadAssembly(iDlg.SelectedItem);
            }
        }

        /// <summary>Handles the Click event of the BtnLoadFile control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnLoadFile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Filter = "Executables(*.exe;*.dll)|*.dll;*.exe|Libraries|*.dll|Applications|*.exe|All(*.*)|*.*";
            iDlg.Multiselect = true;
            var iRes = iDlg.ShowDialog(System.Windows.Window.GetWindow(this));
            if (iRes.HasValue && iRes.Value)
            {
                foreach (var iFile in iDlg.FileNames)
                {
                    LoadAssembly(iFile);
                }
            }
        }

        /// <summary>Loads the assembly in the brain.</summary>
        /// <param name="file">The file from where it came.</param>
        private void LoadAssembly(string file)
        {
            var iChannel = (ReflectionChannel)DataContext;
            if (file.StartsWith("file:/"))
            {
                file = file.Substring(8);
            }

            var iAsm = ReflectionSin.GetAssembly(file);
            var iData = new AssemblyData();
            iData.Assembly = iAsm;
            iData.IsAssemblyLoaded = true; // we set to true, cause we only load like this when visible
            if (iData.Children.Count > 0)
            {
                iChannel.Assemblies.Add(iData);
            }
            else
            {
                System.Windows.MessageBox.Show("No public types with static public methods found in assembly!");
            }
        }

        /// <summary>Handles the Checked event of the BtnToggleOpcodes control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnToggleOpcodes_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (ReflectionChannel)DataContext;
            iChannel.LoadOpCodes();
        }

        /// <summary>Handles the Unchecked event of the BtnToggleOpcodes control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnToggleOpcodes_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (ReflectionChannel)DataContext;
            iChannel.UnloadOpCodes();
        }

        /// <summary>The delete_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Delete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iChannel = (ReflectionChannel)DataContext;
            var iSelected = TrvAssemblies.SelectedItem as AssemblyData;
            iSelected.IsLoaded = false; // need to make certain no data is loaded.
            iChannel.Assemblies.Remove(iSelected);
        }

        /// <summary>Handles the CanExecute event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Delete_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TrvAssemblies.SelectedItem is AssemblyData;
        }

        /// <summary>The export_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Export_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iData = e.Parameter as FunctionData;
            e.CanExecute = iData != null && iData.IsLoaded.HasValue && iData.IsLoaded.Value;
        }

        /// <summary>Handles the Executed event of the Export control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Export_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iData = e.Parameter as FunctionData;
            if (iData != null)
            {
                var iParamTypeNames =
                    (from p in iData.Method.GetParameters() select p.ParameterType.AssemblyQualifiedName).ToList();
                var iNew = new ExportableReflectionSinEntryPoint
                               {
                                   AssemblyName =
                                       iData.Method.DeclaringType.Assembly.Location, 
                                   MethodName = iData.Method.Name, 
                                   ParameterTypes = iParamTypeNames, 
                                   TypeName = iData.Method.ReflectedType.FullName, 
                                   MappedName = iData.NeuronInfo.DisplayTitle
                               };

                using (var iFile = new System.IO.MemoryStream())
                {
                    var iSettings = BaseXmlStreamer.CreateWriterSettings();
                    using (var iWriter = System.Xml.XmlWriter.Create(iFile, iSettings))
                    {
                        var valueSerializer =
                            new System.Xml.Serialization.XmlSerializer(typeof(ExportableReflectionSinEntryPoint));
                        valueSerializer.Serialize(iWriter, iNew);
                    }

                    iFile.Flush();
                    iFile.Position = 0;
                    var iReader = new System.IO.StreamReader(iFile);
                    System.Windows.Clipboard.SetText(iReader.ReadToEnd());
                }
            }
        }
    }
}