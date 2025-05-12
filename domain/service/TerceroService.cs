using System;
using System.Collections.Generic;
using tallerc.domain.entities;
using tallerc.domain.repositories;

namespace tallerc.domain.services
{
    /// <summary>
    /// Servicio que encapsula la lógica de negocio relacionada con los terceros
    /// </summary>
    public class TerceroService
    {
        private readonly ITerceroRepository _terceroRepository;

        /// <summary>
        /// Constructor que inyecta el repositorio de terceros
        /// </summary>
        /// <param name="terceroRepository">Instancia del repositorio de terceros</param>
        public TerceroService(ITerceroRepository terceroRepository)
        {
            _terceroRepository = terceroRepository ?? throw new ArgumentNullException(nameof(terceroRepository));
        }

        /// <summary>
        /// Obtiene todos los terceros
        /// </summary>
        /// <returns>Lista de terceros</returns>
        public List<Tercero> GetAllTerceros()
        {
            return _terceroRepository.GetAll();
        }

        /// <summary>
        /// Obtiene todos los proveedores
        /// </summary>
        /// <returns>Lista de proveedores</returns>
        public List<Tercero> GetAllProveedores()
        {
            return _terceroRepository.GetProveedores();
        }

        /// <summary>
        /// Obtiene todos los empleados
        /// </summary>
        /// <returns>Lista de empleados</returns>
        public List<Tercero> GetAllEmpleados()
        {
            return _terceroRepository.GetEmpleados();
        }

        /// <summary>
        /// Busca terceros por nombre o apellido
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Lista de terceros que coinciden con la búsqueda</returns>
        public List<Tercero> SearchTerceros(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Tercero>();

            return _terceroRepository.SearchByNombre(searchTerm);
        }
                    public List<Tercero> GetByTipo(int tipoTerceroId)
            {
                return _terceroRepository.GetByTipo(tipoTerceroId);
            }

        /// <summary>
        /// Obtiene un tercero por su ID
        /// </summary>
        /// <param name="id">ID del tercero</param>
        /// <returns>Tercero encontrado o null</returns>
        public Tercero GetTerceroById(int id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return _terceroRepository.GetById(id);
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Crea un nuevo tercero
        /// </summary>
        /// <param name="tercero">Datos del tercero a crear</param>
        /// <returns>ID del tercero creado o -1 si hay error</returns>
        public int CreateTercero(Tercero tercero)
        {
            if (tercero == null)
                throw new ArgumentNullException(nameof(tercero));

            if (!tercero.EsValido())
                return -1;

            // Verificar si ya existe un tercero con el mismo documento
            var existente = _terceroRepository.GetByDocumento(tercero.NumeroDocumento);
            if (existente != null)
                return -1;

            // Establecer fechas
            tercero.CreatedAt = DateTime.Now;
            tercero.UpdatedAt = DateTime.Now;

            return _terceroRepository.Add(tercero);
        }

        /// <summary>
        /// Actualiza un tercero existente
        /// </summary>
        /// <param name="tercero">Datos actualizados del tercero</param>
        /// <returns>True si la actualización fue exitosa</returns>
        public bool UpdateTercero(Tercero tercero)
        {
            if (tercero == null)
                throw new ArgumentNullException(nameof(tercero));

            if (!tercero.EsValido() || tercero.Id <= 0)
                return false;

            // Verificar si existe el tercero
            if (!_terceroRepository.Exists(tercero.Id))
                return false;

            // Actualizar fecha de modificación
            tercero.UpdatedAt = DateTime.Now;

            return _terceroRepository.Update(tercero);
        }

        /// <summary>
        /// Elimina un tercero por su ID
        /// </summary>
        /// <param name="id">ID del tercero a eliminar</param>
        /// <returns>True si la eliminación fue exitosa</returns>
        public bool DeleteTercero(int id)
        {
            if (id <= 0)
                return false;

            return _terceroRepository.Delete(id);
        }

        /// <summary>
        /// Verifica si existe un tercero con el número de documento especificado
        /// </summary>
        /// <param name="numeroDocumento">Número de documento a verificar</param>
        /// <returns>True si existe un tercero con ese documento</returns>
        public bool ExisteDocumento(string numeroDocumento)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
                return false;

            var tercero = _terceroRepository.GetByDocumento(numeroDocumento);
            return tercero != null;
        }
    }
    }