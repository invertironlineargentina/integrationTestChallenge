using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFix.Fields;

namespace OrderRoutingFixClient.QuickFixExtensions
{
    public class SecondaryTradeIDCustom : StringField
    {
        public const int SecondaryTradeIDTag = 1040;

        public SecondaryTradeIDCustom(int tag) : base(tag)
        {
        }

        public SecondaryTradeIDCustom(int tag, string str) : base(tag, str)
        {
        }
    }
}
