// infrastructure/mysql/ConexionSingleton.cs
using System;
// using System.Configuration; // Puedes comentar esta línea si no tienes App.config y no quieres la advertencia al leerlo
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
            string? appConfigConnectionString = null;
            // La siguiente sección intenta leer de App.config. Si no existe o no se usa, está bien.
            // try
            // {
            //     appConfigConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MiConexionMySQL"]?.ConnectionString;
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine($"ADVERTENCIA al leer App.config (puede ser normal si no se usa o si System.Configuration no está referenciado): {ex.Message}");
            // }

            if (!string.IsNullOrEmpty(appConfigConnectionString))
            {
                _connectionString = appConfigConnectionString;
                Console.WriteLine($"INFO: Usando cadena de conexión de App.config: '{_connectionString}'");
            }
            else
            {
                // Cadena de conexión por defecto si App.config no se usa o no tiene la cadena
                _connectionString = "Server=localhost;Database=tallerdechart;User=root;Password=1234;";
                Console.WriteLine($"INFO: Usando cadena de conexión por defecto: '{_connectionString}'");
            }

            if (string.IsNullOrEmpty(_connectionString))
            {
                // Esta excepción no debería ocurrir si siempre tenemos la cadena por defecto como fallback.
                throw new InvalidOperationException("La cadena de conexión no puede ser nula o vacía después de la inicialización.");
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

        public MySqlConnection GetNuevaConexion()
        {
            Console.WriteLine($"DEBUG CONEXION: Intentando conectar con: '{_connectionString}'");
            try
            {
                var connection = new MySqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"ERROR MYSQL en GetNuevaConexion (cadena usada: '{_connectionString}'): {ex.Message}");
                throw;
            }
            catch (Exception ex) // Captura general por si acaso
            {
                Console.WriteLine($"ERROR GENERAL en GetNuevaConexion (cadena usada: '{_connectionString}'): {ex.Message}");
                throw;
            }
        }

        public int ExecuteNonQuery(string query, MySqlTransaction transaction, params MySqlParameter[] parameters)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction), "La transacción no puede ser nula para esta sobrecarga de ExecuteNonQuery.");
            if (transaction.Connection == null) throw new InvalidOperationException("La transacción no está asociada a una conexión válida.");
            
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
                Console.WriteLine($"ERROR MYSQL en ExecuteNonQuery con transacción (Query: {query.Substring(0, Math.Min(query.Length, 100))}...): {ex.Message}");
                throw;
            }
        }

        public int ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            try
            {
                using (var conn = GetNuevaConexion())
                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"ERROR MYSQL en ExecuteNonQuery (Query: {query.Substring(0, Math.Min(query.Length, 100))}...): {ex.Message}");
                throw;
            }
        }

        public object? ExecuteScalar(string query, MySqlTransaction transaction, params MySqlParameter[] parameters)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction), "La transacción no puede ser nula para esta sobrecarga de ExecuteScalar.");
            if (transaction.Connection == null) throw new InvalidOperationException("La transacción no está asociada a una conexión válida.");

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
                Console.WriteLine($"ERROR MYSQL en ExecuteScalar con transacción (Query: {query.Substring(0, Math.Min(query.Length, 100))}...): {ex.Message}");
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
                Console.WriteLine($"ERROR MYSQL en ExecuteScalar (Query: {query.Substring(0, Math.Min(query.Length, 100))}...): {ex.Message}");
                throw;
            }
        }

        public MySqlDataReader ExecuteReader(string query, params MySqlParameter[] parameters)
        {
            MySqlConnection? conn = null;
            try
            {
                conn = GetNuevaConexion();
                var cmd = new MySqlCommand(query, conn);
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (MySqlException ex)
            {
                conn?.Close();
                conn?.Dispose();
                Console.WriteLine($"ERROR MYSQL en ExecuteReader (Query: {query.Substring(0, Math.Min(query.Length, 100))}...): {ex.Message}");
                throw;
            }
        }
    }
}