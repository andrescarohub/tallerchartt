using System;
using System.Collections.Generic;

namespace tallerc.domain.entities
{
    /// <summary>
    /// Representa una compra realizada a un proveedor
    /// </summary>
    public class Compra
    {
        /// <summary>
        /// Identificador único de la compra
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador del proveedor al que se realizó la compra
        /// </summary>
        public int ProveedorId { get; set; }

        /// <summary>
        /// Identificador del empleado que registró la compra
        /// </summary>
        public int EmpleadoId { get; set; }

        /// <summary>
        /// Fecha en que se realizó la compra
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Número de factura o documento de la compra
        /// </summary>
        public string NumeroFactura { get; set; }

        /// <summary>
        /// Observaciones adicionales sobre la compra
        /// </summary>
        public string Observaciones { get; set; }

        /// <summary>
        /// Estado de la compra (1: Pendiente, 2: Completada, 3: Cancelada)
        /// </summary>
        public int Estado { get; set; }

        /// <summary>
        /// Colección de detalles de la compra
        /// </summary>
        public List<DetalleCompra> Detalles { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Compra()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            Fecha = DateTime.Now;
            Estado = 1; // Por defecto, la compra está pendiente
            Detalles = new List<DetalleCompra>();
        }

        /// <summary>
        /// Calcula el total de la compra sumando todos los detalles
        /// </summary>
        /// <returns>Monto total de la compra</returns>
        public decimal CalcularTotal()
        {
            decimal total = 0;
            
            if (Detalles != null && Detalles.Count > 0)
            {
                foreach (var detalle in Detalles)
                {
                    total += detalle.Total;
                }
            }
            
            return total;
        }

        /// <summary>
        /// Agrega un nuevo detalle a la compra
        /// </summary>
        /// <param name="detalle">Detalle a agregar</param>
        public void AgregarDetalle(DetalleCompra detalle)
        {
            if (detalle != null)
            {
                Detalles.Add(detalle);
            }
        }

        /// <summary>
        /// Valida que los campos obligatorios estén completos
        /// </summary>
        /// <returns>True si la entidad es válida, False en caso contrario</returns>
        public bool EsValida()
        {
            return ProveedorId > 0 && 
                   EmpleadoId > 0 && 
                   !string.IsNullOrEmpty(NumeroFactura) && 
                   Detalles != null && 
                   Detalles.Count > 0;
        }
    }
}