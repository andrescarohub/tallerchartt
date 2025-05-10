using System;
using System.Collections.Generic;

namespace tallerc.domain.repositories
{
    /// <summary>
    /// Interfaz genérica que define las operaciones CRUD básicas para cualquier entidad
    /// </summary>
    /// <typeparam name="T">Tipo de entidad con la que trabaja el repositorio</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <returns>Lista de entidades</returns>
        List<T> GetAll();

        /// <summary>
        /// Obtiene una entidad por su identificador
        /// </summary>
        /// <param name="id">Identificador de la entidad</param>
        /// <returns>Entidad encontrada o null si no existe</returns>
        T GetById(int id);

        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <returns>Identificador de la entidad agregada</returns>
        int Add(T entity);

        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        /// <param name="entity">Entidad con los datos actualizados</param>
        /// <returns>True si la operación fue exitosa</returns>
        bool Update(T entity);

        /// <summary>
        /// Elimina una entidad por su identificador
        /// </summary>
        /// <param name="id">Identificador de la entidad a eliminar</param>
        /// <returns>True si la operación fue exitosa</returns>
        bool Delete(int id);

        /// <summary>
        /// Verifica si existe una entidad con el identificador especificado
        /// </summary>
        /// <param name="id">Identificador a verificar</param>
        /// <returns>True si existe, False en caso contrario</returns>
        bool Exists(int id);
    }
}