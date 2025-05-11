using System;
using System.Collections.Generic;
using MySqlConnector;
using tallerc.domain.entities;
using tallerc.domain.repositories;

namespace tallerc.infrastructure.repositories
{
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public class TerceroRepository : BaseRepository<Tercero>, ITerceroRepository
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        public TerceroRepository() : base("Terceros") { }

        // Mapea un registro de la DB a un objeto Tercero
        protected override Tercero MapToEntity(MySqlDataReader reader)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return new Tercero
            {
                Id = Convert.ToInt32(reader["Id"]),
                Nombre = reader["Nombre"].ToString(),
                Apellido = reader["Apellido"] != DBNull.Value ? reader["Apellido"].ToString() : null,
                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                NumeroDocumento = reader["NumeroDocumento"].ToString(),
                TipoDocumentoId = Convert.ToInt32(reader["TipoDocumentoId"]),
                TipoTerceroId = Convert.ToInt32(reader["TipoTerceroId"]),
                CiudadId = Convert.ToInt32(reader["CiudadId"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
            };
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        // --- CRUD Methods ---
        public override int Add(Tercero entity)
        {
            try
            {
                string query = @"
                    INSERT INTO Terceros 
                        (Nombre, Apellido, Email, NumeroDocumento, TipoDocumentoId, TipoTerceroId, CiudadId, CreatedAt, UpdatedAt)
                    VALUES 
                        (@Nombre, @Apellido, @Email, @NumeroDocumento, @TipoDocumentoId, @TipoTerceroId, @CiudadId, @CreatedAt, @UpdatedAt);
                    SELECT LAST_INSERT_ID();";

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@Nombre", entity.Nombre),
                    new MySqlParameter("@Apellido", entity.Apellido ?? (object)DBNull.Value),
                    new MySqlParameter("@Email", entity.Email ?? (object)DBNull.Value),
                    new MySqlParameter("@NumeroDocumento", entity.NumeroDocumento),
                    new MySqlParameter("@TipoDocumentoId", entity.TipoDocumentoId),
                    new MySqlParameter("@TipoTerceroId", entity.TipoTerceroId),
                    new MySqlParameter("@CiudadId", entity.CiudadId),
                    new MySqlParameter("@CreatedAt", DateTime.Now),
                    new MySqlParameter("@UpdatedAt", DateTime.Now)
                };

                var id = _conexion.ExecuteScalar(query, parameters.ToArray());
                return Convert.ToInt32(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Add: {ex.Message}");
                return -1;
            }
        }

        public override bool Update(Tercero entity)
        {
            try
            {
                string query = @"
                    UPDATE Terceros SET
                        Nombre = @Nombre,
                        Apellido = @Apellido,
                        Email = @Email,
                        NumeroDocumento = @NumeroDocumento,
                        TipoDocumentoId = @TipoDocumentoId,
                        TipoTerceroId = @TipoTerceroId,
                        CiudadId = @CiudadId,
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@Id", entity.Id),
                    new MySqlParameter("@Nombre", entity.Nombre),
                    new MySqlParameter("@Apellido", entity.Apellido ?? (object)DBNull.Value),
                    new MySqlParameter("@Email", entity.Email ?? (object)DBNull.Value),
                    new MySqlParameter("@NumeroDocumento", entity.NumeroDocumento),
                    new MySqlParameter("@TipoDocumentoId", entity.TipoDocumentoId),
                    new MySqlParameter("@TipoTerceroId", entity.TipoTerceroId),
                    new MySqlParameter("@CiudadId", entity.CiudadId),
                    new MySqlParameter("@UpdatedAt", DateTime.Now)
                };

                int rowsAffected = _conexion.ExecuteNonQuery(query, parameters.ToArray());
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Update: {ex.Message}");
                return false;
            }
        }

        public override bool Delete(int id)
        {
            try
            {
                string query = "DELETE FROM Terceros WHERE Id = @Id";
                var parameter = new MySqlParameter("@Id", id);
                
                int rowsAffected = _conexion.ExecuteNonQuery(query, parameter);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Delete: {ex.Message}");
                return false;
            }
        }

        // --- Custom Methods ---
        public List<Tercero> GetByTipo(int tipoTerceroId)
        {
            var terceros = new List<Tercero>();
            string query = "SELECT * FROM Terceros WHERE TipoTerceroId = @TipoTerceroId";
            var parameter = new MySqlParameter("@TipoTerceroId", tipoTerceroId);

            using (var reader = _conexion.ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    terceros.Add(MapToEntity(reader));
                }
            }
            return terceros;
        }

        public List<Tercero> SearchByNombre(string nombre)
        {
            var terceros = new List<Tercero>();
            string query = "SELECT * FROM Terceros WHERE Nombre LIKE @Nombre OR Apellido LIKE @Nombre";
            var parameter = new MySqlParameter("@Nombre", $"%{nombre}%");

            using (var reader = _conexion.ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    terceros.Add(MapToEntity(reader));
                }
            }
            return terceros;
        }

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        public Tercero? GetByDocumento(string numeroDocumento)
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            string query = "SELECT * FROM Terceros WHERE NumeroDocumento = @NumeroDocumento";
            var parameter = new MySqlParameter("@NumeroDocumento", numeroDocumento);

            using (var reader = _conexion.ExecuteReader(query, parameter))
            {
                if (reader.Read())
                {
                    return MapToEntity(reader);
                }
                return null;
            }
        }

        public List<Tercero> GetProveedores() => GetByTipo(2); // 2 = Proveedor
        public List<Tercero> GetEmpleados() => GetByTipo(3);   // 3 = Empleado
    }
}
