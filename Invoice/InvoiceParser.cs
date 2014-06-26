﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Biller.Core;

namespace OrderTypes_Biller.Invoice
{
    public class InvoiceParser : Biller.Core.Interfaces.DocumentParser
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
            if (document is Invoice)
            {
                var result = database.GetCustomer(data.Element("CustomerID").Value);
                var customer = result.Result;
                (document as Invoice).Customer = customer;

                var articles = data.Element("OrderedArticles").Elements();
                (document as Invoice).OrderedArticles.Clear();
                foreach (XElement article in articles)
                {
                    var temp = new Biller.Core.Articles.OrderedArticle();

                    var task = database.ArticleUnits();
                    temp.ArticleUnit = task.Result.Where(x => x.Name == article.Element("ArticleUnit").Value).Single();

                    var taskTaxClass = database.TaxClasses();
                    temp.TaxClass = taskTaxClass.Result.Where(x => x.Name == article.Element("TaxClass").Value).Single();

                    temp.ParseFromXElement(article);
                    (document as Invoice).OrderedArticles.Add(temp);
                }

                try
                {
                    (document as Invoice).PaymentMethode.ParseFromXElement(data.Element((document as Invoice).PaymentMethode.XElementName));
                    (document as Invoice).OrderShipment.ParseFromXElement(data.Element((document as Invoice).OrderShipment.XElementName));
                }
                catch (Exception e)
                { }

                var money = new Biller.Core.Utils.Money(0);
            }
            else
            {
                return false;
            }
            return true;
        }

        public string DocumentType { get { return "Invoice"; } }

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
                var invoice = new Invoice();
                return invoice.LocalizedDocumentType;
            }
        }
    }
}
