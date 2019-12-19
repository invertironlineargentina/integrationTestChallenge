using IOLDAO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOLEntities
{
    public class TransaccionesServices
    {
        private IOLDataAccessApplicationBlock _dao;

        public TransaccionesServices()
        {
            _dao = new IOLDataAccessApplicationBlock();
        }

        public Transaccion GetTransaccionByID(int idTransaccion)
        {
            var transaccion = new Transaccion();
            var result = _dao.ExecuteQuery($"select * from ttr_transaccion where id_transaccion = {idTransaccion}");
            if (result.Tables[0].Rows.Count > 0)
            {
                DataRow dr = result.Tables[0].Rows[0];
                transaccion = MapearTransaccion(dr);
            }

            return transaccion;
        }

        public int GetTransaccionIdByIdFix(string idFix)
        {
            var transaccion = new Transaccion();
            var result = _dao.ExecuteQuery($"select * from ttr_transaccion where idfix = {idFix}");
            if (result.Tables[0].Rows.Count > 0)
            {
                DataRow dr = result.Tables[0].Rows[0];
                transaccion = MapearTransaccion(dr);
            }
            return transaccion.ID;
        }

        public List<Transaccion> RecibirTransacciones()
        {
            var lista = new List<Transaccion>();
            var result = _dao.ExecuteQuery($"select * from ttr_transacciones_nuevas");
            foreach (DataRow dr in result.Tables[0].Rows)
            {
                var transaccion = MapearTransaccion(dr);
                lista.Add(transaccion);
            }
            return lista;
        }

        public void VincularTransaccionConIdFix(Transaccion transaccion, string idFix)
        {
            var result = _dao.ExecuteQuery($"update ttr_transaccion set idfix = {idFix} where id_transaccion = {transaccion.ID}");
        }

        public Transaccion MapearTransaccion(DataRow dr)
        {
            var transaccion = new Transaccion();
            transaccion.ID = Convert.ToInt32(dr["id_transaccion"]);
            transaccion.IDFix = dr["idFix"].ToString();
            transaccion.Estado = (EstadoTransacciones)(Convert.ToInt32(dr["estado"]));
            transaccion.Simbolo = dr["simbolo"].ToString();
            transaccion.Cantidad = Convert.ToDecimal(dr["cantidad"]);
            transaccion.CantidadConcertada = Convert.ToDecimal(dr["cantidadConcertada"]);
            transaccion.FechaOrden = Convert.ToDateTime(dr["fechaOrden"]);
            return transaccion;
        }

        public List<string> ObtenerPartidas(int idTransaccion)
        {
            var lista = new List<string>();
            lista.Add(idTransaccion + "10");
            lista.Add(idTransaccion + "20");
            return lista;
        }
    }
}
