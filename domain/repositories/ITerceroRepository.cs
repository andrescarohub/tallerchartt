using System.Collections.Generic;
using tallerc.domain.entities;

namespace tallerc.domain.repositories
{
    /// <summary>
    /// Interfaz para el repositorio de terceros, extiende las operaciones genéricas
    /// y agrega operaciones específicas para la entidad Tercero
    /// </summary>
    public interface ITerceroRepository : IGenericRepository<Tercero>
    {
        /// <summary>
        /// Obtiene todos los terceros de un tipo específico
        /// </summary>
        /// <param name="tipoTerceroId">ID del tipo de tercero (1: Cliente, 2: Proveedor, 3: Empleado)</param>
        /// <returns>Lista de terceros del tipo especificado</returns>
        List<Tercero> GetByTipo(int tipoTerceroId);

        /// <summary>
        /// Busca terceros cuyo nombre o apellido contengan el texto especificado
        /// </summary>
        /// <param name="nombre">Texto a buscar en el nombre o apellido</param>
        /// <returns>Lista de terceros que cumplen con el criterio de búsqueda</returns>
        List<Tercero> SearchByNombre(string nombre);

        /// <summary>
        /// Busca un tercero por su número de documento
        /// </summary>
        /// <param name="numeroDocumento">Número de documento a buscar</param>
        /// <returns>Tercero encontrado o null si no existe</returns>
        Tercero GetByDocumento(string numeroDocumento);

        /// <summary>
        /// Obtiene todos los proveedores (tipo de tercero 2)
        /// </summary>
        /// <returns>Lista de proveedores</returns>
        List<Tercero> GetProveedores();

        /// <summary>
        /// Obtiene todos los empleados (tipo de tercero 3)
        /// </summary>
        /// <returns>Lista de empleados</returns>
        List<Tercero> GetEmpleados();
    }
}