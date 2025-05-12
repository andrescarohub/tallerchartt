// infrastructure/repositories/CompraRepository.cs
using System;
using System.Collections.Generic;
using System.Linq; // Para .Any()
using MySqlConnector;
using tallerc.domain.entities;
using tallerc.domain.repositories;
using tallerc.infrastructure.mysql; // Asegúrate que ConexionSingleton está aquí

namespace tallerc.infrastructure.repositories
{
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public class CompraRepository : BaseRepository<Compra>, ICompraRepository
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        public CompraRepository() : base("Compra") // <-- CORREGIDO: Nombre de tabla singular
        {
        }

        protected override Compra MapToEntity(MySqlDataReader reader)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return new Compra
            {
                Id = Convert.ToInt32(reader["Id"]),
                ProveedorId = Convert.ToInt32(reader["ProveedorId"]),
                EmpleadoId = Convert.ToInt32(reader["EmpleadoId"]),
                Fecha = Convert.ToDateTime(reader["Fecha"]),
                NumeroFactura = reader["NumeroFactura"].ToString(),
                Observaciones = reader["Observaciones"] != DBNull.Value ? reader["Observaciones"].ToString() : null,
                Estado = Convert.ToInt32(reader["Estado"])
            };
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        public override int Add(Compra entity)
        {
            try
            {
                // Usar tabla "Compra" (singular)
                string query = @"
                    INSERT INTO Compra (ProveedorId, EmpleadoId, Fecha, NumeroFactura, Observaciones, Estado)
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
             try
            {
                // Usar tabla "Compra" (singular)
                string query = @"
                    UPDATE Compra SET 
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
            MySqlConnection? conn = null; // <-- CORREGIDO: Declarar fuera del using para que esté disponible en finally
            MySqlTransaction? transaction = null;
            try
            {
                conn = _conexion.GetNuevaConexion(); // Obtenemos la conexión primero
                transaction = conn.BeginTransaction();

                // 1. Eliminar detalles
                // Usar tabla "DetalleCompra" (singular)
                string deleteDetallesQuery = "DELETE FROM DetalleCompra WHERE CompraId = @CompraId";
                var detalleParam = new MySqlParameter("@CompraId", id);
                _conexion.ExecuteNonQuery(deleteDetallesQuery, transaction, detalleParam); // <-- CORREGIDO: Usar sobrecarga con transacción

                // 2. Eliminar cabecera
                // Usar tabla "Compra" (singular)
                string deleteCompraQuery = "DELETE FROM Compra WHERE Id = @Id";
                var compraParam = new MySqlParameter("@Id", id);
                int rowsAffected = _conexion.ExecuteNonQuery(deleteCompraQuery, transaction, compraParam); // <-- CORREGIDO: Usar sobrecarga con transacción

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Error en CompraRepository.Delete: {ex.Message}");
                return false;
            }
            finally // <-- CORREGIDO: Asegurar que la conexión se cierra
            {
                conn?.Close();
                conn?.Dispose();
            }
        }

        public List<Compra> GetByProveedor(int proveedorId)
        {
            var compras = new List<Compra>();
            // Usar tabla "Compra" (singular)
            string query = "SELECT * FROM Compra WHERE ProveedorId = @ProveedorId";
            var parameter = new MySqlParameter("@ProveedorId", proveedorId);
            using (var reader = _conexion.ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    compras.Add(MapToEntity(reader));
                }
            }
            return compras;
        }

        public List<Compra> GetByFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            var compras = new List<Compra>();
            // Usar tabla "Compra" (singular)
            string query = "SELECT * FROM Compra WHERE Fecha >= @FechaInicio AND Fecha <= @FechaFin";
             var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@FechaInicio", fechaInicio.Date), // Considerar solo la fecha
                new MySqlParameter("@FechaFin", fechaFin.Date.AddDays(1).AddTicks(-1)) // Hasta el final del día
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
            // Usar tabla "Compra" (singular)
            string query = "SELECT * FROM Compra WHERE Estado = @Estado";
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
            MySqlConnection? conn = null; // <-- CORREGIDO: Declarar fuera del using para que esté disponible en finally
            MySqlTransaction? transaction = null;
            int compraId = -1; // <-- CORREGIDO: Inicializar compraId

            try
            {
                conn = _conexion.GetNuevaConexion();
                transaction = conn.BeginTransaction();

                // 1. Insertar la cabecera de la Compra
                // Usar tabla "Compra" (singular)
                string queryCompra = @"
                    INSERT INTO Compra (ProveedorId, EmpleadoId, Fecha, NumeroFactura, Observaciones, Estado)
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
                
                // Usar sobrecarga de ExecuteScalar con transacción
                var resultId = _conexion.ExecuteScalar(queryCompra, transaction, compraParams.ToArray());
                compraId = Convert.ToInt32(resultId);
                    
                if (compraId <= 0) throw new Exception("No se pudo crear la cabecera de la compra.");
                compra.Id = compraId;

                // 2. Insertar los Detalles de la Compra
                if (compra.Detalles != null && compra.Detalles.Any())
                {
                    // Usar tabla "DetalleCompra" (singular)
                    string queryDetalle = @"
                        INSERT INTO DetalleCompra (CompraId, ProductoId, Cantidad, Valor) 
                        VALUES (@CompraId, @ProductoId, @Cantidad, @Valor);";
                        
                    foreach (var detalle in compra.Detalles)
                    {
                        detalle.CompraId = compraId;
                        var detalleParams = new List<MySqlParameter>
                        {
                            new MySqlParameter("@CompraId", detalle.CompraId),
                            new MySqlParameter("@ProductoId", detalle.ProductoId),
                            new MySqlParameter("@Cantidad", detalle.Cantidad),
                            new MySqlParameter("@Valor", detalle.Valor)
                        };
                        // Usar sobrecarga de ExecuteNonQuery con transacción
                        _conexion.ExecuteNonQuery(queryDetalle, transaction, detalleParams.ToArray());
                    }
                }

                transaction.Commit();
                return compraId;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Error en CompraRepository.AddCompraConDetalles: {ex.Message}");
                return -1; // Asegurar que se retorna -1 en caso de error
            }
            finally // <-- CORREGIDO: Asegurar que la conexión se cierra
            {
                conn?.Close();
                conn?.Dispose();
            }
        }

