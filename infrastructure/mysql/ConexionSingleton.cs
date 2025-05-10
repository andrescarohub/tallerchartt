using System;
using System.Configuration; // Para App.config
using MySqlConnector;
using System.Data; // Para ConnectionState y CommandBehavior

namespace tallerc.infrastructure.mysql
{
    public sealed class ConexionSingleton
    {
        private static ConexionSingleton? _instance;
        private readonly string _connectionString;
        private static readonly object _lock = new object();

        private ConexionSingleton()
        {
            // Leer desde App.config (RECOMENDADO)
            // _connectionString = ConfigurationManager.ConnectionStrings["MiConexionMySQL"]?.ConnectionString;
            // if (string.IsNullOrEmpty(_connectionString))
            // {
            //    _connectionString = "Server=localhost;Database=tallerdechart;User=root;Password=1234;"; // Fallback
            //    Console.WriteLine("ADVERTENCIA: Usando cadena de conexión por defecto. Verifica tu App.config.");
            // }

            // Por ahora, mantenemos tu cadena hardcodeada para simplificar, pero considera el cambio.
            _connectionString = "Server=localhost;Database=tallerdechart;User=root;Password=1234;";
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("La cadena de conexión no puede ser nula o vacía.");
            }
        }

        public static ConexionSingleton Instance
        {
            get
            {
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
        /// Obtiene una NUEVA conexión abierta a la base de datos.
        /// El llamador es responsable de cerrarla (idealmente usando un bloque 'using').
        /// </summary>
        /// <returns>Un nuevo objeto MySqlConnection abierto.</returns>
        public MySqlConnection GetNuevaConexion()
        {
            try
            {
                var connection = new MySqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al crear y abrir una nueva conexión: {ex.Message}");
                throw; // Relanzar para que el llamador maneje el error.
            }
        }

        // --- Métodos de Ejecución (ahora pueden operar con una conexión y transacción pasadas) ---

        public int ExecuteNonQuery(string query, MySqlTransaction? transaction, params MySqlParameter[] parameters)
        {
            // Este método ahora requiere que la conexión venga de la transacción
            if (transaction == null) throw new ArgumentNullException(nameof(transaction), "La transacción no puede ser nula para esta sobrecarga.");
            if (transaction.Connection == null) throw new InvalidOperationException("La transacción no está asociada a una conexión.");
            
            try
            {
                using (var cmd = new MySqlCommand(query, transaction.Connection, transaction))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al ejecutar NonQuery con transacción: {ex.Message}");
                throw;
            }
        }

        // Sobrecarga para ejecutar sin una transacción explícita (usará su propia conexión)
        public int ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            try
            {
                using (var conn = GetNuevaConexion()) // Obtiene una nueva conexión
                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                } // La conexión se cierra aquí automáticamente por el 'using'
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al ejecutar NonQuery: {ex.Message}");
                throw;
            }
        }


        public object? ExecuteScalar(string query, MySqlTransaction? transaction, params MySqlParameter[] parameters)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction), "La transacción no puede ser nula para esta sobrecarga.");
            if (transaction.Connection == null) throw new InvalidOperationException("La transacción no está asociada a una conexión.");

            try
            {
                using (var cmd = new MySqlCommand(query, transaction.Connection, transaction))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al ejecutar Scalar con transacción: {ex.Message}");
                throw;
            }
        }

        public object? ExecuteScalar(string query, params MySqlParameter[] parameters)
        {
            try
            {
                using (var conn = GetNuevaConexion())
                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error al ejecutar Scalar: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una consulta SQL que retorna un conjunto de resultados.
        /// La conexión se cierra automáticamente cuando el DataReader es dispuesto si se usa CommandBehavior.CloseConnection.
        /// </summary>
        public MySqlDataReader ExecuteReader(string query, params MySqlParameter[] parameters)
        {
            // Este método DEBE devolver un DataReader que cierre su propia conexión.
            MySqlConnection? conn = null;
            try
            {
                conn = GetNuevaConexion(); // Obtiene una nueva conexión
                var cmd = new MySqlCommand(query, conn);
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                // IMPORTANTE: CommandBehavior.CloseConnection cierra la conexión cuando el reader se cierra.
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (MySqlException ex)
            {
                conn?.Close(); // Intenta cerrar la conexión si falló antes de devolver el reader
                conn?.Dispose();
                Console.WriteLine($"Error al ejecutar Reader: {ex.Message}");
                throw;
            }
        }
        
        // El BeginTransaction() original que tenías ya no es necesario aquí si cada
        // repositorio obtiene una nueva conexión para su transacción.
        // El repositorio haría:
        // using (var conn = ConexionSingleton.Instance.GetNuevaConexion())
        // using (var transaction = conn.BeginTransaction())
        // { ... }
    }
}