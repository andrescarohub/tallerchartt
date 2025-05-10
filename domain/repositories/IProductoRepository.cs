
using System.Collections.Generic;
using tallerc.domain.entities;

namespace tallerc.domain.repositories
{
    /// <summary>
    /// Interfaz para el repositorio de productos, extiende las operaciones genéricas
    /// y agrega operaciones específicas para la entidad Producto
    /// </summary>
    public interface IProductoRepository : IGenericRepository<Producto>
    {
        /// <summary>
        /// Obtiene los productos con stock por debajo del mínimo
        /// </summary>
        /// <returns>Lista de productos con stock bajo</returns>
        List<Producto> GetWithLowStock();

        /// <summary>
        /// Actualiza el stock de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidad">Cantidad a agregar (o restar si es negativa)</param>
        /// <returns>True si la operación fue exitosa</returns>
        bool UpdateStock(int productoId, int cantidad);

        /// <summary>
        /// Obtiene productos asociados a un plan específico
        /// </summary>
        /// <param name="planId">ID del plan</param>
        /// <returns>Lista de productos asociados al plan</returns>
        List<Producto> GetByPlan(int planId);

        /// <summary>
        /// Busca productos por nombre o código de barras
        /// </summary>
        /// <param name="texto">Texto a buscar en el nombre o código de barras</param>
        /// <returns>Lista de productos que cumplen con el criterio de búsqueda</returns>
        List<Producto> Search(string texto);

        /// <summary>
        /// Busca un producto por su código de barras
        /// </summary>
        /// <param name="barcode">Código de barras a buscar</param>
        /// <returns>Producto encontrado o null si no existe</returns>
        Producto GetByBarcode(string barcode);
    }
}