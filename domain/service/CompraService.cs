using System;
using System.Collections.Generic;
using tallerc.domain.entities;
using tallerc.domain.repositories;

namespace tallerc.domain.services
{
    /// <summary>
    /// Servicio que encapsula la lógica de negocio relacionada con las compras
    /// </summary>
    public class CompraService
    {
        private readonly ICompraRepository _compraRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly ITerceroRepository _terceroRepository;

        /// <summary>
        /// Constructor que inyecta los repositorios necesarios
        /// </summary>
        /// <param name="compraRepository">Repositorio de compras</param>
        /// <param name="productoRepository">Repositorio de productos</param>
        /// <param name="terceroRepository">Repositorio de terceros</param>
        public CompraService(
            ICompraRepository compraRepository,
            IProductoRepository productoRepository,
            ITerceroRepository terceroRepository)
        {
            _compraRepository = compraRepository ?? throw new ArgumentNullException(nameof(compraRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _terceroRepository = terceroRepository ?? throw new ArgumentNullException(nameof(terceroRepository));
        }

        /// <summary>
        /// Obtiene todas las compras
        /// </summary>
        /// <returns>Lista de compras</returns>
        public List<Compra> GetAllCompras()
        {
            return _compraRepository.GetAll();
        }

        /// <summary>
        /// Obtiene las compras en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Lista de compras en el rango especificado</returns>
        public List<Compra> GetComprasPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            // Asegurar que la fecha de fin sea mayor o igual a la de inicio
            if (fechaFin < fechaInicio)
            {
                var temp = fechaInicio;
                fechaInicio = fechaFin;
                fechaFin = temp;
            }

            return _compraRepository.GetByFechas(fechaInicio, fechaFin);
        }

        /// <summary>
        /// Obtiene una compra por su ID, incluyendo sus detalles
        /// </summary>
        /// <param name="id">ID de la compra</param>
        /// <returns>Compra con sus detalles o null</returns>
        public Compra GetCompraById(int id)
        {
            if (id <= 0)
                return null;

            return _compraRepository.GetCompraConDetalles(id);
        }

        /// <summary>
        /// Crea una nueva compra con sus detalles
        /// </summary>
        /// <param name="compra">Datos de la compra</param>
        /// <returns>ID de la compra creada o -1 si hay error</returns>
        public int CreateCompra(Compra compra)
        {
            if (compra == null)
                throw new ArgumentNullException(nameof(compra));

            if (!ValidarCompra(compra))
                return -1;

            // Crear la compra con sus detalles en una transacción
            int compraId = _compraRepository.AddCompraConDetalles(compra);

            if (compraId > 0)
            {
                // Actualizar el stock de cada producto
                foreach (var detalle in compra.Detalles)
                {
                    _productoRepository.UpdateStock(detalle.ProductoId, detalle.Cantidad);
                }
            }

            return compraId;
        }

        /// <summary>
        /// Cancela una compra
        /// </summary>
        /// <param name="compraId">ID de la compra a cancelar</param>
        /// <returns>True si la cancelación fue exitosa</returns>
        public bool CancelarCompra(int compraId)
        {
            if (compraId <= 0)
                return false;

            var compra = _compraRepository.GetCompraConDetalles(compraId);
            if (compra == null || compra.Estado == 3) // 3: Cancelada
                return false;

            // Si la compra ya estaba completada, revertir el stock
            if (compra.Estado == 2) // 2: Completada
            {
                foreach (var detalle in compra.Detalles)
                {
                    _productoRepository.UpdateStock(detalle.ProductoId, -detalle.Cantidad);
                }
            }

            // Actualizar el estado a cancelada
            return _compraRepository.UpdateEstado(compraId, 3);
        }

        /// <summary>
        /// Completa una compra pendiente
        /// </summary>
        /// <param name="compraId">ID de la compra a completar</param>
        /// <returns>True si la operación fue exitosa</returns>
        public bool CompletarCompra(int compraId)
        {
            if (compraId <= 0)
                return false;

            var compra = _compraRepository.GetById(compraId);
            if (compra == null || compra.Estado != 1) // 1: Pendiente
                return false;

            return _compraRepository.UpdateEstado(compraId, 2); // 2: Completada
        }

        /// <summary>
        /// Valida que los datos de una compra sean correctos
        /// </summary>
        /// <param name="compra">Compra a validar</param>
        /// <returns>True si la compra es válida</returns>
        private bool ValidarCompra(Compra compra)
        {
            if (!compra.EsValida())
                return false;

            // Verificar que exista el proveedor
            var proveedor = _terceroRepository.GetById(compra.ProveedorId);
            if (proveedor == null || proveedor.TipoTerceroId != 2) // 2: Proveedor
                return false;

            // Verificar que exista el empleado
            var empleado = _terceroRepository.GetById(compra.EmpleadoId);
            if (empleado == null || empleado.TipoTerceroId != 3) // 3: Empleado
                return false;

            // Verificar cada detalle
            foreach (var detalle in compra.Detalles)
            {
                if (!detalle.EsValido())
                    return false;

                // Verificar que exista el producto
                var producto = _productoRepository.GetById(detalle.ProductoId);
                if (producto == null)
                    return false;

                // Verificar que no se exceda el stock máximo
                if (!producto.PuedeAumentarStock(detalle.Cantidad))
                    return false;
            }

            return true;
        }
    }
}