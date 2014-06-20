using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderTypes_Biller.Models
{
    public class FooterColumnModel
    {
        public ParagraphAlignment Alignment { get; set; }

        public int AlignmentIndex
        {
            get { return (int)Alignment; }
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

        public string Value { get; set; }
    }
}
