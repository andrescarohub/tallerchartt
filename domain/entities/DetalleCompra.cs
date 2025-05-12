using System;

namespace tallerc.domain.entities
{
    /// <summary>
    /// Representa un detalle o línea de una compra realizada
    /// </summary>
    public class DetalleCompra
    {
        /// <summary>
        /// Identificador único del detalle de compra
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador de la compra a la que pertenece este detalle
        /// </summary>
        public int CompraId { get; set; }

        /// <summary>
        /// Identificador del producto adquirido
        /// </summary>
        public int ProductoId { get; set; }

        /// <summary>
        /// Cantidad de unidades adquiridas
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Valor unitario del producto en el momento de la compra
        /// </summary>
        public decimal Valor { get; set; }

        /// <summary>
        /// Producto asociado a este detalle (para facilitar el acceso a sus propiedades)
        /// </summary>
        public Producto Producto { get; set; }

        /// <summary>
        /// Calcula el total del detalle (Cantidad * Valor)
        /// </summary>
        public decimal Total => Cantidad * Valor;

        /// <summary>
        /// Constructor por defecto
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public DetalleCompra()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            Cantidad = 1;
            Valor = 0;
        }

        /// <summary>
        /// Constructor con parámetros
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidad">Cantidad de unidades</param>
        /// <param name="valor">Valor unitario</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public DetalleCompra(int productoId, int cantidad, decimal valor)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            ProductoId = productoId;
            Cantidad = cantidad;
            Valor = valor;
        }

        /// <summary>
        /// Valida que los campos obligatorios estén completos
        /// </summary>
        /// <returns>True si la entidad es válida, False en caso contrario</returns>
        public bool EsValido()
        {
            return ProductoId > 0 && 
                   Cantidad > 0 && 
                   Valor > 0;
        }
    }
}