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

        private void StackPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SelectedArticleListColumn = (sender as StackPanel).DataContext as Models.ArticleListColumnModel;

            var sp = (StackPanel)sender;
            sp.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDD");
        }

        private void StackPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            viewModel.SaveSettings();
            
            var sp = (StackPanel)sender;
            sp.Background = System.Windows.Media.Brushes.Transparent;
        }

        /// <summary>
        /// Move Item down / Raise the index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            var index = viewModel.SettingsController.ArticleListColumns.IndexOf(viewModel.SelectedArticleListColumn);
            viewModel.SettingsController.ArticleListColumns.RemoveAt(index);
            viewModel.SettingsController.ArticleListColumns.Insert(Math.Min(index + 1, viewModel.SettingsController.ArticleListColumns.Count), viewModel.SelectedArticleListColumn);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            var index = viewModel.SettingsController.ArticleListColumns.IndexOf(viewModel.SelectedArticleListColumn);
            viewModel.SettingsController.ArticleListColumns.RemoveAt(index);
            viewModel.SettingsController.ArticleListColumns.Insert(Math.Max(index - 1, 0), viewModel.SelectedArticleListColumn);
        }

        
    }
}
