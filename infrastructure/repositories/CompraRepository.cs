// infrastructure/repositories/CompraRepository.cs
using System;
using System.Collections.Generic;
using System.Data; // Para IDisposable y potencialmente IDbTransaction
using MySqlConnector;
using tallerc.domain.entities;
using tallerc.domain.repositories;

namespace tallerc.infrastructure.repositories
{
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public class CompraRepository : BaseRepository<Compra>, ICompraRepository
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        // Podríamos inyectar IDetalleCompraRepository si queremos delegar la persistencia de detalles
        // private readonly IDetalleCompraRepository _detalleCompraRepository;

        public CompraRepository(/*IDetalleCompraRepository detalleCompraRepository*/) : base("Compras") 
        {
            // _detalleCompraRepository = detalleCompraRepository;
        }

        protected override Compra MapToEntity(MySqlDataReader reader)
        {
            // Este mapeo no carga los detalles por defecto.
            // GetCompraConDetalles se encargará de eso.
            return new Compra
            {
                Id = Convert.ToInt32(reader["Id"]),
                ProveedorId = Convert.ToInt32(reader["ProveedorId"]),
                EmpleadoId = Convert.ToInt32(reader["EmpleadoId"]),
                Fecha = Convert.ToDateTime(reader["Fecha"]),
                NumeroFactura = reader["NumeroFactura"].ToString(),
                Observaciones = reader["Observaciones"] != DBNull.Value ? reader["Observaciones"].ToString() : null,
                Estado = Convert.ToInt32(reader["Estado"])
                // Detalles se inicializa vacío en el constructor de Compra
            };
        }

        // Add, Update, Delete de BaseRepository son abstractos, pero ICompraRepository
        // tiene AddCompraConDetalles que es más específico.
        // Decidiremos si overrideamos Add, Update, Delete o los dejamos sin implementar
        // si toda la lógica se maneja con los métodos específicos de ICompraRepository.
        // Por ahora, implementaremos los básicos también.

        public override int Add(Compra entity)
        {
            // Este Add es para la cabecera de la compra SOLAMENTE.
            // AddCompraConDetalles es para la operación completa.
            // Generalmente, se preferirá AddCompraConDetalles.
            try
            {
                string query = @"
                    INSERT INTO Compras (ProveedorId, EmpleadoId, Fecha, NumeroFactura, Observaciones, Estado)
                    VALUES (@ProveedorId, @EmpleadoId, @Fecha, @NumeroFactura, @Observaciones, @Estado);
                    SELECT LAST_INSERT_ID();";

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@ProveedorId", entity.ProveedorId),
                    new MySqlParameter("@EmpleadoId", entity.EmpleadoId),
                    new MySqlParameter("@Fecha", entity.Fecha),
                    new MySqlParameter("@NumeroFactura", entity.NumeroFactura),
                    new MySqlParameter("@Observaciones", entity.Observaciones ?? (object)DBNull.Value),
                    new MySqlParameter("@Estado", entity.Estado)
                };

                var id = _conexion.ExecuteScalar(query, parameters.ToArray());
                return Convert.ToInt32(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CompraRepository.Add (cabecera): {ex.Message}");
                return -1;
            }
        }
        
