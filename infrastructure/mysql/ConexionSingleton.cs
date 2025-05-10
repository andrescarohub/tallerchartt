using System;
using MySqlConnector;

namespace tallerc.infrastructure.mysql
{
    /// <summary>
    /// Implementación del patrón Singleton para la conexión a la base de datos MySQL.
    /// Garantiza que solo exista una instancia de la conexión en toda la aplicación.
    /// </summary>
    public sealed class ConexionSingleton
    {
        // Instancia única
        private static ConexionSingleton? _instance;
        
        // Objeto de conexión a MySQL
        private MySqlConnection _connection;
        
        // String de conexión
        private readonly string _connectionString;
        
        // Objeto de bloqueo para garantizar la creación segura en entornos multi-hilo
        private static readonly object _lock = new object();

        /// <summary>
        /// Constructor privado para evitar instanciación directa
        /// </summary>
        private ConexionSingleton()
        {
            _connectionString = "Server=localhost;Database=tallerdechart;User=root;Password=1234;";
            _connection = new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// Obtiene la instancia única de la conexión (Singleton)
        /// </summary>
        public static ConexionSingleton Instance
        {
            get
            {
                // Doble verificación para mejorar el rendimiento
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConexionSingleton();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Obtiene la conexión a la base de datos
        /// </summary>
        /// <returns>Objeto MySqlConnection activo</returns>
        public MySqlConnection GetConnection()
        {
            try
            {
                // Si la conexión está cerrada, abrirla
                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }
                return _connection;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al conectar a la base de datos: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cierra la conexión a la base de datos si está abierta
        /// </summary>
        public void CloseConnection()
        {
            try
            {
                if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al cerrar la conexión: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una consulta SQL que no retorna datos (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="query">Consulta SQL a ejecutar</param>
        /// <param name="parameters">Parámetros para la consulta</param>
        /// <returns>Número de filas afectadas</returns>
        public int ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            try
            {
                using (var cmd = new MySqlCommand(query, GetConnection()))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al ejecutar consulta: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una consulta SQL que retorna un único valor
        /// </summary>
        /// <param name="query">Consulta SQL a ejecutar</param>
        /// <param name="parameters">Parámetros para la consulta</param>
        /// <returns>Resultado de la consulta</returns>
        public object ExecuteScalar(string query, params MySqlParameter[] parameters)
        {
            try
            {
                using (var cmd = new MySqlCommand(query, GetConnection()))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
#pragma warning disable CS8603 // Possible null reference return.
                    return cmd.ExecuteScalar();
#pragma warning restore CS8603 // Possible null reference return.
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al ejecutar consulta: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una consulta SQL que retorna un conjunto de resultados
        /// </summary>
        /// <param name="query">Consulta SQL a ejecutar</param>
        /// <param name="parameters">Parámetros para la consulta</param>
        /// <returns>Objeto MySqlDataReader con los resultados</returns>
        public MySqlDataReader ExecuteReader(string query, params MySqlParameter[] parameters)
        {
            try
            {
                var cmd = new MySqlCommand(query, GetConnection());
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteReader();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al ejecutar consulta: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Inicia una transacción en la base de datos
        /// </summary>
        /// <returns>Objeto MySqlTransaction</returns>
        public MySqlTransaction BeginTransaction()
        {
            try
            {
                return GetConnection().BeginTransaction();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al iniciar transacción: {ex.Message}");
                throw;
            }
        }
    }
}