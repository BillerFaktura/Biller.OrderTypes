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
            get { return "Implements different order types to save and load from database"; }
        }

        public double Version
        {
            get { return 0.1; }
        }

        public void Activate()
        {
            var vm = new Export.Settings.ViewModel(this);
            internalViewModels.Add(vm);
            ParentViewModel.SettingsTabViewModel.RegisteredExportClasses.Add(new Export.OrderPdfExport(ParentViewModel, vm));
            ParentViewModel.SettingsTabViewModel.SettingsList.Add(new Export.Settings.SettingsTab { DataContext =  vm});
            ParentViewModel.UpdateManager.Register(new Biller.Core.Models.AppModel() { Title = "Auftragsdokumente", Description = "Stellt die Auftragsdokumente Rechnung, Lieferschein und Angebot bereit", GuID = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value.ToLower(), Version = 1.20140813, UpdateSource = "https://raw.githubusercontent.com/LastElb/BillerV2/master/update.json" });
        }

        public List<Biller.UI.Interface.IViewModel> ViewModels()
        {
            return internalViewModels;
        }

        private List<Biller.UI.Interface.IViewModel> internalViewModels { get; set; }
    }
}
