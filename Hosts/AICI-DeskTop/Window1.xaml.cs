//-----------------------------------------------------------------------
// <copyright file="Window1.xaml.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>07/08/2010</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Collections.Specialized;

namespace AICI_DeskTop
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      Aici fNetwork = new Aici();

      public Window1()
      {
         InitializeComponent();
         Action iStartNetwork = new Action(fNetwork.StartNetwork);                              //we start the network asynchronosly.
         AsyncCallback iCallBack = a => 
         {
            Action iSetDataContext = delegate { DataContext = fNetwork; };                           //An AsyncCallback is called from a seperate thread of the pool, which can't access the UI thread, so jump once again.
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, iSetDataContext);
         };
         iStartNetwork.BeginInvoke(iCallBack, null);
      }

      #region NeeedsScrollIntoView

      /// <summary>
      /// NeeedsScrollIntoView Attached Dependency Property
      /// </summary>
      public static readonly DependencyProperty NeeedsScrollIntoViewProperty =
          DependencyProperty.RegisterAttached("NeeedsScrollIntoView", typeof(bool), typeof(Window1),
              new FrameworkPropertyMetadata((bool)false,
                  new PropertyChangedCallback(OnNeeedsScrollIntoViewChanged)));

      /// <summary>
      /// Gets the NeeedsScrollIntoView property.  This attached property 
      /// indicates wether a dialog item needs to be scrolled into view or not.
      /// </summary>
      public static bool GetNeeedsScrollIntoView(DependencyObject d)
      {
         return (bool)d.GetValue(NeeedsScrollIntoViewProperty);
      }

      /// <summary>
      /// Sets the NeeedsScrollIntoView property.  This attached property 
      /// indicates wether a dialog item needs to be scrolled into view or not.
      /// </summary>
      public static void SetNeeedsScrollIntoView(DependencyObject d, bool value)
      {
         d.SetValue(NeeedsScrollIntoViewProperty, value);
      }

      /// <summary>
      /// Handles changes to the NeeedsScrollIntoView property.
      /// </summary>
      private static void OnNeeedsScrollIntoViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         if ((bool)e.NewValue == true)
         {
            ListBoxItem iItem = d as ListBoxItem;
            if (iItem != null)
               iItem.BringIntoView();
         }
      }

      #endregion



      private void TxtInput_PreviewKeyDown(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.Return)
         {
            TextChannel iChannel = ((FrameworkElement)sender).DataContext as TextChannel;
            if (iChannel != null)
               iChannel.SendInputText();
         }
      }

      private void Window_Closing(object sender, CancelEventArgs e)
      {
         fNetwork.CloseNetwork();         
      }

      private void SaveConv_Click(object sender, RoutedEventArgs e)
      {
         SaveFileDialog iDlg = new SaveFileDialog();
         iDlg.AddExtension = true;
         iDlg.DefaultExt = "txt";
         iDlg.Filter = "Text files (*.txt)|*.txt";
         bool? iDlgRes = iDlg.ShowDialog(this);
         if (iDlgRes.HasValue == true && iDlgRes.Value == true)
         {
            TextChannel iChannel = (TextChannel)TabChannels.SelectedContent;
            Debug.Assert(iChannel != null);
            using (StreamWriter iWriter = File.CreateText(iDlg.FileName))
            {
               foreach (DialogItem i in iChannel.ChatLog)
                  iWriter.WriteLine("{0}: {1}", i.Originator, i.Text);
            }
         }
      }

      private void Copy_Click(object sender, RoutedEventArgs e)
      {
         TextChannel iChannel = (TextChannel)DataContext;
         Debug.Assert(iChannel != null);
         StringBuilder iRes = new StringBuilder();
         foreach (DialogItem i in iChannel.ChatLog)
         {
            iRes.AppendFormat("{0}: {1}", i.Originator, i.Text);
            iRes.AppendLine();
         }
         Clipboard.SetText(iRes.ToString(), TextDataFormat.Text);
      }

   }
}