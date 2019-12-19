using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOLEntities
{
    public enum EstadoTransacciones
    {
        Iniciada = 1,
        EnProceso = 2,
        ParcialmenteTerminada = 3,
        Terminada = 4,
        Cancelada = 5,
        PendienteCancelacion = 6,
        CanceladaPorVencimientoValidez = 7,
        ParcialmenteTerminadaConPedidoCancelacion = 8,
        EnModificacion = 9
    }
}
