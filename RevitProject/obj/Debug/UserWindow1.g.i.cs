﻿#pragma checksum "..\..\UserWindow1.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "0680B7A8F1803DF5E59A3FA245FC691DEEF0885C7A0C33084DB014A7EC6CE657"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

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


namespace RevitProject {
    
    
    /// <summary>
    /// UserWindow1
    /// </summary>
    public partial class UserWindow1 : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 62 "..\..\UserWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock FormText;
        
        #line default
        #line hidden
        
        
        #line 85 "..\..\UserWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel stackPanel;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\UserWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RadioButton1;
        
        #line default
        #line hidden
        
        
        #line 92 "..\..\UserWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RadioButton2;
        
        #line default
        #line hidden
        
        
        #line 97 "..\..\UserWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RadioButton3;
        
        #line default
        #line hidden
        
        
        #line 102 "..\..\UserWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RadioButton4;
        
        #line default
        #line hidden
        
        
        #line 161 "..\..\UserWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ConfirmButton;
        
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
            System.Uri resourceLocater = new System.Uri("/RevitProject;component/userwindow1.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\UserWindow1.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
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
            this.FormText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            
            #line 75 "..\..\UserWindow1.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_SelectContour);
            
            #line default
            #line hidden
            return;
            case 3:
            this.stackPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 4:
            this.RadioButton1 = ((System.Windows.Controls.RadioButton)(target));
            
            #line 91 "..\..\UserWindow1.xaml"
            this.RadioButton1.Checked += new System.Windows.RoutedEventHandler(this.RadioButton_Studio);
            
            #line default
            #line hidden
            return;
            case 5:
            this.RadioButton2 = ((System.Windows.Controls.RadioButton)(target));
            
            #line 96 "..\..\UserWindow1.xaml"
            this.RadioButton2.Checked += new System.Windows.RoutedEventHandler(this.RadioButton_OneBedroom);
            
            #line default
            #line hidden
            return;
            case 6:
            this.RadioButton3 = ((System.Windows.Controls.RadioButton)(target));
            
            #line 101 "..\..\UserWindow1.xaml"
            this.RadioButton3.Checked += new System.Windows.RoutedEventHandler(this.RadioButton_TwoBedroom);
            
            #line default
            #line hidden
            return;
            case 7:
            this.RadioButton4 = ((System.Windows.Controls.RadioButton)(target));
            
            #line 105 "..\..\UserWindow1.xaml"
            this.RadioButton4.Checked += new System.Windows.RoutedEventHandler(this.RadioButton_ThreeBedroom);
            
            #line default
            #line hidden
            return;
            case 8:
            this.ConfirmButton = ((System.Windows.Controls.Button)(target));
            
            #line 166 "..\..\UserWindow1.xaml"
            this.ConfirmButton.Click += new System.Windows.RoutedEventHandler(this.Button_Confirm);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

