//-----------------------------------------------------------------------
// <copyright file="WindowManager.xaml.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>22/07/2011</date>
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
using System.Diagnostics;

namespace JaStDev.HAB.Designer
{
   /// <summary>
   /// Interaction logic for WindowManager.xaml
   /// </summary>
   public partial class WindowManager : ResourceDictionary
   {
      public WindowManager()
      {
         InitializeComponent();
      }

      /// <summary>
      /// When one of the windowManager's windows is loaded, we make certain that it has all of the same command
      /// bindings and input bindings as that of the main window.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
      void Window_Loaded(object sender, RoutedEventArgs e)
      {
         Window iSender = sender as Window;
         Debug.Assert(iSender != null);
         if(WindowMain.Current.CommandBindings.Count > 0 && iSender.CommandBindings.Contains(WindowMain.Current.CommandBindings[0]) == false)
            iSender.CommandBindings.AddRange(WindowMain.Current.CommandBindings);
         if (WindowMain.Current.InputBindings.Count > 0 && iSender.InputBindings.Contains(WindowMain.Current.InputBindings[0]) == false)
            iSender.InputBindings.AddRange(WindowMain.Current.InputBindings);
      }

      /// <summary>
      /// Handles the gotFocus event of the Window control.
      /// When an item within the form gets focus, we try to make the selected neuron active within the application.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
      void Window_gotFocus(object sender, RoutedEventArgs e)
      {
         WindowMain.Current.DockingControl_GotFocus(sender, e);
      }
   }
}