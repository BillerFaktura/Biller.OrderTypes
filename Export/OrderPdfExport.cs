using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using System.Diagnostics;
using MigraDoc.Rendering.Printing;
using MigraDoc.DocumentObjectModel.IO;
using NLog;
using Biller.Core.Models;
using System.Text.RegularExpressions;
using PdfSharp;

namespace OrderTypes_Biller.Export
{
    public class OrderPdfExport : Biller.Core.Interfaces.IExport
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        Biller.UI.ViewModel.MainWindowViewModel MainWindowViewModel;
        Settings.ViewModel ParentViewModel;
        public OrderPdfExport(Biller.UI.ViewModel.MainWindowViewModel MainWindowViewModel, Settings.ViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;
            this.MainWindowViewModel = MainWindowViewModel;
            PreviewElement = new MigraDoc.Rendering.Windows.DocumentPreview();
            PrintDialog = new System.Windows.Forms.PrintDialog();
        }

        MigraDoc.Rendering.Windows.DocumentPreview PreviewElement;

        System.Windows.Forms.PrintDialog PrintDialog;

        /// <summary>
        /// Creates the dynamic parts of the invoice.
        /// </summary>
        async Task<MigraDoc.DocumentObjectModel.Document> FillContent(Order.Order order)
        {
            MigraDoc.DocumentObjectModel.Document document = new MigraDoc.DocumentObjectModel.Document();
            var pageSetup = document.DefaultPageSetup.Clone();
            pageSetup.TopMargin = Unit.FromCentimeter(11);
            pageSetup.BottomMargin = Unit.FromCentimeter(4);
            var companySettings = (await MainWindowViewModel.Database.AllStorageableItems(new Biller.Core.Models.CompanySettings())).FirstOrDefault() as Biller.Core.Models.CompanySettings;

            PrepareStyles(document);

            // Each MigraDoc document needs at least one section.
            Section section = document.AddSection();
            section.PageSetup = pageSetup;

            CreateHeader(section, companySettings, order);
            CreateFooter(section, companySettings);
            CreatePreListContent(section, order);
            CreateArticleList(section, order);
            CreatePostListContent(section, order);

            return document;
        }

        private void PrepareStyles(MigraDoc.DocumentObjectModel.Document document)
        {
            // Get the predefined style Normal.
            Style style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Calibri";

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called Table based on style Normal
            style = document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Calibri";
            style.Font.Size = 9;

            // Create a new style called Reference based on style Normal
            style = document.Styles.AddStyle("Reference", "Normal");
            style.ParagraphFormat.SpaceBefore = "5mm";
            style.ParagraphFormat.SpaceAfter = "5mm";
            style.ParagraphFormat.TabStops.AddTabStop("16cm", TabAlignment.Right);
            style.Font.Size = 16;
        }

        private void CreateFooter(Section section, Biller.Core.Models.CompanySettings companySettings)
        {
            var footer = section.Footers.Primary.AddTable();
            footer.Style = "Table";
            footer.Borders.Color = Color.Empty;
            footer.Borders.Visible = false;
            footer.Rows.LeftIndent = 0;
            footer.LeftPadding = "0cm";

            foreach (var footercolumn in ParentViewModel.SettingsController.FooterColumns)
            {
                var fcolumn = footer.AddColumn(Unit.FromCentimeter(footercolumn.ColumnWidth));
                fcolumn.Borders.Visible = false;
                fcolumn.Format.Alignment = footercolumn.Alignment;
            }

            if (ParentViewModel.SettingsController.FooterColumns.Count > 0)
            {
                Row footerrow = footer.AddRow();

                var index = 0;
                foreach (var footercolumn in ParentViewModel.SettingsController.FooterColumns)
                {
                    var content = ReplaceFooterPlaceHolder(footercolumn.Value, companySettings);
                    footerrow.Cells[index].AddParagraph(content);
                    index += 1;
                }
            }
        }

