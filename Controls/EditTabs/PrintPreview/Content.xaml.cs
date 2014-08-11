﻿using Biller.UI.DocumentView.Contextual;
using MigraDoc.DocumentObjectModel.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrderTypes_Biller.Controls.PrintPreview
{
    /// <summary>
    /// Interaktionslogik für Content.xaml
    /// </summary>
    public partial class Content : UserControl
    {
        public Content()
        {
            InitializeComponent();
        }

        private void UpdatePreview()
        {
            var viewmodel = (DataContext as DocumentEditViewModel);
            if (viewmodel.ExportClass != null)
                viewmodel.ExportClass.RenderDocumentPreview(viewmodel.Document);
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }
    }
}
