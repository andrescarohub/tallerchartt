using System;
using System.Collections.Generic;
using tallerc.domain.entities;

namespace tallerc.domain.repositories
{
    /// <summary>
    /// Interfaz para el repositorio de compras, extiende las operaciones genéricas
    /// y agrega operaciones específicas para la entidad Compra
    /// </summary>
    public interface ICompraRepository : IGenericRepository<Compra>
    {
        /// <summary>
        /// Obtiene las compras realizadas a un proveedor específico
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <returns>Lista de compras realizadas al proveedor</returns>
        List<Compra> GetByProveedor(int proveedorId);

        /// <summary>
        /// Obtiene las compras realizadas en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del rango</param>
        /// <param name="fechaFin">Fecha de fin del rango</param>
        /// <returns>Lista de compras en el rango de fechas</returns>
        List<Compra> GetByFechas(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene las compras por estado
        /// </summary>
        /// <param name="estado">Estado de las compras (1: Pendiente, 2: Completada, 3: Cancelada)</param>
        /// <returns>Lista de compras en el estado especificado</returns>
        List<Compra> GetByEstado(int estado);

        /// <summary>
        /// Agrega una compra con sus detalles en una transacción
        /// </summary>
        /// <param name="compra">Compra con sus detalles</param>
        /// <returns>ID de la compra creada o -1 si falla</returns>
        int AddCompraConDetalles(Compra compra);

        /// <summary>
        /// Actualiza el estado de una compra
        /// </summary>
        /// <param name="compraId">ID de la compra</param>
        /// <param name="estado">Nuevo estado</param>
        /// <returns>True si la operación fue exitosa</returns>
        bool UpdateEstado(int compraId, int estado);

        /// <summary>
        /// Obtiene una compra con sus detalles
        /// </summary>
        /// <param name="compraId">ID de la compra</param>
        /// <returns>Compra con sus detalles o null si no existe</returns>
        Compra GetCompraConDetalles(int compraId);
    }
}