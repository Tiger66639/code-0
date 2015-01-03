using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AiciWeb.AiciServiceReference;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections.ObjectModel;

namespace AiciWeb
{
    public partial class MainPage : UserControl
    {
        #region innertypes

        public class DialogLogItem
        {
            SolidColorBrush fColor;
            public string Originator { get; set; }
            public string Text { get; set; }
            public SolidColorBrush Color
            {
                get
                {
                    return fColor;
                }
                set
                {
                    fColor = value;
                }
            }
        }

        #endregion

        #region Fields
        #if DEBUG
        const string AiciServiceURL = "http://localhost:24301/AiciService.svc";
        #else
        //const string AiciServiceURL = "http://janbogaerts.name/aiciweb/AiciService.svc";
        const string AiciServiceURL = "http://bragisoft.com/AiciClient/AiciService.svc";
        #endif

        AiciServiceClient fProxy;
        string fChannel;
        ObservableCollection<DialogLogItem> fDialog = new ObservableCollection<DialogLogItem>();
        #endregion

        #region ctor
        public MainPage()
        {
            InitializeComponent();
            LstDialogs.ItemsSource = fDialog;
            EndpointAddress address = new EndpointAddress(AiciServiceURL);
            PollingDuplexHttpBinding binding = new PollingDuplexHttpBinding(PollingDuplexMode.MultipleMessagesPerPoll);
            fProxy = new AiciServiceClient(binding, address);
            fProxy.OutputReceived += new EventHandler<OutputReceivedEventArgs>(fProxy_OutputReceived);
            fProxy.RequestChannelCompleted += new EventHandler<RequestChannelCompletedEventArgs>(fProxy_RequestChannelCompleted);
            fProxy.RequestChannelAsync();

            AddSystemDialog("Requesting communication channel from Aici server...");
        }

        #endregion


        #region Event handlers
        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnSend_Click(sender, e);
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(fChannel) == false)
            {
                AiciMesage iMessage = new AiciMesage();
                iMessage.Channel = fChannel;
                iMessage.InputMessage = TxtInput.Text;

                DialogLogItem iNew = new DialogLogItem();
                iNew.Color = new SolidColorBrush(Colors.Black);
                iNew.Originator = "You";
                iNew.Text = iMessage.InputMessage;
                fDialog.Add(iNew);

                TxtInput.Text = "";
                fProxy.SendMessageAsync(iMessage);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(fChannel) == true)
                fProxy.ReleaseChannelAsync(fChannel);
        }

        #region Proxy
        void fProxy_OutputReceived(object sender, OutputReceivedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                AddErrorDlg("PC side canceled output operation");
                return;
            }
            if (e.Error == null)
            {
                DialogLogItem iDlg = new DialogLogItem();
                iDlg.Color = new SolidColorBrush(Colors.Blue);
                iDlg.Originator = "PC";
                iDlg.Text = e.value;
                fDialog.Add(iDlg);
            }
            else
                AddErrorDlg(e.Error.ToString());
        }

        void fProxy_RequestChannelCompleted(object sender, RequestChannelCompletedEventArgs e)
        {
            fProxy.RequestChannelCompleted -= fProxy_RequestChannelCompleted;
            if (e.Cancelled == false)
            {
                if (e.Error == null)
                {
                    fChannel = e.Result;
                    BtnSend.IsEnabled = true;
                    TxtInput.IsEnabled = true;
                    AddSystemDialog("Connected with Aici, you can chat now.");
                }
                else
                    AddErrorDlg(string.Format("Failed to connect to the Aici server with the error: {0}.", e.Error.ToString()));
            }
            else
                AddErrorDlg("Failed to connect to the Aici server: the operation was canceled.");
        }

        private void AddErrorDlg(string text)
        {
            DialogLogItem iDlg = new DialogLogItem();
            iDlg.Color = new SolidColorBrush(Colors.Red);
            iDlg.Originator = "ERROR";
            iDlg.Text = text;
            fDialog.Add(iDlg);
            LstDialogs.ScrollIntoView(iDlg);
        }

        private void AddSystemDialog(string text)
        {
            DialogLogItem iNew = new DialogLogItem();
            iNew.Color = new SolidColorBrush(Colors.Green);
            iNew.Originator = "System";
            iNew.Text = text;
            fDialog.Add(iNew);
        } 

        #endregion


        #endregion
    }
}
