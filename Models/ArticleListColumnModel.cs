using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderTypes_Biller.Models
{
    public class ArticleListColumnModel : Biller.Data.Utils.PropertyChangedHelper
    {
        public string Header { get; set; }

        public string Content { get; set; }

        public ParagraphAlignment Alignment { get; set; }

        public int AlignmentIndex { get { return (int)Alignment; }
            set
            {
                if (value == 0)
                    Alignment = ParagraphAlignment.Left;
                if (value == 1)
                    Alignment = ParagraphAlignment.Center;
                if (value == 2)
                    Alignment = ParagraphAlignment.Right;
            }
        }

        public double ColumnWidth { get; set; }
    }
}