        private void CreateHeader(Section section, Biller.Core.Models.CompanySettings companySettings, Order.Order order)
        {
            Paragraph paragraph;
            Table table;
            Column column;
            Row row;

            if (!String.IsNullOrEmpty(ParentViewModel.SettingsController.DisplayedPath))
            {
                Image image = section.Headers.Primary.AddImage(ParentViewModel.SettingsController.DisplayedPath);
                image.LockAspectRatio = true;
                switch (ParentViewModel.SettingsController.RelativeVertical)
                {
                    case 0:
                        image.RelativeVertical = RelativeVertical.Margin;
                        break;
                    case 1:
                        image.RelativeVertical = RelativeVertical.Page;
                        break;
                    default:
                        image.RelativeVertical = RelativeVertical.Margin;
                        break;
                }
                switch (ParentViewModel.SettingsController.RelativeHorizontal)
                {
                    case 0:
                        image.RelativeHorizontal = RelativeHorizontal.Margin;
                        break;
                    case 1:
                        image.RelativeHorizontal = RelativeHorizontal.Page;
                        break;
                    default:
                        image.RelativeHorizontal = RelativeHorizontal.Margin;
                        break;
                }
                if (ParentViewModel.SettingsController.ImagePositionLeftIndex >= 0)
                {
                    switch (ParentViewModel.SettingsController.ImagePositionLeftIndex)
                    {
                        case 0:
                            image.Left = ShapePosition.Left;
                            break;
                        case 1:
                            image.Left = ShapePosition.Center;
                            break;
                        case 2:
                            image.Left = ShapePosition.Right;
                            break;
                        default:
                            image.Left = ShapePosition.Center;
                            break;
                    }
                }
                else
                {
                    image.Left = ParentViewModel.SettingsController.cmUnit.ValueToString(ParentViewModel.SettingsController.ImagePositionLeft);
                }
                image.Top = ParentViewModel.SettingsController.cmUnit.ValueToString(ParentViewModel.SettingsController.ImagePositionTop);
                image.WrapFormat.Style = WrapStyle.Through;
            }
            

            // Addresses
            table = section.Headers.Primary.AddTable();
            table.Style = "Table";
            table.Rows.Alignment = RowAlignment.Left;
            column = table.AddColumn("10cm");
            column.Format.Alignment = ParagraphAlignment.Left;
            row = table.AddRow();
            row.Format.SpaceBefore = Unit.FromCentimeter(ParentViewModel.SettingsController.AddressFrameTop);

            if (ParentViewModel.SettingsController.AddressFrameShowSender)
            {
                paragraph = row.Cells[0].AddParagraph(companySettings.MainAddress.OneLineString);
                paragraph.Format.Font.Size = 8;
            }

            row = table.AddRow();
            paragraph = row.Cells[0].AddParagraph();
            foreach (var line in order.Customer.MainAddress.AddressStrings)
                paragraph.AddText(line + "\n");

            table = section.Headers.Primary.AddTable();
            table.Style = "Table";
            table.Rows.Alignment = RowAlignment.Right;
            column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Left;
            column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Right;
            row = table.AddRow();
            row.Cells[0].AddParagraph("Rechnungsdatum:");
            row.Cells[1].AddParagraph(order.Date.ToString("dd.MM.yyyy"));
            row = table.AddRow();
            row.Cells[0].AddParagraph("Leistungsdatum:");
            row.Cells[1].AddParagraph(order.DateOfDelivery.ToString("dd.MM.yyyy"));
            if (ParentViewModel.SettingsController.OrderInfoShowCustomerID)
            {
                row = table.AddRow();
                row.Cells[0].AddParagraph("Kundennummer:");
                row.Cells[1].AddParagraph(order.Customer.CustomerID);
            }
            if (ParentViewModel.SettingsController.OrderInfoShowPageNumbers)
            {
                row = table.AddRow();
                row.Cells[0].AddParagraph("Seite");
                paragraph = row.Cells[1].AddParagraph();
                paragraph.AddPageField();
                paragraph.AddText(" von ");
                paragraph.AddNumPagesField();
            }
        }

