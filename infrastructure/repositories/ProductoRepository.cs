// infrastructure/repositories/ProductoRepository.cs
using System;
using System.Collections.Generic;
using MySqlConnector;
using tallerc.domain.entities;
using tallerc.domain.repositories;

namespace tallerc.infrastructure.repositories
{
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public class ProductoRepository : BaseRepository<Producto>, IProductoRepository
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        public ProductoRepository() : base("Productos") { }

        protected override Producto MapToEntity(MySqlDataReader reader)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8604 // Possible null reference argument.
            return new Producto
            {
                Id = Convert.ToInt32(reader["Id"]),
                Nombre = reader["Nombre"].ToString(),
                StockActual = Convert.ToInt32(reader["StockActual"]),
                StockMinimo = Convert.ToInt32(reader["StockMinimo"]),
                StockMaximo = Convert.ToInt32(reader["StockMaximo"]),
                Barcode = reader["Barcode"] != DBNull.Value ? reader["Barcode"].ToString() : null,
                PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                CategoriaId = reader["CategoriaId"] != DBNull.Value ? Convert.ToInt32(reader["CategoriaId"]) : (int?)null,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                // Añade aquí otras propiedades si tu entidad Producto las tiene y están en la tabla
            };
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        public override int Add(Producto entity)
        {
            try
            {
                string query = @"
                    INSERT INTO Productos 
                        (Nombre, StockActual, StockMinimo, StockMaximo, Barcode, PrecioUnitario, CategoriaId, CreatedAt, UpdatedAt)
                    VALUES 
                        (@Nombre, @StockActual, @StockMinimo, @StockMaximo, @Barcode, @PrecioUnitario, @CategoriaId, @CreatedAt, @UpdatedAt);
                    SELECT LAST_INSERT_ID();";

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@Nombre", entity.Nombre),
                    new MySqlParameter("@StockActual", entity.StockActual),
                    new MySqlParameter("@StockMinimo", entity.StockMinimo),
                    new MySqlParameter("@StockMaximo", entity.StockMaximo),
                    new MySqlParameter("@Barcode", entity.Barcode ?? (object)DBNull.Value),
                    new MySqlParameter("@PrecioUnitario", entity.PrecioUnitario),
                    new MySqlParameter("@CategoriaId", entity.CategoriaId.HasValue ? (object)entity.CategoriaId.Value : DBNull.Value),
                    new MySqlParameter("@CreatedAt", DateTime.Now),
                    new MySqlParameter("@UpdatedAt", DateTime.Now)
                };

                var id = _conexion.ExecuteScalar(query, parameters.ToArray());
                return Convert.ToInt32(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ProductoRepository.Add: {ex.Message}");
                // Considera lanzar una excepción personalizada o loguear de forma más robusta
                return -1;
            }
        }

        public override bool Update(Producto entity)
        {
            try
            {
                string query = @"
                    UPDATE Productos SET
                        Nombre = @Nombre,
                        StockActual = @StockActual,
                        StockMinimo = @StockMinimo,
                        StockMaximo = @StockMaximo,
                        Barcode = @Barcode,
                        PrecioUnitario = @PrecioUnitario,
                        CategoriaId = @CategoriaId,
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@Id", entity.Id),
                    new MySqlParameter("@Nombre", entity.Nombre),
                    new MySqlParameter("@StockActual", entity.StockActual),
                    new MySqlParameter("@StockMinimo", entity.StockMinimo),
                    new MySqlParameter("@StockMaximo", entity.StockMaximo),
                    new MySqlParameter("@Barcode", entity.Barcode ?? (object)DBNull.Value),
                    new MySqlParameter("@PrecioUnitario", entity.PrecioUnitario),
                    new MySqlParameter("@CategoriaId", entity.CategoriaId.HasValue ? (object)entity.CategoriaId.Value : DBNull.Value),
                    new MySqlParameter("@UpdatedAt", DateTime.Now)
                };

                int rowsAffected = _conexion.ExecuteNonQuery(query, parameters.ToArray());
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ProductoRepository.Update: {ex.Message}");
                return false;
            }
        }

        public override bool Delete(int id)
        {
            // Considera la eliminación lógica (marcar como inactivo) en lugar de física
            // si hay dependencias (ej. productos en ventas históricas).
            try
            {
                string query = "DELETE FROM Productos WHERE Id = @Id";
                var parameter = new MySqlParameter("@Id", id);
                
                int rowsAffected = _conexion.ExecuteNonQuery(query, parameter);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ProductoRepository.Delete: {ex.Message}");
                return false;
            }
        }

        // --- Custom Methods from IProductoRepository ---
        public List<Producto> GetWithLowStock()
        {
            var productos = new List<Producto>();
            // La entidad Producto tiene RequiereReposicion() que compara StockActual con StockMinimo
            // Podríamos hacer el filtro en la BD para eficiencia
            string query = "SELECT * FROM Productos WHERE StockActual < StockMinimo";
            
            using (var reader = _conexion.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    productos.Add(MapToEntity(reader));
                }
            }
            return productos;
        }

        public bool UpdateStock(int productoId, int cantidad)
        {
            // Esta operación debe ser atómica y manejar concurrencia si es necesario.
            // El '+' y '-' se hacen en la consulta para asegurar atomicidad a nivel de DB.
            try
            {
                string query = "UPDATE Productos SET StockActual = StockActual + @Cantidad, UpdatedAt = @UpdatedAt WHERE Id = @ProductoId";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Cantidad", cantidad),
                    new MySqlParameter("@UpdatedAt", DateTime.Now),
                    new MySqlParameter("@ProductoId", productoId)
                };
                
                int rowsAffected = _conexion.ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ProductoRepository.UpdateStock: {ex.Message}");
                return false;
            }
        }

        public List<Producto> GetByPlan(int planId)
        {
            // Esto requerirá una tabla de enlace PlanPromocionalProducto
            // y un JOIN. Asumamos que la tabla se llama PlanPromocionalProductos
            // y tiene columnas PlanPromocionalId y ProductoId.
            var productos = new List<Producto>();
            string query = @"
                SELECT p.* 
                FROM Productos p
                INNER JOIN PlanPromocionalProductos ppp ON p.Id = ppp.ProductoId
                WHERE ppp.PlanPromocionalId = @PlanId";
            var parameter = new MySqlParameter("@PlanId", planId);

            using (var reader = _conexion.ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    productos.Add(MapToEntity(reader));
                }
            }
            return productos;
        }

        public List<Producto> Search(string texto)
        {
            var productos = new List<Producto>();
            string query = "SELECT * FROM Productos WHERE Nombre LIKE @Texto OR Barcode LIKE @Texto";
            var parameter = new MySqlParameter("@Texto", $"%{texto}%");

            using (var reader = _conexion.ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    productos.Add(MapToEntity(reader));
                }
            }
            return productos;
        }

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        public Producto? GetByBarcode(string barcode)
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            string query = "SELECT * FROM Productos WHERE Barcode = @Barcode";
            var parameter = new MySqlParameter("@Barcode", barcode);

            using (var reader = _conexion.ExecuteReader(query, parameter))
            {
                if (reader.Read())
                {
                    return MapToEntity(reader);
                }
                return null;
            }
        }
    }
}