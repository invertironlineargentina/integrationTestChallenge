using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilidadesCore;

namespace OrderRoutingQueueConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var interfacePresenter = new InterfacePresenterConsole();

            var consumidorConcertadorOrdenes = new ConsumidorConcertadorOrdenes(interfacePresenter);
            consumidorConcertadorOrdenes.Iniciar();

            string comando = string.Empty;
            interfacePresenter.MostrarMensaje("Iniciando Concertador de órdenes");
            do
            {
                interfacePresenter.MostrarMensaje("Escriba exit para salir.");
                comando = Console.ReadLine();

            } while (comando != "exit");
        }
    }
}
