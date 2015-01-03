//-----------------------------------------------------------------------
// <copyright file="ProcItemTemplateSelector.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>23/12/2009</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace JaStDev.HAB.Designer
{
   public class ProcItemTemplateSelector : DataTemplateSelector
   {
      bool fDisplayVariables = false;
      #region DisplayVariables

      /// <summary>
      /// Gets/sets wether the variables should be depicted or the processor content.
      /// </summary>
      public bool DisplayVariables
      {
         get
         {
            return fDisplayVariables;
         }
         set
         {
            fDisplayVariables = value;
         }
      }

      #endregion

      /// <summary>
      /// When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate"/> based on custom logic.
      /// </summary>
      /// <param name="item">The data object for which to select the template.</param>
      /// <param name="container">The data-bound object.</param>
      /// <returns>
      /// Returns a <see cref="T:System.Windows.DataTemplate"/> or null. The default value is null.
      /// </returns>
      public override DataTemplate SelectTemplate(object item, DependencyObject container)
      {
         try
         {
            FrameworkElement iCont = container as FrameworkElement;
            if (item is ProcItem)
            {
               if (DisplayVariables == true)
                  return iCont.TryFindResource("ProcItemTemplate") as DataTemplate;
               else
                  return iCont.TryFindResource("ProcItemTemplate2") as DataTemplate;
            }
            else if (item is ProcManFolder)
               return iCont.TryFindResource("ProcManFolderTemplate") as DataTemplate;

            if (Properties.Settings.Default.FlowItemDisplayMode == 0)
               return iCont.TryFindResource("FlowItemStaticNormalView") as DataTemplate;
            else if (Properties.Settings.Default.FlowItemDisplayMode == 1)
               return iCont.TryFindResource("FlowItemStaticLeftIcon") as DataTemplate;
            else if (Properties.Settings.Default.FlowItemDisplayMode == 2)
               return iCont.TryFindResource("FlowItemStaticUnderIcon") as DataTemplate;
            else
               return base.SelectTemplate(item, container);
         }
         catch (Exception e)
         {
            LogService.Log.LogError("ProcItemTemplateSelector.SelectTemplate", e.ToString());
         }
         return null;
      }
   }
}