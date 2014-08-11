using Biller.Core.Articles;
using Biller.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderTypes_Biller.Calculations
{
    public class SmallBusinessCalculation : DefaultOrderCalculation
    {
        public SmallBusinessCalculation(Order.Order order)
            : base(order)
        {
            _parentOrder = order;
        }

        Order.Order _parentOrder;

        public override void CalculateValues()
        {
            TaxValues.Clear();
            ArticleSummary.Amount = 0;
            // iterate through each article and add its value
            foreach (var article in _parentOrder.OrderedArticles)
            {
                ArticleSummary.Amount += article.RoundedGrossOrderValue.Amount;

                // taxclass listing
                //if (TaxValues.Any(x => x.TaxClass.Name == article.TaxClass.Name))
                //    TaxValues.Single(x => x.TaxClass.Name == article.TaxClass.Name).Value += article.ExactVAT;
                //else
                //    TaxValues.Add(new Biller.Core.Models.TaxClassMoneyModel() { Value = new Money(article.ExactVAT), TaxClass = article.TaxClass });
            }

            //NetArticleSummary
            //NetArticleSummary.Amount = ArticleSummary.Amount;
            //foreach (var item in TaxValues)
            //    NetArticleSummary.Amount -= item.Value.Amount;

            OrderSummary.Amount = ArticleSummary.Amount;
            //NetOrderSummary.Amount = NetArticleSummary.Amount;

            // Discount
            if (_parentOrder.OrderRebate.Amount > 0)
            {
                var temp = OrderSummary.Amount;
                OrderSummary.Amount = OrderSummary.Amount * (1 - _parentOrder.OrderRebate.Amount);
                //NetOrderRebate.Amount = NetOrderSummary.Amount - NetOrderSummary.Amount * (1 - _parentOrder.OrderRebate.Amount);
                //NetOrderSummary.Amount = NetOrderSummary.Amount * (1 - _parentOrder.OrderRebate.Amount);
                OrderRebate = new EMoney(temp - OrderSummary.Amount);
                //foreach (var item in TaxValues)
                //    item.Value = item.Value * (1 - _parentOrder.OrderRebate.Amount);
            }
            else
                OrderRebate = new EMoney(0);

            // Shipping
            if (!String.IsNullOrEmpty(_parentOrder.OrderShipment.Name))
            {
                // Just for Germany
                // We need to split the taxes with the ratio it is before
                // Austria: Shipping has reduced taxes
                // CH: 
                OrderSummary.Amount += _parentOrder.OrderShipment.DefaultPrice.Amount;
                dynamic keyValueStore = Biller.UI.ViewModel.MainWindowViewModel.GetCurrentMainWindowViewModel().SettingsTabViewModel.KeyValueStore;
                //var UseGermanSupplementaryTaxRegulation = keyValueStore.UseGermanSupplementaryTaxRegulation;
                //if (UseGermanSupplementaryTaxRegulation == null)
                //    UseGermanSupplementaryTaxRegulation = false;
                //if (UseGermanSupplementaryTaxRegulation)
                //{
                //    var wholetax = 0.0;
                //    var wholeShipmentTax = 0.0;
                //    foreach (var item in TaxValues)
                //        wholetax += item.Value.Amount;

                //    List<Biller.Core.Models.TaxClassMoneyModel> temporaryTaxes = new List<Biller.Core.Models.TaxClassMoneyModel>();
                //    foreach (var taxitem in TaxValues)
                //    {
                //        var ratio = 1 / (wholetax / taxitem.Value.Amount);
                //        var shipment = new OrderedArticle(new Article());
                //        shipment.TaxClass = taxitem.TaxClass;
                //        shipment.OrderedAmount = 1;
                //        shipment.OrderPrice.Price1 = _parentOrder.OrderShipment.DefaultPrice;

                //        var TaxSupplementaryWorkSeparate = keyValueStore.TaxSupplementaryWorkSeperate;
                //        if (TaxSupplementaryWorkSeparate == null)
                //            TaxSupplementaryWorkSeparate = false;

                //        if (TaxSupplementaryWorkSeparate)
                //            temporaryTaxes.Add(new Biller.Core.Models.TaxClassMoneyModel() { Value = new Money(ratio * shipment.ExactVAT), TaxClass = taxitem.TaxClass, TaxClassAddition = " auf Nebenleistungen" });
                //        else
                //            taxitem.Value += (ratio * shipment.ExactVAT);
                //        wholeShipmentTax += ratio * shipment.ExactVAT;
                //    }
                //    NetShipment.Amount = _parentOrder.OrderShipment.DefaultPrice.Amount - wholeShipmentTax;
                //    NetOrderSummary.Amount += NetShipment.Amount;

                //    foreach (Biller.Core.Models.TaxClassMoneyModel temporaryTax in temporaryTaxes)
                //        TaxValues.Add(temporaryTax);
                //}
                //else
                //{
                    var shipment = new OrderedArticle(new Article());
                    try
                    {
                        shipment.TaxClass = (TaxClass)keyValueStore.ShipmentTaxClass;
                        shipment.OrderedAmount = 1;
                        shipment.OrderPrice.Price1 = _parentOrder.OrderShipment.DefaultPrice;
                    }
                    catch
                    {
                        Biller.UI.ViewModel.MainWindowViewModel.GetCurrentMainWindowViewModel().NotificationManager.ShowNotification("Fehler bei Berechnung des Betrages", "Es wurde keine Steuerklasse für Nebenleistungen angegeben. Überprüfen Sie die Einstellungen.");
                        return;
                    }
                    TaxValues.Add(new Biller.Core.Models.TaxClassMoneyModel() { Value = new Money(shipment.ExactVAT), TaxClass = shipment.TaxClass, TaxClassAddition = " auf Nebenleistungen" });
                    //NetShipment.Amount = _parentOrder.OrderShipment.DefaultPrice.Amount - shipment.ExactVAT;
                    //NetOrderSummary.Amount += NetShipment.Amount;
                //}
            }

            // Skonto
            if (_parentOrder.PaymentMethode.Discount.Amount > 0)
                CashBack = new EMoney(_parentOrder.PaymentMethode.Discount.Amount * ArticleSummary.Amount);
            else
                CashBack = new EMoney(0);

            RaiseUpdateManually("ArticleSummary");
            RaiseUpdateManually("TaxValues");
            RaiseUpdateManually("OrderRebate");
            RaiseUpdateManually("CashBack");
            RaiseUpdateManually("NetArticleSummary");
            RaiseUpdateManually("NetOrderSummary");
            RaiseUpdateManually("OrderSummary");
            RaiseUpdateManually("NetShipment");
            RaiseUpdateManually("NetOrderRebate");
        }
    }
}
