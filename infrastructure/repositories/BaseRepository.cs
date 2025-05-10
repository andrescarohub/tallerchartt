using System;
using System.Collections.Generic;
using MySqlConnector;
using tallerc.domain.repositories; // Asegúrate que IGenericRepository está aquí
using tallerc.infrastructure.mysql;

namespace tallerc.infrastructure.repositories
{
    public abstract class BaseRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ConexionSingleton _db; // Renombrado para claridad, es la instancia del Singleton
        protected readonly string _tableName;

        protected BaseRepository(string tableName)
        {
            _db = ConexionSingleton.Instance; // Obtiene la instancia del Singleton
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        protected abstract T MapToEntity(MySqlDataReader reader);

        public virtual List<T> GetAll()
        {
            List<T> entities = new List<T>();
            try
            {
                string query = $"SELECT * FROM {_tableName}";
                // ExecuteReader ahora maneja su propia conexión y la cierra.
                using (var reader = _db.ExecuteReader(query))
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
                Console.WriteLine($"Error en BaseRepository.GetAll para {_tableName}: {ex.Message}");
                throw; // O manejar de otra forma
            }
        }

        public virtual T? GetById(int id) // T? para indicar que puede ser null
        {
            try
            {
                string query = $"SELECT * FROM {_tableName} WHERE Id = @Id";
                MySqlParameter parameter = new MySqlParameter("@Id", id);
                
                using (var reader = _db.ExecuteReader(query, parameter))
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
                Console.WriteLine($"Error en BaseRepository.GetById para {_tableName}: {ex.Message}");
                throw;
            }
        }

        public virtual bool Exists(int id)
        {
            try
            {
                string query = $"SELECT COUNT(1) FROM {_tableName} WHERE Id = @Id";
                MySqlParameter parameter = new MySqlParameter("@Id", id);
                
                var result = _db.ExecuteScalar(query, parameter); // Usa la versión sin transacción explícita
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en BaseRepository.Exists para {_tableName}: {ex.Message}");
                throw;
            }
        }

        // Add, Update, Delete siguen siendo abstractos porque son muy específicos
        // y a menudo requieren manejo de transacciones que BaseRepository no impone.
        public abstract int Add(T entity);
        public abstract bool Update(T entity);
        public abstract bool Delete(int id);
    }
}