        private void CreatePreListContent(Section section, Order.Order order)
        {
            // Order ID
            var paragraph = section.AddParagraph();
            //paragraph.Format.SpaceBefore = "8cm";
            paragraph.Style = "Reference";
            paragraph.AddFormattedText(order.LocalizedDocumentType + " Nr. " + order.DocumentID, TextFormat.Bold);

            // Order opening text
            if (!String.IsNullOrEmpty(order.OrderOpeningText))
            {
                paragraph = section.AddParagraph(order.OrderOpeningText);
                paragraph.Format.SpaceAfter = "0.75cm";
            }
        }

        private void CreateArticleList(Section section, Order.Order order)
        {
            // Create the article table
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Color = TableBorder;
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            // Article list columns
            foreach (var ArticleColumn in ParentViewModel.SettingsController.ArticleListColumns)
            {
                var column = table.AddColumn(Unit.FromCentimeter(ArticleColumn.ColumnWidth));
                column.Format.Alignment = ArticleColumn.Alignment;
            }


            if (ParentViewModel.SettingsController.ArticleListColumns.Count == 0)
            {
                //ToDo: Show a notification
                return;
            }
            var row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.SpaceBefore = "0,1cm";
            row.Format.SpaceAfter = "0,25cm";

            var index = 0;
            foreach (var ArticleColumn in ParentViewModel.SettingsController.ArticleListColumns)
            {
                row.Cells[index].AddParagraph(ArticleColumn.Header);
                index += 1;
            }

            logger.Trace("FillContent - AddArticle " + order.DocumentType + ":" + order.DocumentID);

            foreach (var article in order.OrderedArticles)
            {
                logger.Trace("AddArticle - " + article.ArticleID);

                Row row1 = table.AddRow();
                index = 0;
                foreach (var ArticleColumn in ParentViewModel.SettingsController.ArticleListColumns)
                {
                    row1.Cells[index].AddParagraph(ReplaceArticlePlaceHolder(ArticleColumn.Content, article));
                    index += 1;
                }

                row1.Format.SpaceBefore = "0,1cm";
                row1.Format.SpaceAfter = "0,4cm";
            }

            logger.Trace("Setting Borders");
            Border BlackBorder = new Border();
            BlackBorder.Visible = true;
            BlackBorder.Color = Colors.Black;
            BlackBorder.Width = 0.75;

            Border BlackThickBorder = new Border();
            BlackThickBorder.Visible = true;
            BlackThickBorder.Color = Colors.Black;
            BlackThickBorder.Width = 1.5;

            Border NoBorder = new Border();
            NoBorder.Visible = false;

            logger.Trace("Adding subtotal net");
            var lastcolumn = ParentViewModel.SettingsController.ArticleListColumns.Count - 1;

            dynamic sb = Biller.UI.ViewModel.MainWindowViewModel.GetCurrentMainWindowViewModel().SettingsTabViewModel.KeyValueStore;
            if (sb.IsSmallBusiness)
            {
                // Add the total price row
                row = table.AddRow();
                row.Format.PageBreakBefore = true;
                row.Cells[0].AddParagraph("Zwischensumme");
                row.Cells[0].Format.Font.Bold = true;
                row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[0].MergeRight = lastcolumn - 1;
                row.Cells[lastcolumn].AddParagraph(order.OrderCalculation.ArticleSummary.AmountString);
                row.Format.SpaceBefore = "0,1cm";
                row.Cells[0].Borders.Bottom = NoBorder.Clone();
                row.Cells[lastcolumn].Borders.Bottom = NoBorder.Clone();
                row.Cells[0].Borders.Top = NoBorder.Clone();
                row.Cells[lastcolumn].Borders.Top = NoBorder.Clone();
            }
            else
            {
                // Add the total price row
                row = table.AddRow();
                row.Format.PageBreakBefore = true;
                row.Cells[0].AddParagraph("Zwischensumme Netto");
                row.Cells[0].Format.Font.Bold = true;
                row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[0].MergeRight = lastcolumn - 1;
                row.Cells[lastcolumn].AddParagraph(order.OrderCalculation.NetArticleSummary.AmountString);
                row.Format.SpaceBefore = "0,1cm";
                row.Cells[0].Borders.Bottom = NoBorder.Clone();
                row.Cells[lastcolumn].Borders.Bottom = NoBorder.Clone();
                row.Cells[0].Borders.Top = NoBorder.Clone();
                row.Cells[lastcolumn].Borders.Top = NoBorder.Clone();
            }

            if (order.OrderCalculation.OrderRebate.Amount > 0)
            {
                logger.Trace("Adding OrderRebate");
                row = table.AddRow();
                row.Cells[0].AddParagraph("Abzgl. " + order.OrderRebate.PercentageString + " Gesamtrabatt");
                row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[0].MergeRight = lastcolumn - 1;
                if (sb.IsSmallBusiness)
                    row.Cells[lastcolumn].AddParagraph(order.OrderCalculation.OrderRebate.AmountString);
                else
                    row.Cells[lastcolumn].AddParagraph(order.OrderCalculation.NetOrderRebate.AmountString);
                row.Format.SpaceBefore = "0,1cm";
                row.Cells[0].Borders.Bottom = NoBorder.Clone();
                row.Cells[lastcolumn].Borders.Bottom = NoBorder.Clone();
                row.Cells[0].Borders.Top = NoBorder.Clone();
                row.Cells[lastcolumn].Borders.Top = NoBorder.Clone();
            }

            if (!String.IsNullOrEmpty(order.OrderShipment.Name))
            {
                logger.Trace("Adding Shipment");
                row = table.AddRow();
                row.Cells[0].AddParagraph("Zzgl. " + order.OrderShipment.Name);
                row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[0].MergeRight = lastcolumn - 1;
                row.Cells[lastcolumn].AddParagraph(order.OrderCalculation.NetShipment.AmountString);
                row.Format.SpaceBefore = "0,1cm";
                row.Cells[0].Borders.Bottom = BlackBorder.Clone();
                row.Cells[lastcolumn].Borders.Bottom = BlackBorder.Clone();
                row.Cells[0].Borders.Top = NoBorder.Clone();
                row.Cells[lastcolumn].Borders.Top = NoBorder.Clone();
            }

            if (!sb.IsSmallBusiness)
            {
                if (order.OrderCalculation.OrderRebate.Amount > 0 || !String.IsNullOrEmpty(order.OrderShipment.Name))
                {
                    logger.Trace("Adding Subtotal");
                    row = table.AddRow();
                    row.Cells[0].AddParagraph("Zwischensumme Netto");
                    row.Cells[0].Format.Font.Bold = true;
                    row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                    row.Cells[0].MergeRight = lastcolumn - 1;
                    row.Cells[lastcolumn].AddParagraph(order.OrderCalculation.NetOrderSummary.AmountString);
                    row.Format.SpaceBefore = "0,1cm";
                    row.Cells[0].Borders.Bottom = NoBorder.Clone();
                    row.Cells[lastcolumn].Borders.Bottom = NoBorder.Clone();
                    row.Cells[0].Borders.Top = BlackBorder.Clone();
                    row.Cells[lastcolumn].Borders.Top = BlackBorder.Clone();
                }

                // Add the VAT row
                foreach (var tax in order.OrderCalculation.TaxValues)
                {
                    logger.Trace("Adding Tax - " + tax.TaxClass.Name);
                    row = table.AddRow();
                    row.Cells[0].AddParagraph("Zzgl. " + tax.TaxClass.Name + " " + tax.TaxClassAddition);
                    row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                    row.Cells[0].MergeRight = lastcolumn - 1;
                    row.Cells[lastcolumn].AddParagraph(tax.Value.AmountString);
                    row.Cells[0].Borders.Bottom = NoBorder.Clone();
                    row.Cells[lastcolumn].Borders.Bottom = NoBorder.Clone();
                    row.Cells[0].Borders.Top = NoBorder.Clone();
                    row.Cells[lastcolumn].Borders.Top = NoBorder.Clone();
                }
            }

            logger.Trace("Adding subtotal gross");
            row = table.AddRow();
            row.Cells[0].Borders.Bottom = BlackThickBorder.Clone();
            row.Cells[0].AddParagraph("Gesamtbetrag");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[0].MergeRight = lastcolumn - 1;
            row.Cells[lastcolumn].AddParagraph(order.OrderCalculation.OrderSummary.AmountString);
            row.Cells[lastcolumn].Borders.Bottom = BlackThickBorder.Clone();
            row.Format.SpaceBefore = "0,25cm";
            row.Format.SpaceAfter = "0,05cm";
        }

