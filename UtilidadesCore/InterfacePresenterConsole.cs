using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilidadesCore
{
    public class InterfacePresenterConsole : IInterfacePresenter
    {
        public void MostrarMensaje(string text)
        {
            Console.WriteLine(text);
        }
    }
}
