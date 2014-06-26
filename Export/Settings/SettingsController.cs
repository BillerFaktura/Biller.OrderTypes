using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biller.Core.Utils;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.ComponentModel;

namespace OrderTypes_Biller.Export.Settings
{
    public class SettingsController : Biller.Core.Utils.PropertyChangedHelper, Biller.Core.Interfaces.IXMLStorageable
    {
        public Biller.Core.Utils.Unit cmUnit { get; private set; }

        public SettingsController()
        {
            cmUnit = new Unit() { DecimalDigits = 3, DecimalSeperator = ".", Name = "Centimeter", ShortName = "cm", ThousandSeperator = "" };
            AddressFrameTop = 5;
            AddressFrameShowSender = true;
            
            ArticleListColumns = new ObservableCollection<Models.ArticleListColumnModel>();
            FooterColumns = new ObservableCollection<Models.FooterColumnModel>();
        }

        #region AddressFrame
        public double AddressFrameTop { get { return GetValue(() => AddressFrameTop); } set { SetValue(value); } }
        public bool AddressFrameShowSender { get { return GetValue(() => AddressFrameShowSender); } set { SetValue(value); } }
        #endregion

        #region OrderInfo
        public bool OrderInfoShowCustomerID { get { return GetValue(() => OrderInfoShowCustomerID); } set { SetValue(value); } }
        public bool OrderInfoShowPageNumbers { get { return GetValue(() => OrderInfoShowPageNumbers); } set { SetValue(value); } }
        #endregion

        #region ArticleList
        public ObservableCollection<Models.ArticleListColumnModel> ArticleListColumns { get { return GetValue(() => ArticleListColumns); } set { SetValue(value); } }
        #endregion

        #region Footer
        public ObservableCollection<Models.FooterColumnModel> FooterColumns { get { return GetValue(() => FooterColumns); } set { SetValue(value); } }
        #endregion

        #region Header
        [DefaultValue("")]
        public string RelativeImagePath { get; set; }
        [DefaultValue("")]
        public string AbsoluteImagePath { get; set; }
        
        [JsonIgnore]
        public string DisplayedPath
        { 
            get
            {
                if (File.Exists(AbsoluteImagePath))
                    return AbsoluteImagePath;
                var path = MakeAbsolutePath((Assembly.GetExecutingAssembly().Location).Replace(System.IO.Path.GetFileName(Assembly.GetExecutingAssembly().Location), ""), RelativeImagePath);
                if (File.Exists(path))
                {
                    AbsoluteImagePath = path;
                    return path;
                }
                //ToDo: Add Notification
                return "";
            } 
            set
            {
                SetValue(value);
                ProcessPath(value);
            }
        }
        public int PositionLeft { get { return GetValue(() => PositionLeft); } set { SetValue(value); } }
        public int RelativeHorizontal { get { return GetValue(() => RelativeHorizontal); } set { SetValue(value); } }
        public int RelativeVertical { get { return GetValue(() => RelativeVertical); } set { SetValue(value); } }
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

            string json = source.Element("Content").Value;
            var settings = JsonConvert.DeserializeObject<SettingsController>(json);
            AddressFrameTop = settings.AddressFrameTop;
            AddressFrameShowSender = settings.AddressFrameShowSender;
            PositionLeft = settings.PositionLeft;
            RelativeHorizontal = settings.RelativeHorizontal;
            RelativeVertical = settings.RelativeVertical;
            OrderInfoShowCustomerID = settings.OrderInfoShowCustomerID;
            OrderInfoShowPageNumbers = settings.OrderInfoShowPageNumbers;
            RelativeImagePath = settings.RelativeImagePath;
            AbsoluteImagePath = settings.AbsoluteImagePath;
            if (settings.ArticleListColumns != null)
                ArticleListColumns = settings.ArticleListColumns;
            else
            {
                ArticleListColumns.Add(new Models.ArticleListColumnModel() { Header = "Pos", AlignmentIndex = 0, Content = "{Position}", ColumnWidth = 1 });
                ArticleListColumns.Add(new Models.ArticleListColumnModel() { Header = "Art.-Nr.", AlignmentIndex = 0, Content = "{ArticleID}", ColumnWidth = 1.7 });
                ArticleListColumns.Add(new Models.ArticleListColumnModel() { Header = "Menge", AlignmentIndex = 0, Content = "{Amount}", ColumnWidth = 1.5 });
                ArticleListColumns.Add(new Models.ArticleListColumnModel() { Header = "Artikel", AlignmentIndex = 0, Content = "{ArticleNameWithText}", ColumnWidth = 6 });
                ArticleListColumns.Add(new Models.ArticleListColumnModel() { Header = "Einzelpreis", AlignmentIndex = 0, Content = "{SinglePriceGross}", ColumnWidth = 2 });
                ArticleListColumns.Add(new Models.ArticleListColumnModel() { Header = "Steuersatz", AlignmentIndex = 0, Content = "{TaxRate}", ColumnWidth = 1.7 });
                ArticleListColumns.Add(new Models.ArticleListColumnModel() { Header = "Gesamt Brutto", AlignmentIndex = 2, Content = "{OrderedValueGross}", ColumnWidth = 2 });
            }
            if (settings.FooterColumns != null)
                FooterColumns = settings.FooterColumns;
            else
            {

            }
            
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

        public Biller.Core.Interfaces.IXMLStorageable GetNewInstance()
        {
            return new SettingsController();
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (toPath.StartsWith("..\\"))
                return toPath;
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string MakeAbsolutePath(string localPath, string absolutePath)
        {
            if (absolutePath == null)
                return "";
            if (!absolutePath.StartsWith("..\\"))
                return absolutePath;
            var s = Path.Combine(localPath, absolutePath);
            s = Path.GetFullPath(s);
            return s;
        }

        private void ProcessPath(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                RelativeImagePath = "";
                AbsoluteImagePath = "";
            }
            RelativeImagePath = MakeRelativePath((Assembly.GetExecutingAssembly().Location).Replace(System.IO.Path.GetFileName(Assembly.GetExecutingAssembly().Location), ""), value);
            AbsoluteImagePath = MakeAbsolutePath((Assembly.GetExecutingAssembly().Location).Replace(System.IO.Path.GetFileName(Assembly.GetExecutingAssembly().Location), ""), value);
        }
    }
}
