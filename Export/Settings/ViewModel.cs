using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OrderTypes_Biller.Export.Settings
{
    public class ViewModel : Biller.Data.Utils.PropertyChangedHelper
    {
        static List<string> allowedDataFields = new List<string>(new[] { "{CustomerAddressWithLineBreak}", "{CustomerAddressOneLine}", "{CustomerName}", "{SenderAddressWithLineBreak}",
        "{SenderAddressOneLine}", "{LocalizedDocumentType}", "{DocumentID}", "{DocumentType}", "{DocumentDate}", "{DateOfDelivery}", "{PaymentMethodeName}", "{PaymentMethodeText}",
        "{DocumentOpeningText}", "{DocumentClosingText}"});

        static List<string> allowedArticleDataFields = new List<string>(new[] { "{Position}", "{Amount}", "{ArticleID}", "{ArticleText}", "{ArticleName}",
        "{SinglePriceGross}", "{SinglePriceNet}", "{TaxRate}", "{OrderedValueGross}", "{OrderedValueNet}", "{Rebate}"});

        public ViewModel()
        {
            Elements = new ObservableCollection<Controls.IExportControl>();
            SettingsController = new SettingsController();
        }

        public ObservableCollection<Controls.IExportControl> Elements { get { return GetValue(() => Elements); } private set { SetValue(value); } }

        public Controls.IExportControl SelectedElement { get { return GetValue(() => SelectedElement); } set { SetValue(value); } }

        public SettingsController SettingsController { get { return GetValue(() => SettingsController); } set { SetValue(value); } }
        public List<string> AllowedArticleDataFields { get { return ViewModel.allowedArticleDataFields; } }

        public Models.ArticleListColumnModel SelectedArticleListColumn { get { return GetValue(() => SelectedArticleListColumn); } set { SetValue(value); } }
    }
}
