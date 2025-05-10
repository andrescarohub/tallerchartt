// infrastructure/repositories/BaseRepository.cs
using System;
using System.Collections.Generic;
using MySqlConnector;
using tallerc.domain.repositories; // Asegúrate que IGenericRepository está aquí
using tallerc.infrastructure.mysql;

namespace tallerc.infrastructure.repositories
{
    public abstract class BaseRepository<T> : IGenericRepository<T> where T : class
    {
        // Volvemos a usar _conexion para la instancia del Singleton
        protected readonly ConexionSingleton _conexion; 
        protected readonly string _tableName;

        protected BaseRepository(string tableName)
        {
            _conexion = ConexionSingleton.Instance; // Obtiene la instancia del Singleton
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
                using (var reader = _conexion.ExecuteReader(query)) // Usamos _conexion
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

        // Asegúrate de que IGenericRepository.GetById devuelva T?
        public virtual T? GetById(int id) 
        {
            try
            {
                string query = $"SELECT * FROM {_tableName} WHERE Id = @Id";
                MySqlParameter parameter = new MySqlParameter("@Id", id);
                
                using (var reader = _conexion.ExecuteReader(query, parameter)) // Usamos _conexion
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
                
                var result = _conexion.ExecuteScalar(query, parameter); // Usamos _conexion
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en BaseRepository.Exists para {_tableName}: {ex.Message}");
                throw;
            }
        }

        public abstract int Add(T entity);
        public abstract bool Update(T entity);
        public abstract bool Delete(int id);
    }
}