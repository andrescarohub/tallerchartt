using System;
using System.Collections.Generic;
using MySqlConnector;
using tallerc.domain.repositories;
using tallerc.infrastructure.mysql;

namespace tallerc.infrastructure.repositories
{
    /// <summary>
    /// Clase base para los repositorios que proporciona funcionalidad común
    /// </summary>
    /// <typeparam name="T">Tipo de entidad con la que trabaja el repositorio</typeparam>
    public abstract class BaseRepository<T> : IGenericRepository<T> where T : class
    {
        // Protegido para que las clases derivadas puedan utilizarlo
        protected readonly ConexionSingleton _conexion;
        
        // Nombre de la tabla en la base de datos
        protected readonly string _tableName;

        /// <summary>
        /// Constructor que inicializa la conexión a la base de datos y el nombre de la tabla
        /// </summary>
        /// <param name="tableName">Nombre de la tabla en la base de datos</param>
        protected BaseRepository(string tableName)
        {
            _conexion = ConexionSingleton.Instance;
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        /// <summary>
        /// Método abstracto para mapear un MySqlDataReader a una entidad T
        /// </summary>
        /// <param name="reader">Objeto MySqlDataReader con los datos</param>
        /// <returns>Entidad T creada a partir de los datos</returns>
        protected abstract T MapToEntity(MySqlDataReader reader);

        /// <summary>
        /// Obtiene todas las entidades de la tabla
        /// </summary>
        /// <returns>Lista de entidades</returns>
        public virtual List<T> GetAll()
        {
            List<T> entities = new List<T>();
            try
            {
                string query = $"SELECT * FROM {_tableName}";
                using (var reader = _conexion.ExecuteReader(query))
                {
                    while (reader.Read())
                    {
                        entities.Add(MapToEntity(reader));
                    }
                }
                return entities;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetAll: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una entidad por su ID
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <returns>Entidad encontrada o null</returns>
        public virtual T? GetById(int id)
        {
            try
            {
                string query = $"SELECT * FROM {_tableName} WHERE Id = @Id";
                MySqlParameter parameter = new MySqlParameter("@Id", id);
                
                using (var reader = _conexion.ExecuteReader(query, parameter))
                {
                    if (reader.Read())
                    {
                        return MapToEntity(reader);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetById: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe una entidad con el ID especificado
        /// </summary>
        /// <param name="id">ID a verificar</param>
        /// <returns>True si existe, False en caso contrario</returns>
        public virtual bool Exists(int id)
        {
            try
            {
                string query = $"SELECT COUNT(1) FROM {_tableName} WHERE Id = @Id";
                MySqlParameter parameter = new MySqlParameter("@Id", id);
                
                var result = _conexion.ExecuteScalar(query, parameter);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Exists: {ex.Message}");
                throw;
            }
        }

        // Los métodos Add, Update y Delete deben implementarse en las clases derivadas
        // ya que dependen de la estructura específica de cada entidad
        public abstract int Add(T entity);
        public abstract bool Update(T entity);
        public abstract bool Delete(int id);
    }
}