        public bool UpdateEstado(int compraId, int estado)
        {
            try
            {
                // Usar tabla "Compra" (singular)
                string query = "UPDATE Compra SET Estado = @Estado WHERE Id = @Id";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Estado", estado),
                    new MySqlParameter("@Id", compraId)
                };
                int rowsAffected = _conexion.ExecuteNonQuery(query, parameters.ToArray());
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
            Compra? compra = null;
            // Usar tabla "Compra" (singular)
            string queryCompra = "SELECT * FROM Compra WHERE Id = @Id";
            var parameterCompra = new MySqlParameter("@Id", compraId);

            // Usamos ExecuteReader que maneja su propia conexión y la cierra
            using (var reader = _conexion.ExecuteReader(queryCompra, parameterCompra))
            {
                if (reader.Read())
                {
                    compra = MapToEntity(reader);
                }
            }

            if (compra != null)
            {
                compra.Detalles = new List<DetalleCompra>();
                // Usar tabla "DetalleCompra" (singular)
                string queryDetalles = "SELECT * FROM DetalleCompra WHERE CompraId = @CompraId";
                var parameterDetalles = new MySqlParameter("@CompraId", compraId);
                
                // Usamos ExecuteReader que maneja su propia conexión y la cierra
                using (var readerDetalles = _conexion.ExecuteReader(queryDetalles, parameterDetalles))
                {
                    while (readerDetalles.Read())
                    {
                        DetalleCompra detalle = new DetalleCompra
                        {
                            Id = Convert.ToInt32(readerDetalles["Id"]),
                            CompraId = Convert.ToInt32(readerDetalles["CompraId"]),
                            // Usar los nombres de columna de la tabla DetalleCompra que definimos
                            ProductoId = Convert.ToInt32(readerDetalles["ProductoId"]), 
                            Cantidad = Convert.ToInt32(readerDetalles["Cantidad"]),     
                            Valor = Convert.ToDecimal(readerDetalles["Valor"]) 
                        };
                        compra.Detalles.Add(detalle);
                    }
                }
            }
            return compra;
        }
    }
}