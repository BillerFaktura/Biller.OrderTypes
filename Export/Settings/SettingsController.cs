using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biller.Data.Utils;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace OrderTypes_Biller.Export.Settings
{
    public class SettingsController : Biller.Data.Utils.PropertyChangedHelper, Biller.Data.Interfaces.IXMLStorageable
    {
        public Biller.Data.Utils.Unit cmUnit { get; private set; }

        public SettingsController()
        {
            cmUnit = new Unit() { DecimalDigits = 3, DecimalSeperator = ".", Name = "Centimeter", ShortName = "cm", ThousandSeperator = "" };
            AddressFrameHeight = 3;
            AddressFrameWidth = 7;
            AddressFrameLeft = 2;
            AddressFrameTop = 5;
            AddressFrameShowSender = true;
            OrderInfoTop = 4;
            OrderInfoRight = -0.5;
            ArticleListColumns = new ObservableCollection<Models.ArticleListColumnModel>();
        }

        #region AddressFrame
        public double AddressFrameHeight { get { return GetValue(() => AddressFrameHeight); } set { SetValue(value); } }
        public double AddressFrameWidth { get { return GetValue(() => AddressFrameWidth); } set { SetValue(value); } }
        public double AddressFrameTop { get { return GetValue(() => AddressFrameTop); } set { SetValue(value); } }
        public double AddressFrameLeft { get { return GetValue(() => AddressFrameLeft); } set { SetValue(value); } }
        public bool AddressFrameShowSender { get { return GetValue(() => AddressFrameShowSender); } set { SetValue(value); } }
        #endregion

        #region OrderInfo
        public double OrderInfoTop { get { return GetValue(() => OrderInfoTop); } set { SetValue(value); } }
        public double OrderInfoRight { get { return GetValue(() => OrderInfoRight); } set { SetValue(value); } }
        public bool OrderInfoShowCustomerID { get { return GetValue(() => OrderInfoShowCustomerID); } set { SetValue(value); } }
        #endregion

        #region ArticleList
        public ObservableCollection<Models.ArticleListColumnModel> ArticleListColumns { get { return GetValue(() => ArticleListColumns); } set { SetValue(value); } }
        #endregion

        public XElement GetXElement()
        {
            string json = JsonConvert.SerializeObject(this);
            return new XElement(XElementName, new XElement(IDFieldName, ID), new XElement("Content", json));
        }

        public void ParseFromXElement(System.Xml.Linq.XElement source)
        {
            if (source.Name != XElementName)
                throw new Exception("Expected " + XElementName + " but got " + source.Name);

            var settings = JsonConvert.DeserializeObject<SettingsController>(source.Element("Content").Value);
            AddressFrameHeight = settings.AddressFrameHeight;
            AddressFrameWidth = settings.AddressFrameWidth;
            AddressFrameTop = settings.AddressFrameTop;
            AddressFrameLeft = settings.AddressFrameLeft;
            AddressFrameShowSender = settings.AddressFrameShowSender;
            OrderInfoTop = settings.OrderInfoTop;
            OrderInfoRight = settings.OrderInfoRight;
            OrderInfoShowCustomerID = settings.OrderInfoShowCustomerID;
            ArticleListColumns = settings.ArticleListColumns;
        }

        public string XElementName
        {
            get { return "ExportLayoutSetting"; }
        }

        public string ID
        {
            get { return "1"; }
        }

        public string IDFieldName
        {
            get { return "ID"; }
        }

        public Biller.Data.Interfaces.IXMLStorageable GetNewInstance()
        {
            return new SettingsController();
        }
    }
}
