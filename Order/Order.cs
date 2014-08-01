using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biller.Core;
using OrderTypes_Biller.Calculations;
using Biller.Core.Document;

namespace OrderTypes_Biller.Order
{
    /// <summary>
    /// Abstract class to implement various types of orders like <see cref="Invoice"/>, Offer, Delivery Note, ...
    /// 
    /// </summary>
    public abstract class Order : Biller.Core.Document.Document
    {
        public Order()
        {
            // insert empty values to avoid null exceptions.
            OrderedArticles = new ObservableCollection<Biller.Core.Articles.OrderedArticle>();
            DocumentID = ""; OrderOpeningText = ""; OrderClosingText = ""; Customer = new Biller.Core.Customers.Customer();
            Date = DateTime.Now; DateOfDelivery = DateTime.Now;
            OrderRebate = new Biller.Core.Utils.Percentage();
            OrderShipment = new Biller.Core.Utils.Shipment();
            PaymentMethode = new Biller.Core.Utils.PaymentMethode();
            OrderCalculation = new DefaultOrderCalculation(this);
        }

        /// <summary>
        /// Default implementation for storing the <see cref="OrderedArticles"/>.
        /// </summary>
        public virtual ObservableCollection<Biller.Core.Articles.OrderedArticle> OrderedArticles
        {
            get { return GetValue(() => OrderedArticles); }
            set { SetValue(value); }
        }

        /// <summary>
        /// Default implementation for storing the <see cref="OrderOpeningText"/>.
        /// </summary>
        public virtual string OrderOpeningText
        {
            get { return GetValue(() => OrderOpeningText); }
            set { SetValue(value); }
        }

        /// <summary>
        /// Default implementation for storing the <see cref="OrderClosingText"/>.
        /// </summary>
        public virtual string OrderClosingText
        {
            get { return GetValue(() => OrderClosingText); }
            set { SetValue(value); }
        }

        public virtual Biller.Core.Customers.Customer Customer
        {
            get { return GetValue(() => Customer); }
            set { SetValue(value); }
        }

        public virtual DefaultOrderCalculation OrderCalculation
        {
            get { return GetValue(() => OrderCalculation); }
            set { SetValue(value); }
        }

        public virtual Biller.Core.Utils.PaymentMethode PaymentMethode
        {
            get { return GetValue(() => PaymentMethode); }
            set { SetValue(value); }
        }

        public virtual Biller.Core.Utils.Percentage OrderRebate
        {
            get { return GetValue(() => OrderRebate); }
            set { SetValue(value); }
        }

        public virtual Biller.Core.Utils.Shipment OrderShipment
        {
            get { return GetValue(() => OrderShipment); }
            set { SetValue(value); }
        }

        public virtual DateTime DateOfDelivery
        {
            get { return GetValue(() => DateOfDelivery); }
            set { SetValue(value); }
        }

        public override abstract string DocumentType { get; }

        public override abstract System.Xml.Linq.XElement GetXElement();

        public override abstract void ParseFromXElement(System.Xml.Linq.XElement source);

        public static Biller.Core.Document.PreviewDocument PreviewFromOrder(Order source)
        {
            dynamic preview = new Biller.Core.Document.PreviewDocument(source.DocumentType);
            preview.DocumentID = source.DocumentID;
            preview.Date = source.Date;
            preview.Customer = source.Customer.DisplayName;
            preview.Value = source.OrderCalculation.OrderSummary;
            preview.LocalizedDocumentType = source.LocalizedDocumentType;

            return preview;
        }
    }
}
