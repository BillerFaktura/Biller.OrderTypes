using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OrderTypes_Biller
{
    public class EntryPoint : Biller.UI.Interface.IPlugIn
    {
        public EntryPoint(Biller.UI.ViewModel.MainWindowViewModel parentViewModel)
        {
            this.ParentViewModel = parentViewModel;
            internalViewModels = new List<Biller.UI.Interface.IViewModel>();
        }

        public Biller.UI.ViewModel.MainWindowViewModel ParentViewModel { get; private set; }

        public string Name
        {
            get { return "Ordertypes @ Biller"; }
        }

        public string Description
        {
            get { return "Stellt die Auftragsdokumente Rechnung, Lieferschein und Angebot bereit"; }
        }

        public double Version
        {
            get { return 1.20140826; }
        }

        public void Activate()
        {
            var vm = new Export.Settings.ViewModel(this);
            internalViewModels.Add(vm);

            ParentViewModel.DocumentTabViewModel.AddDocumentFactory(new Invoice.InvoiceFactory());
            ParentViewModel.DocumentTabViewModel.AddDocumentFactory(new Docket.DocketFactory());
            ParentViewModel.DocumentTabViewModel.AddDocumentFactory(new Offer.OfferFactory());
            ParentViewModel.SettingsTabViewModel.RegisteredExportClasses.Add(new Export.OrderPdfExport(ParentViewModel, vm));
            ParentViewModel.SettingsTabViewModel.SettingsList.Add(new Export.Settings.SettingsTab { DataContext =  vm});
            ParentViewModel.UpdateManager.Register(new Biller.Core.Models.AppModel() { Title = Name, Description = Description, GuID = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value.ToLower(), Version = 1.20140828, UpdateSource = "https://raw.githubusercontent.com/LastElb/OrderTypes-Biller/master/update.json" });
        }

        public List<Biller.UI.Interface.IViewModel> ViewModels()
        {
            return internalViewModels;
        }

        private List<Biller.UI.Interface.IViewModel> internalViewModels { get; set; }
    }
}