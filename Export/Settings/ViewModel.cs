using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OrderTypes_Biller.Export.Settings
{
    public class ViewModel : Biller.Core.Utils.PropertyChangedHelper, Biller.UI.Interface.IViewModel
    {
        static List<string> allowedDataFields = new List<string>(new[] { "{CustomerAddressWithLineBreak}", "{CustomerAddressOneLine}", "{CustomerName}", "{SenderAddressWithLineBreak}",
        "{SenderAddressOneLine}", "{LocalizedDocumentType}", "{DocumentID}", "{DocumentType}", "{DocumentDate}", "{DateOfDelivery}", "{PaymentMethodeName}", "{PaymentMethodeText}",
        "{DocumentOpeningText}", "{DocumentClosingText}"});

        static List<string> allowedArticleDataFields = new List<string>(new[] { "{Position}", "{Amount}", "{ArticleID}", "{ArticleText}", "{ArticleName}", "{ArticleNameWithText}",
        "{SinglePriceGross}", "{SinglePriceNet}", "{TaxRate}", "{OrderedValueGross}", "{OrderedValueNet}", "{Rebate}", "{OrderedWeight}"});

        string DataLocation = (Assembly.GetExecutingAssembly().Location).Replace(System.IO.Path.GetFileName(Assembly.GetExecutingAssembly().Location), "") + "Data\\";

        public ViewModel(EntryPoint EntryPoint)
        {
            mainWindowViewModel = EntryPoint.ParentViewModel;
            Elements = new ObservableCollection<Controls.IExportControl>();
            SettingsController = new SettingsController();
        }

        private Biller.UI.ViewModel.MainWindowViewModel mainWindowViewModel;

        void SettingsController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SaveSettings();
        }

        public ObservableCollection<Controls.IExportControl> Elements { get { return GetValue(() => Elements); } private set { SetValue(value); } }

        public Controls.IExportControl SelectedElement { get { return GetValue(() => SelectedElement); } set { SetValue(value); } }

        public SettingsController SettingsController { get { return GetValue(() => SettingsController); } set { SetValue(value); } }
        public List<string> AllowedArticleDataFields { get { return ViewModel.allowedArticleDataFields; } }

        public Models.ArticleListColumnModel SelectedArticleListColumn { get { return GetValue(() => SelectedArticleListColumn); } set { SetValue(value); } }

        public Models.FooterColumnModel SelectedFooterColumn { get { return GetValue(() => SelectedFooterColumn); } set { SetValue(value); } }

        public async void SaveSettings()
        {
            await mainWindowViewModel.Database.SaveOrUpdateStorageableItem(SettingsController);
        }

        public async Task LoadData()
        {
            await mainWindowViewModel.Database.AddAdditionalPreviewDocumentParser(new Docket.DocketParser());
            await mainWindowViewModel.Database.AddAdditionalPreviewDocumentParser(new Invoice.InvoiceParser());
            await mainWindowViewModel.Database.RegisterStorageableItem(new Export.Settings.SettingsController());

            var savedItem = (await mainWindowViewModel.Database.AllStorageableItems(new Export.Settings.SettingsController())).FirstOrDefault();
            if (savedItem != null)
                SettingsController = savedItem as Export.Settings.SettingsController;

            SettingsController.PropertyChanged += SettingsController_PropertyChanged;
        }

        public void ReceiveData(object data)
        {
            //
        }
    }
}