        public override bool Update(Compra entity)
        {
            // Actualiza solo la cabecera de la compra. Los detalles se manejarían por separado
            // o mediante una lógica más compleja si se permite modificar una compra con detalles.
             try
            {
                string query = @"
                    UPDATE Compras SET 
                        ProveedorId = @ProveedorId, 
                        EmpleadoId = @EmpleadoId, 
                        Fecha = @Fecha, 
                        NumeroFactura = @NumeroFactura, 
                        Observaciones = @Observaciones, 
                        Estado = @Estado
                    WHERE Id = @Id";

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@Id", entity.Id),
                    new MySqlParameter("@ProveedorId", entity.ProveedorId),
                    new MySqlParameter("@EmpleadoId", entity.EmpleadoId),
                    new MySqlParameter("@Fecha", entity.Fecha),
                    new MySqlParameter("@NumeroFactura", entity.NumeroFactura),
                    new MySqlParameter("@Observaciones", entity.Observaciones ?? (object)DBNull.Value),
                    new MySqlParameter("@Estado", entity.Estado)
                };
                int rowsAffected = _conexion.ExecuteNonQuery(query, parameters.ToArray());
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CompraRepository.Update (cabecera): {ex.Message}");
                return false;
            }
        }

        public override bool Delete(int id)
        {
            // ¡CUIDADO! Eliminar una compra debería también eliminar sus detalles
            // y considerar el impacto en el stock si la compra fue completada.
            // Esto debería manejarse con una transacción.
            // El CompraService es un mejor lugar para orquestar esto si la lógica es compleja.
            // O hacer una eliminación en cascada en la BD.
            // Por simplicidad aquí, solo borramos la cabecera.
            MySqlTransaction transaction = null;
            try
            {
                using (var connection = _conexion.GetNuevaConnection()) // Asegura que la conexión esté abierta
                {
                    transaction = connection.BeginTransaction();

                    // 1. Eliminar detalles
                    string deleteDetallesQuery = "DELETE FROM DetalleCompras WHERE CompraId = @CompraId";
                    var detalleParam = new MySqlParameter("@CompraId", id);
                    _conexion.ExecuteNonQuery(deleteDetallesQuery, transaction, detalleParam);
                    // No necesitas usar `connection` directamente aquí si tu _conexion.ExecuteNonQuery
                    // ya puede tomar una transacción y usa la conexión del Singleton.
                    // Si _conexion.ExecuteNonQuery no toma una transacción, necesitarás
                    // crear MySqlCommand, asignarle la conexión y la transacción, y luego ejecutarlo.


                    // 2. Eliminar cabecera
                    string deleteCompraQuery = "DELETE FROM Compras WHERE Id = @Id";
                    var compraParam = new MySqlParameter("@Id", id);
                     int rowsAffected = _conexion.ExecuteNonQuery(deleteCompraQuery, transaction, compraParam);

                    transaction.Commit();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Error en CompraRepository.Delete: {ex.Message}");
                return false;
            }
        }


        // --- Custom Methods from ICompraRepository ---

        public List<Compra> GetByProveedor(int proveedorId)
        {
            var compras = new List<Compra>();
            string query = "SELECT * FROM Compras WHERE ProveedorId = @ProveedorId";
            var parameter = new MySqlParameter("@ProveedorId", proveedorId);
            using (var reader = _conexion.ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    // Este Get no carga los detalles
                    compras.Add(MapToEntity(reader));
                }
            }
            return compras;
        }

        public List<Compra> GetByFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            var compras = new List<Compra>();
            // Asegúrate de que la parte de la hora se maneje correctamente para la fechaFin (ej. hasta el final del día)
            string query = "SELECT * FROM Compras WHERE Fecha >= @FechaInicio AND Fecha <= @FechaFin";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@FechaInicio", fechaInicio),
                new MySqlParameter("@FechaFin", fechaFin)
            };
            using (var reader = _conexion.ExecuteReader(query, parameters))
            {
                while (reader.Read())
                {
                    compras.Add(MapToEntity(reader));
                }
            }
            return compras;
        }

        public List<Compra> GetByEstado(int estado)
        {
            var compras = new List<Compra>();
            string query = "SELECT * FROM Compras WHERE Estado = @Estado";
            var parameter = new MySqlParameter("@Estado", estado);
            using (var reader = _conexion.ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    compras.Add(MapToEntity(reader));
                }
            }
            return compras;
        }

        public int AddCompraConDetalles(Compra compra)
        {
            MySqlTransaction transaction = null;
            try
            {
                // Obtener la conexión y empezar la transacción
                // Necesitas asegurar que _conexion.GetConnection() te da una conexión abierta
                // o abrirla tú mismo.
                using (var connection = _conexion.GetNuevaConnection()) // Asumiendo que GetConnection abre si es necesario
                {
                    transaction = connection.BeginTransaction();

                    // 1. Insertar la cabecera de la Compra
                    string queryCompra = @"
                        INSERT INTO Compras (ProveedorId, EmpleadoId, Fecha, NumeroFactura, Observaciones, Estado)
                        VALUES (@ProveedorId, @EmpleadoId, @Fecha, @NumeroFactura, @Observaciones, @Estado);
                        SELECT LAST_INSERT_ID();";
                    var compraParams = new List<MySqlParameter>
                    {
                        new MySqlParameter("@ProveedorId", compra.ProveedorId),
                        new MySqlParameter("@EmpleadoId", compra.EmpleadoId),
                        new MySqlParameter("@Fecha", compra.Fecha),
                        new MySqlParameter("@NumeroFactura", compra.NumeroFactura),
                        new MySqlParameter("@Observaciones", compra.Observaciones ?? (object)DBNull.Value),
                        new MySqlParameter("@Estado", compra.Estado)
                    };
                    
                    // Necesitas un método en ConexionSingleton o aquí que ejecute ExecuteScalar con una transacción
                    // Si _conexion.ExecuteScalar no acepta una transacción, necesitas crear el MySqlCommand manualmente:
                    int compraId;
                    using(MySqlCommand cmdCompra = new MySqlCommand(queryCompra, connection, transaction))
                    {
                        cmdCompra.Parameters.AddRange(compraParams.ToArray());
                        compraId = Convert.ToInt32(cmdCompra.ExecuteScalar());
                    }
                    
                    if (compraId <= 0) throw new Exception("No se pudo crear la cabecera de la compra.");
                    compra.Id = compraId;


                    // 2. Insertar los Detalles de la Compra
                    if (compra.Detalles != null && compra.Detalles.Any())
                    {
                        string queryDetalle = @"
                            INSERT INTO DetalleCompras (CompraId, ProductoId, Cantidad, Valor) 
                            VALUES (@CompraId, @ProductoId, @Cantidad, @Valor);";
                        
                        foreach (var detalle in compra.Detalles)
                        {
                            detalle.CompraId = compraId; // Asegurar que el detalle tiene el ID de la compra
                            var detalleParams = new List<MySqlParameter>
                            {
                                new MySqlParameter("@CompraId", detalle.CompraId),
                                new MySqlParameter("@ProductoId", detalle.ProductoId),
                                new MySqlParameter("@Cantidad", detalle.Cantidad),
                                new MySqlParameter("@Valor", detalle.Valor)
                            };
                            // Mismo caso para ExecuteNonQuery con transacción
                            using(MySqlCommand cmdDetalle = new MySqlCommand(queryDetalle, connection, transaction))
                            {
                                cmdDetalle.Parameters.AddRange(detalleParams.ToArray());
                                cmdDetalle.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                    return compraId;
                }
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Error en CompraRepository.AddCompraConDetalles: {ex.Message}");
                return -1;
            }
        }

        public bool UpdateEstado(int compraId, int estado)
        {
            try
            {
                string query = "UPDATE Compras SET Estado = @Estado WHERE Id = @Id";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Estado", estado),
                    new MySqlParameter("@Id", compraId)
                };
                int rowsAffected = _conexion.ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CompraRepository.UpdateEstado: {ex.Message}");
                return false;
            }
        }

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        public Compra? GetCompraConDetalles(int compraId)
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            Compra compra = null;
            string queryCompra = "SELECT * FROM Compras WHERE Id = @Id";
            var parameterCompra = new MySqlParameter("@Id", compraId);

            using (var reader = _conexion.ExecuteReader(queryCompra, parameterCompra))
            {
                if (reader.Read())
                {
                    compra = MapToEntity(reader);
                }
            }

           // ... dentro de public Compra? GetCompraConDetalles(int compraId) ...
            if (compra != null)
            {
                compra.Detalles = new List<DetalleCompra>();
                string queryDetalles = "SELECT * FROM DetalleCompras WHERE CompraId = @CompraId"; // Asumiendo que tu tabla se llama DetalleCompras
                var parameterDetalles = new MySqlParameter("@CompraId", compraId);
                
                using (var readerDetalles = _conexion.ExecuteReader(queryDetalles, parameterDetalles))
                {
                    while (readerDetalles.Read())
                    {
                        // Mapear directamente aquí
                        DetalleCompra detalle = new DetalleCompra
                        {
                            Id = Convert.ToInt32(readerDetalles["Id"]), // Ajusta los nombres de columna a tu tabla Detalle_Compra
                            CompraId = Convert.ToInt32(readerDetalles["CompraId"]),
                            ProductoId = Convert.ToInt32(readerDetalles["id_producto"]), // De tu esquema
                            Cantidad = Convert.ToInt32(readerDetalles["cantidad"]),     // De tu esquema
                            Valor = Convert.ToDecimal(readerDetalles["precio_unitario"]) // De tu esquema (este es el precio de compra unitario)
                            // La propiedad Producto (el objeto completo) no se carga aquí, solo el ID.
                        };
                        compra.Detalles.Add(detalle);
                    }
                }
            }
            return compra;
        }
    }
}