﻿#pragma checksum "..\..\..\..\..\Editors\Visuals\Views\VisualEditorView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7CF7E628A3AEAABFBB1013C47070F28B"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.34209
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using JaStDev.ControlFramework.Controls;
using JaStDev.ControlFramework.Controls.Primitives;
using JaStDev.ControlFramework.Input;
using JaStDev.ControlFramework.Utility;
using JaStDev.HAB;
using JaStDev.HAB.Designer;
using JaStDev.HAB.Designer.WPF.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace JaStDev.HAB.Designer.Editors {
    
    
    /// <summary>
    /// VisualEditorView
    /// </summary>
    public partial class VisualEditorView : JaStDev.HAB.Designer.CtrlEditorBase, System.Windows.Markup.IComponentConnector {
        
        
        #line 38 "..\..\..\..\..\Editors\Visuals\Views\VisualEditorView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnAddFrame;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\..\Editors\Visuals\Views\VisualEditorView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox CmbLowOperator;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\..\..\Editors\Visuals\Views\VisualEditorView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox CmbHighOperator;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\..\..\Editors\Visuals\Views\VisualEditorView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox LstVisuals;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/HAB.Designer;component/editors/visuals/views/visualeditorview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Editors\Visuals\Views\VisualEditorView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 20 "..\..\..\..\..\Editors\Visuals\Views\VisualEditorView.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.AddFrame_Executed);
            
            #line default
            #line hidden
            return;
            case 2:
            this.BtnAddFrame = ((System.Windows.Controls.Button)(target));
            return;
            case 3:
            this.CmbLowOperator = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.CmbHighOperator = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 5:
            this.LstVisuals = ((System.Windows.Controls.ListBox)(target));
            
            #line 73 "..\..\..\..\..\Editors\Visuals\Views\VisualEditorView.xaml"
            this.LstVisuals.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ListBox_SelectionChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

