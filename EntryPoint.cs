﻿using System;
using System.Collections.Generic;
using System.Linq;
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
            //await Task.Run(() =>
            //{
            //    ParentViewModel.Database.AddAdditionalPreviewDocumentParser(new Docket.DocketParser());
            //    ParentViewModel.Database.AddAdditionalPreviewDocumentParser(new Invoice.InvoiceParser());
            //    ParentViewModel.Database.RegisterStorageableItem(new Export.Settings.SettingsController());
            //});
            

            var vm = new Export.Settings.ViewModel(this);
            //vm.LoadData();

            internalViewModels.Add(vm);

            ParentViewModel.SettingsTabViewModel.RegisteredExportClasses.Add(new Export.OrderPdfExport(ParentViewModel, vm));

            ParentViewModel.SettingsTabViewModel.SettingsList.Add(new Export.Settings.SettingsTab { DataContext =  vm});

            
        }

        public List<Biller.UI.Interface.IViewModel> ViewModels()
        {
            return internalViewModels;
        }

        private List<Biller.UI.Interface.IViewModel> internalViewModels { get; set; }
    }
}