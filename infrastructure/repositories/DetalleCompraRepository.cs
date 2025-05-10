// infrastructure/repositories/DetalleCompraRepository.cs
using System;
using System.Collections.Generic;
using MySqlConnector;
using tallerc.domain.entities;
using tallerc.domain.repositories;

namespace tallerc.infrastructure.repositories
{
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public class DetalleCompraRepository : BaseRepository<DetalleCompra>, IDetalleCompraRepository
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        // Para DetalleCompra, el nombre de la tabla es crucial
        public DetalleCompraRepository() : base("DetalleCompras") { } // O como se llame tu tabla

        protected override DetalleCompra MapToEntity(MySqlDataReader reader)
        {
            // Asumiendo que tienes una propiedad Producto en DetalleCompra
            // y que quieres cargarla aquí (requeriría un JOIN en la consulta o una carga separada)
            // Por simplicidad, aquí solo mapearemos los IDs. El servicio o una consulta más compleja
            // se encargaría de poblar el objeto Producto completo.
            return new DetalleCompra
            {
                Id = Convert.ToInt32(reader["Id"]),
                CompraId = Convert.ToInt32(reader["CompraId"]),
                ProductoId = Convert.ToInt32(reader["ProductoId"]),
                Cantidad = Convert.ToInt32(reader["Cantidad"]),
                Valor = Convert.ToDecimal(reader["Valor"]) // o como se llame tu columna de precio
                // Producto = null // Se llenaría después si es necesario
            };
        }

        public override int Add(DetalleCompra entity)
        {
            // Usualmente los detalles se añaden en lote o como parte de AddCompraConDetalles.
            // Este método sería para añadir un detalle individual si fuera necesario.
            try
            {
                string query = @"
                    INSERT INTO DetalleCompras (CompraId, ProductoId, Cantidad, Valor) 
                    VALUES (@CompraId, @ProductoId, @Cantidad, @Valor);
                    SELECT LAST_INSERT_ID();";

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@CompraId", entity.CompraId),
                    new MySqlParameter("@ProductoId", entity.ProductoId),
                    new MySqlParameter("@Cantidad", entity.Cantidad),
                    new MySqlParameter("@Valor", entity.Valor)
                };
                var id = _conexion.ExecuteScalar(query, parameters.ToArray());
                return Convert.ToInt32(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DetalleCompraRepository.Add: {ex.Message}");
                return -1;
            }
        }

        public override bool Update(DetalleCompra entity)
        {
            // Actualizar un detalle de compra es menos común, usualmente se borra y se añade
            // o se maneja a nivel de la compra.
            try
            {
                string query = @"
                    UPDATE DetalleCompras SET 
                        ProductoId = @ProductoId, 
                        Cantidad = @Cantidad, 
                        Valor = @Valor 
                    WHERE Id = @Id AND CompraId = @CompraId"; // Importante filtrar por CompraId también

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@Id", entity.Id),
                    new MySqlParameter("@CompraId", entity.CompraId),
                    new MySqlParameter("@ProductoId", entity.ProductoId),
                    new MySqlParameter("@Cantidad", entity.Cantidad),
                    new MySqlParameter("@Valor", entity.Valor)
                };
                int rowsAffected = _conexion.ExecuteNonQuery(query, parameters.ToArray());
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DetalleCompraRepository.Update: {ex.Message}");
                return false;
            }
        }

        public override bool Delete(int id)
        {
            // Eliminar detalles individuales. Podrías necesitar DeleteByCompraId.
            try
            {
                string query = "DELETE FROM DetalleCompras WHERE Id = @Id";
                var parameter = new MySqlParameter("@Id", id);
                int rowsAffected = _conexion.ExecuteNonQuery(query, parameter);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DetalleCompraRepository.Delete: {ex.Message}");
                return false;
            }
        }

        public List<DetalleCompra> GetByCompraId(int compraId)
        {
            var detalles = new List<DetalleCompra>();
            string query = "SELECT * FROM DetalleCompras WHERE CompraId = @CompraId";
            var parameter = new MySqlParameter("@CompraId", compraId);

            using (var reader = _conexion.ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    detalles.Add(MapToEntity(reader));
                }
            }
            return detalles;
        }
    }
}