        private void CreatePostListContent(Section section, Order.Order order)
        {
            // Add payment paragraph
            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.AddText(ReplaceDocumentPlaceHolder(order.PaymentMethode.Text, order));

            // Add the notes paragraph
            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.AddText(ReplaceDocumentPlaceHolder(order.OrderClosingText, order));
        }

        readonly static Color TableBorder = new Color(0, 0, 0);

        public System.Windows.UIElement PreviewControl
        {
            get { return PreviewElement; }
            set { }
        }

        public async void RenderDocumentPreview(Biller.Core.Document.Document document)
        {
            if (document is Order.Order)
                PreviewElement.Ddl = DdlWriter.WriteToString(await GetDocument(document as Order.Order));
        }

        private async Task<MigraDoc.DocumentObjectModel.Document> GetDocument(Order.Order order)
        {
            return await FillContent(order);
        }

        public async void SaveDocument(Biller.Core.Document.Document document, string filename, bool OpenOnSuccess = true)
        {
            if (document is Order.Order)
            {
                PdfDocumentRenderer renderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.Always);
                renderer.Document = await GetDocument(document as Order.Order);

                renderer.RenderDocument();
                renderer.Save(filename);

                if (OpenOnSuccess)
                    Process.Start(filename);
            }
        }

        public void PrintDocument(Biller.Core.Document.Document document)
        {
            /*
            // Reuse the renderer from the preview
            RenderDocumentPreview(document);
            DocumentRenderer renderer = this.PreviewElement.Renderer;
            if (renderer != null)
            {
                int pageCount = renderer.FormattedDocument.PageCount;
                // Creates a PrintDocument that simplyfies printing of MigraDoc documents
                MigraDocPrintDocument printDocument = new MigraDocPrintDocument();

                if (PrintDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Attach the current printer settings
                    printDocument.PrinterSettings = this.PrintDialog.PrinterSettings;

                    if (this.PrintDialog.PrinterSettings.PrintRange == System.Drawing.Printing.PrintRange.Selection)
                        printDocument.SelectedPage = this.PreviewElement.Page;

                    renderer.PrepareDocument();

                    // Attach the current document renderer
                    printDocument.Renderer = renderer;
                    
                    // Print the document
                    printDocument.Print();
                }
            }
            */

            SaveDocument(document, document.DocumentType + document.DocumentID + ".pdf");
        }

        private string ReplaceArticlePlaceHolder(string placeholder, Biller.Core.Articles.OrderedArticle article)
        {
            if (placeholder == "{Position}")
                return article.OrderPosition.ToString();
            if (placeholder == "{Amount}")
                return article.OrderedAmountString;
            if (placeholder == "{ArticleID}")
                return article.ArticleID;
            if (placeholder == "{ArticleID}")
                return article.ArticleID;
            if (placeholder == "{ArticleName}")
                return article.ArticleDescription;
            if (placeholder == "{ArticleText}")
                return article.ArticleText;
            if (placeholder == "{ArticleNameWithText}")
                return article.ArticleDescription + "\n" + article.OrderText;
            if (placeholder == "{SinglePriceGross}")
                return article.OrderPrice.Price1.ToString();
            if (placeholder == "{SinglePriceNet}")
                return article.OrderPrice.Price2.ToString();
            if (placeholder == "{TaxRate}")
                return article.TaxClass.TaxRate.PercentageString;
            if (placeholder == "{OrderedValueGross}")
                return article.RoundedGrossOrderValue.ToString();
            if (placeholder == "{OrderedValueNet}")
                return article.RoundedNetOrderValue.ToString();
            if (placeholder == "{Rebate}")
                return article.OrderRebate.PercentageString;
            return placeholder;
        }

        private string ReplaceDocumentPlaceHolder(string placeholder, Order.Order order)
        {
            if (placeholder.Contains("{DocumentDate}"))
            {
                Regex.Replace(placeholder, "[{]DocumentDate[}][{][+]+[0-9]{1,}[}]", delegate(Match match)
                {
                    string v = match.ToString();
                    v = v.Replace("{Date}", "").Replace("{", "").Replace("}", "");
                    return order.Date.AddDays(int.Parse(v)).ToString("dd.MM.yyyy");
                });
                Regex.Replace(placeholder, "[{]DocumentDate[}][{][-]+[0-9]{1,}[}]", delegate(Match match)
                {
                    string v = match.ToString();
                    v = v.Replace("{DocumentDate}", "").Replace("{", "").Replace("}", "");
                    return order.Date.AddDays(-int.Parse(v)).ToString("dd.MM.yyyy");
                });
            }
            if (placeholder.Contains("{DateOfDerlivery}"))
            {
                Regex.Replace(placeholder, "[{]DateOfDerlivery[}][{][+]+[0-9]{1,}[}]", delegate(Match match)
                {
                    string v = match.ToString();
                    v = v.Replace("{Date}", "").Replace("{", "").Replace("}", "");
                    return order.DateOfDelivery.AddDays(int.Parse(v)).ToString("dd.MM.yyyy");
                });
                Regex.Replace(placeholder, "[{]DateOfDerlivery[}][{][-]+[0-9]{1,}[}]", delegate(Match match)
                {
                    string v = match.ToString();
                    v = v.Replace("{DateOfDerlivery}", "").Replace("{", "").Replace("}", "");
                    return order.DateOfDelivery.AddDays(-int.Parse(v)).ToString("dd.MM.yyyy");
                });
            }
            if (placeholder == "{OrderValueGross}")
                return order.OrderCalculation.OrderSummary.ToString();
            if (placeholder == "{OrderValueNet}")
                return order.OrderCalculation.NetOrderSummary.ToString();
            if (placeholder == "{CashBackValue}")
                return order.OrderCalculation.CashBack.ToString();
            return placeholder;
        }

        private string ReplaceFooterPlaceHolder(string placeholder, Biller.Core.Models.CompanySettings companySettings)
        {
            if (placeholder.Contains("{CompanyAddress}"))
            {
                var address = "";
                foreach (var line in companySettings.MainAddress.AddressStrings)
                    address += line + "\n";
                placeholder = placeholder.Replace("{CompanyAddress}", address);
            }
            placeholder = placeholder.Replace("{TaxID}", companySettings.TaxID);
            placeholder = placeholder.Replace("{SalesTaxID}", companySettings.SalesTaxID);
                
            return placeholder;
        }

        public List<string> AvailableDocumentTypes()
        {
            var output = new List<string>();
            output.Add("Invoice");
            output.Add("Docket");
            return output;
        }


        public string Name
        {
            get
            {
                return "Standardlayout für Rechnungen und Lieferscheine";
            }
        }

        public string GuID
        {
            get { return "bfeeab8f-c7fc-4560-8278-85de2a413d40"; }
        }
    }
}
