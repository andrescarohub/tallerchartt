using System;
using System.Collections.Generic;
using tallerc.domain.entities;
using tallerc.domain.repositories;

namespace tallerc.domain.services
{
    /// <summary>
    /// Servicio que encapsula la lógica de negocio relacionada con los productos
    /// </summary>
    public class ProductoService
    {
        private readonly IProductoRepository _productoRepository;

        /// <summary>
        /// Constructor que inyecta el repositorio de productos
        /// </summary>
        /// <param name="productoRepository">Instancia del repositorio de productos</param>
        public ProductoService(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
        }

        /// <summary>
        /// Obtiene todos los productos
        /// </summary>
        /// <returns>Lista de productos</returns>
        public List<Producto> GetAllProductos()
        {
            return _productoRepository.GetAll();
        }

        /// <summary>
        /// Obtiene productos con stock bajo
        /// </summary>
        /// <returns>Lista de productos con stock por debajo del mínimo</returns>
        public List<Producto> GetProductosConStockBajo()
        {
            return _productoRepository.GetWithLowStock();
        }

        /// <summary>
        /// Busca productos por nombre o código de barras
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Lista de productos que coinciden con la búsqueda</returns>
        public List<Producto> SearchProductos(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Producto>();

            return _productoRepository.Search(searchTerm);
        }

        /// <summary>
        /// Obtiene un producto por su ID
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Producto encontrado o null</returns>
        public Producto GetProductoById(int id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return _productoRepository.GetById(id);
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Obtiene un producto por su código de barras
        /// </summary>
        /// <param name="barcode">Código de barras</param>
        /// <returns>Producto encontrado o null</returns>
        public Producto? GetProductoByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return null;

            return _productoRepository.GetByBarcode(barcode);
        }

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        /// <param name="producto">Datos del producto a crear</param>
        /// <returns>ID del producto creado o -1 si hay error</returns>
        public int CreateProducto(Producto producto)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            if (!producto.EsValido())
                return -1;

            // Verificar si ya existe un producto con el mismo código de barras
            if (!string.IsNullOrEmpty(producto.Barcode))
            {
                var existente = _productoRepository.GetByBarcode(producto.Barcode);
                if (existente != null)
                    return -1;
            }

            // Establecer fechas
            producto.CreatedAt = DateTime.Now;
            producto.UpdatedAt = DateTime.Now;

            return _productoRepository.Add(producto);
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        /// <param name="producto">Datos actualizados del producto</param>
        /// <returns>True si la actualización fue exitosa</returns>
        public bool UpdateProducto(Producto producto)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            if (!producto.EsValido() || producto.Id <= 0)
                return false;

            // Verificar si existe el producto
            if (!_productoRepository.Exists(producto.Id))
                return false;

            // Actualizar fecha de modificación
            producto.UpdatedAt = DateTime.Now;

            return _productoRepository.Update(producto);
        }

        /// <summary>
        /// Actualiza el stock de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidad">Cantidad a agregar (positivo) o restar (negativo)</param>
        /// <returns>True si la actualización fue exitosa</returns>
        public bool UpdateStock(int productoId, int cantidad)
        {
            if (productoId <= 0)
                return false;

            var producto = _productoRepository.GetById(productoId);
            if (producto == null)
                return false;

            // Verificar que el stock no quede negativo
            if (producto.StockActual + cantidad < 0)
                return false;

            return _productoRepository.UpdateStock(productoId, cantidad);
        }

        /// <summary>
        /// Elimina un producto por su ID
        /// </summary>
        /// <param name="id">ID del producto a eliminar</param>
        /// <returns>True si la eliminación fue exitosa</returns>
        public bool DeleteProducto(int id)
        {
            if (id <= 0)
                return false;

            return _productoRepository.Delete(id);
        }

        /// <summary>
        /// Verifica si un producto requiere reposición de stock
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>True si el stock actual está por debajo del mínimo</returns>
        public bool RequiereReposicion(int id)
        {
            var producto = _productoRepository.GetById(id);
            return producto != null && producto.RequiereReposicion();
        }
    }
}