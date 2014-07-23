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

namespace OrderTypes_Biller.Export.Settings
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            var control = new Controls.TextElement();
            control.MouseDown += control_MouseUp;
            viewModel.Elements.Add(control);
        }

        void control_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SelectedElement = sender as Settings.Controls.IExportControl;
        }

        #region ArticleColumns
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SettingsController.ArticleListColumns.Add(new Models.ArticleListColumnModel());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SettingsController.ArticleListColumns.Remove(viewModel.SelectedArticleListColumn);
        }

        /// <summary>
        /// Article columns stackpanel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SelectedArticleListColumn = (sender as StackPanel).DataContext as Models.ArticleListColumnModel;

            var sp = (StackPanel)sender;
            sp.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDD");
        }

        /// <summary>
        /// Article columns stackpanel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            viewModel.SaveSettings();
            
            var sp = (StackPanel)sender;
            sp.Background = System.Windows.Media.Brushes.Transparent;
        }

        /// <summary>
        /// Move article column down / Raise the index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            var index = viewModel.SettingsController.ArticleListColumns.IndexOf(viewModel.SelectedArticleListColumn);
            if (index >= 0)
            {
                viewModel.SettingsController.ArticleListColumns.RemoveAt(index);
                viewModel.SettingsController.ArticleListColumns.Insert(Math.Min(index + 1, viewModel.SettingsController.ArticleListColumns.Count), viewModel.SelectedArticleListColumn);
            }
        }

        /// <summary>
        /// Move article column up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            var index = viewModel.SettingsController.ArticleListColumns.IndexOf(viewModel.SelectedArticleListColumn);
            if (index >= 0)
            {
                viewModel.SettingsController.ArticleListColumns.RemoveAt(index);
                viewModel.SettingsController.ArticleListColumns.Insert(Math.Max(index - 1, 0), viewModel.SelectedArticleListColumn);
            }
        }
        #endregion

        #region FooterColumns

        private void SP_FooterColumns_LostFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            viewModel.SaveSettings();

            var sp = (StackPanel)sender;
            sp.Background = System.Windows.Media.Brushes.Transparent;
        }

        private void SP_FooterColumns_GotFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SelectedFooterColumn = (sender as StackPanel).DataContext as Models.FooterColumnModel;

            var sp = (StackPanel)sender;
            sp.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDD");
        }

        private void ButtonMoveFooterColumnUp(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            var index = viewModel.SettingsController.FooterColumns.IndexOf(viewModel.SelectedFooterColumn);
            if (index >= 0)
            {
                viewModel.SettingsController.FooterColumns.RemoveAt(index);
                viewModel.SettingsController.FooterColumns.Insert(Math.Max(index - 1, 0), viewModel.SelectedFooterColumn);
            }
        }

        private void ButtonMoveFooterColumnDown(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            var index = viewModel.SettingsController.FooterColumns.IndexOf(viewModel.SelectedFooterColumn);
            if (index >= 0)
            {
                viewModel.SettingsController.FooterColumns.RemoveAt(index);
                viewModel.SettingsController.FooterColumns.Insert(Math.Min(index + 1, viewModel.SettingsController.FooterColumns.Count), viewModel.SelectedFooterColumn);
            }
        }

        private void ButtonAddFooterColumn(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SettingsController.FooterColumns.Add(new Models.FooterColumnModel());
        }

        private void ButtonRemoveFooterColumn(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SettingsController.FooterColumns.Remove(viewModel.SelectedFooterColumn);
        }

        #endregion

        private void WatermarkTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Bilder|*.jpg; *.jpeg; *.jpe; *.jfif; *.png|PDF Dokument|*.pdf|Alle Dateien|*.*";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
                (sender as TextBox).Text = openFileDialog.FileName;
        }
    }
}
