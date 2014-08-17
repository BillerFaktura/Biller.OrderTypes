using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Biller.Core;

namespace OrderTypes_Biller.Offer
{
    public class OfferParser : Biller.Core.Interfaces.DocumentParser
    {
        /// <summary>
        /// Parses <see cref="Customer"/> and <see cref="OrderedArticles"/>.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="data"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool ParseAdditionalData(ref Biller.Core.Document.Document document, XElement data, Biller.Core.Interfaces.IDatabase database)
        {
            if (document is Offer)
            {
                var result = database.GetCustomer(data.Element("CustomerID").Value);
                var customer = result.Result;
                (document as Offer).Customer = customer;

                var articles = data.Element("OrderedArticles").Elements();
                (document as Offer).OrderedArticles.Clear();
                foreach (XElement article in articles)
                {
                    var temp = new Biller.Core.Articles.OrderedArticle();

                    var task = database.ArticleUnits();
                    temp.ArticleUnit = task.Result.Where(x => x.Name == article.Element("ArticleUnit").Value).Single();

                    var taskTaxClass = database.TaxClasses();
                    temp.TaxClass = taskTaxClass.Result.Where(x => x.Name == article.Element("TaxClass").Value).Single();

                    temp.ParseFromXElement(article);
                    (document as Offer).OrderedArticles.Add(temp);
                }

                try
                {
                    (document as Offer).PaymentMethode.ParseFromXElement(data.Element((document as Offer).PaymentMethode.XElementName));
                    (document as Offer).OrderShipment.ParseFromXElement(data.Element((document as Offer).OrderShipment.XElementName));
                } catch { }
                try
                {
                    (document as Offer).SmallBusiness = Boolean.Parse(data.Element("SmallBusiness").Value);
                    if ((document as Offer).SmallBusiness)
                        (document as Offer).OrderCalculation = new Calculations.SmallBusinessCalculation(document as Order.Order, true);
                    else if (!((document as Offer).OrderCalculation is Calculations.DefaultOrderCalculation))
                        (document as Offer).OrderCalculation = new Calculations.DefaultOrderCalculation(document as Order.Order, true);
                } catch { }
                try
                {
                    if (data.Element("EAddress") != null)
                    {
                        (document as Offer).DeliveryAddress.ParseFromXElement(data.Element("EAddress"));
                    }       
                } catch {}
            }
            else
            {
                return false;
            }
            return true;
        }

        public string DocumentType { get { return "Offer"; } }

        public bool ParseAdditionalPreviewData(ref dynamic document, XElement data)
        {
            var money = new Biller.Core.Utils.EMoney(0);
            money.ParseFromXElement(data.Element("PreviewValue").Element(money.XElementName));
            document.Value = money;
            document.LocalizedDocumentType = LocalizedDocumentType;
            return true;
        }

        public string LocalizedDocumentType
        {
            get
            {
                var invoice = new Offer();
                return invoice.LocalizedDocumentType;
            }
        }
    }
}
