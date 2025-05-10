
using System;

namespace tallerc.domain.entities
{
    /// <summary>
    /// Representa a las personas o empresas con las que se interactúa en el sistema.
    /// Puede ser un cliente, proveedor o empleado dependiendo del TipoTerceroId.
    /// </summary>
    public class Tercero
    {
        /// <summary>
        /// Identificador único del tercero
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del tercero
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Apellido del tercero (puede ser null en caso de empresas)
        /// </summary>
        public string Apellido { get; set; }

        /// <summary>
        /// Correo electrónico de contacto
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Número de documento del tercero
        /// </summary>
        public string NumeroDocumento { get; set; }

        /// <summary>
        /// Identificador del tipo de documento (DNI, RUC, etc.)
        /// </summary>
        public int TipoDocumentoId { get; set; }

        /// <summary>
        /// Identificador del tipo de tercero (1: Cliente, 2: Proveedor, 3: Empleado, etc.)
        /// </summary>
        public int TipoTerceroId { get; set; }

        /// <summary>
        /// Identificador de la ciudad donde reside el tercero
        /// </summary>
        public int CiudadId { get; set; }

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de última actualización del registro
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Tercero()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// Retorna el nombre completo del tercero
        /// </summary>
        /// <returns>Nombre y apellido concatenados</returns>
        public string NombreCompleto()
        {
            return $"{Nombre} {Apellido}".Trim();
        }

        /// <summary>
        /// Valida que los campos obligatorios estén completos
        /// </summary>
        /// <returns>True si la entidad es válida, False en caso contrario</returns>
        public bool EsValido()
        {
            return !string.IsNullOrEmpty(Nombre) && 
                   !string.IsNullOrEmpty(NumeroDocumento) && 
                   TipoDocumentoId > 0 && 
                   TipoTerceroId > 0 && 
                   CiudadId > 0;
        }
    }
}