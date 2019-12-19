using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFix.Fields;

namespace OrderRoutingFixClient.QuickFixExtensions
{
    public class NumericOrderID : StringField
    {
        public const int NumericOrderIDTag = 29500;

        public NumericOrderID(int tag) : base(tag)
        {
        }

        public NumericOrderID(int tag, string str) : base(tag, str)
        {
        }
    }
}
