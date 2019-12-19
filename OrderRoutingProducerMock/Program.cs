using OrderRoutingFixClient;
using QuickFix;
using QuickFix.FIX50;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRoutingProducerMock
{
    class Program
    {
        static void Main(string[] args)
        {
            //compra ejemplo
            string fixString = "8=FIXT.1.19=23035=D34=249=tiol_0252=20170816-21:14:37.01756=STUN128=FGW1=6695911=456070415=ARS38=300.000040=244=9.820054=155=AGRO59=060=20170816-18:13:47.50363=4167=CS207=XMEV528=A1138=300.0000453=1448=USU1512447=D452=5310=082";
            var fixmsg = CrearNewOrderSinglePorString(fixString);

        }

        public static NewOrderSingle CrearNewOrderSinglePorString(string fixString)
        {
            var defaultMsgFactory = new DefaultMessageFactory();
            var dataDictionary = new QuickFix.DataDictionary.DataDictionary();
            //https://www.tradingtechnologies.com/xtrader-help/apis/fix-adapter/fix-adapter-resources/
            dataDictionary.Load(AppDomain.CurrentDomain.BaseDirectory + "\\FIX42.xml");
            var mensaje = new NewOrderSingle();
            mensaje.FromString(fixString, false, dataDictionary, dataDictionary, defaultMsgFactory);
            return mensaje;
        }
    }
}
