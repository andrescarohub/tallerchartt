
using System;

namespace tallerc.domain.entities
{
    /// <summary>
    /// Representa los productos gestionados en el sistema.
    /// </summary>
    public class Producto
    {
        /// <summary>
        /// Identificador único del producto
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre o descripción del producto
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Cantidad actual en stock
        /// </summary>
        public int StockActual { get; set; }

        /// <summary>
        /// Nivel mínimo de stock antes de reordenar
        /// </summary>
        public int StockMinimo { get; set; }

        /// <summary>
        /// Nivel máximo de stock que se puede mantener
        /// </summary>
        public int StockMaximo { get; set; }

        /// <summary>
        /// Código de barras del producto
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// Precio unitario del producto
        /// </summary>
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Categoría del producto (puede ser null)
        /// </summary>
        public int? CategoriaId { get; set; }

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
        public Producto()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
             Nombre = string.Empty; // <--- AÑADE ESTO O UN VALOR INICIAL
            StockActual = 0;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// Verifica si el producto está por debajo del stock mínimo
        /// </summary>
        /// <returns>True si el stock actual es menor al stock mínimo</returns>
        public bool RequiereReposicion()
        {
            return StockActual < StockMinimo;
        }

        /// <summary>
        /// Verifica si hay espacio para aumentar el stock
        /// </summary>
        /// <param name="cantidad">Cantidad a aumentar</param>
        /// <returns>True si la cantidad total no supera el stock máximo</returns>
        public bool PuedeAumentarStock(int cantidad)
        {
            return (StockActual + cantidad) <= StockMaximo;
        }

        /// <summary>
        /// Valida que los campos obligatorios estén completos
        /// </summary>
        /// <returns>True si la entidad es válida, False en caso contrario</returns>
        public bool EsValido()
        {
            return !string.IsNullOrEmpty(Nombre) && 
                   StockMinimo >= 0 && 
                   StockMaximo > StockMinimo && 
                   PrecioUnitario > 0;
        }
    }
}