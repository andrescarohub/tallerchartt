
using System.Collections.Generic;
using tallerc.domain.entities;

namespace tallerc.domain.repositories
{
    /// <summary>
    /// Interfaz para el repositorio de detalles de compra, extiende las operaciones genéricas
    /// y agrega operaciones específicas para la entidad DetalleCompra
    /// </summary>
    public interface IDetalleCompraRepository : IGenericRepository<DetalleCompra>
    {
        /// <summary>
        /// Obtiene todos los detalles de una compra específica
        /// </summary>
        /// <param name="compraId">ID de la compra</param>
        /// <returns>Lista de detalles de la compra</returns>
        List<DetalleCompra> GetByCompra(int compraId);

        /// <summary>
        /// Obtiene todos los detalles de compra que incluyen un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>Lista de detalles de compra del producto</returns>
        List<DetalleCompra> GetByProducto(int productoId);

        /// <summary>
        /// Agrega múltiples detalles de compra en una sola operación
        /// </summary>
        /// <param name="detalles">Lista de detalles a agregar</param>
        /// <param name="compraId">ID de la compra a la que pertenecen</param>
        /// <returns>True si la operación fue exitosa</returns>
        bool AddDetalles(List<DetalleCompra> detalles, int compraId);

        /// <summary>
        /// Elimina todos los detalles de una compra
        /// </summary>
        /// <param name="compraId">ID de la compra</param>
        /// <returns>True si la operación fue exitosa</returns>
        bool DeleteByCompra(int compraId);
    }
}