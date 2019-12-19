using QuickFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFix.Fields;
using QuickFix.FIX50;
using SecurityStatus = QuickFix.FIX50.SecurityStatus;
using OrderRoutingFixClient.QuickFixExtensions;
using IOLEntities;

namespace OrderRoutingFixClient
{
    public partial class FixInitiator : MessageCracker
    {

        private static string ObtenerIdFix(ExecutionReport reporteEjecucion)
        {
            return reporteEjecucion.OrderID.getValue();
        }

        private static string ObtenerIdTransaccionContraparte(ExecutionReport reporteEjecucion)
        {
            return reporteEjecucion.GetField(new NumericOrderID(NumericOrderID.NumericOrderIDTag)).getValue();
        }

        private void NotificarRechazoOrden(Transaccion transaccion, int idError, string msjError)
        {
            var automatizacionService = new AutomatizacionServices();
            automatizacionService.LoguearErrorIngresoOrden(transaccion.ID, idError, msjError);
            automatizacionService.BloquearTransaccion(transaccion.ID);
            _interfacePresenter.MostrarMensaje($"Error al enviar la transacción {transaccion.ID}: error {idError}");
        }

        private int GetTransaccionIDbyIdFix(string idFix)
        {
            var idTransaccion = TransaccionesServices.GetTransaccionIdByIdFix(idFix);
            return idTransaccion;
        }

        private Transaccion GetTransaccion(int idTransaccion, bool forzarTraerDesdeDB = false)
        {
            var transaccion = _transaccionesEnviadas.FirstOrDefault(x => x.ID == idTransaccion);

            if (transaccion == null || forzarTraerDesdeDB)
            {
                transaccion = TransaccionesServices.GetTransaccionByID(idTransaccion);
                AgregarTransaccionAListaEnviadas(transaccion);
            }
            return transaccion;
        }
        private Transaccion GetTransaccionByIdFix(string idFix)
        {
            Transaccion transaccion;
            
            var idTransaccion = TransaccionesServices.GetTransaccionIdByIdFix(idFix);
            if (idTransaccion == 0)
                return null;
            transaccion = TransaccionesServices.GetTransaccionByID(idTransaccion);
            AgregarTransaccionAListaEnviadas(transaccion);
            return transaccion;
        }

        private void AgregarTransaccionAListaEnviadas(Transaccion transaccion)
        {
            _transaccionesEnviadas.RemoveAll(x => x.ID == transaccion.ID);
            _transaccionesEnviadas.Add(transaccion);
        }

        private void RemoverTransaccionDeListaEnviadas(Transaccion transaccion)
        {
            _transaccionesEnviadas.RemoveAll(x => x.ID == transaccion.ID);
        }

    }
}
