// Program.cs (Versión Extendida)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq; // Para .Any()
using tallerc.domain.entities;
using tallerc.domain.repositories;
using tallerc.domain.services;
using tallerc.infrastructure.mysql;
using tallerc.infrastructure.repositories;

namespace tallerc
{
    static class Program
    {
        private static TerceroService? _terceroService;
        private static ProductoService? _productoService;
        private static CompraService? _compraService;

        // "Mini-servicios/repos" para catálogos (para UX en menús)
        // En una app más grande, serían clases dedicadas.
        private static Dictionary<int, string> _tiposDocumento = new Dictionary<int, string>();
        private static Dictionary<int, string> _tiposTercero = new Dictionary<int, string>();
        private static Dictionary<int, string> _ciudades = new Dictionary<int, string>();
        private static Dictionary<int, string> _categoriasProducto = new Dictionary<int, string>();


        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-CO");
            CultureInfo.CurrentUICulture = new CultureInfo("es-CO");

            Console.WriteLine("Iniciando TallerC - Sistema de Gestión (Consola)...");

            try
            {
                ConexionSingleton dbSingleton = ConexionSingleton.Instance;
                using (var testConn = dbSingleton.GetNuevaConexion())
                {
                    Console.WriteLine("Conexión a la base de datos establecida exitosamente.");
                }
                CargarCatalogos(); // Cargar datos de tablas de catálogo
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error crítico al conectar o cargar catálogos: {ex.Message}");
                Console.WriteLine("La aplicación no puede continuar. Presione cualquier tecla para salir.");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            ITerceroRepository terceroRepository = new TerceroRepository();
            IProductoRepository productoRepository = new ProductoRepository();
            ICompraRepository compraRepository = new CompraRepository();

            _terceroService = new TerceroService(terceroRepository);
            _productoService = new ProductoService(productoRepository);
            _compraService = new CompraService(compraRepository, productoRepository, terceroRepository);

            bool salir = false;
            while (!salir)
            {
                MostrarMenuPrincipal();
                Console.Write("Seleccione una opción: ");
                string opcion = Console.ReadLine()?.ToLower() ?? "";

                switch (opcion)
                {
                    case "1": GestionarProductos(); break;
                    case "2": GestionarTerceros(); break;
                    case "3": RegistrarCompra(); break;
                    case "4": VerCompras(); break;
                    case "s": salir = true; break;
                    default: Console.WriteLine("Opción no válida."); break;
                }
                if (!salir) { Console.WriteLine("\nPresione cualquier tecla para continuar..."); Console.ReadKey(); }
            }
            Console.WriteLine("Saliendo de la aplicación...");
        }

        static void CargarCatalogos()
        {
            // Esta es una forma simplificada. Idealmente, tendrías repositorios para estos.
            ConexionSingleton connSingleton = ConexionSingleton.Instance;
            using (var reader = connSingleton.ExecuteReader("SELECT Id, Nombre FROM TipoDocumento"))
                while (reader.Read()) _tiposDocumento[Convert.ToInt32(reader["Id"])] = reader["Nombre"].ToString() ?? "";
            using (var reader = connSingleton.ExecuteReader("SELECT Id, Nombre FROM TipoTercero"))
                while (reader.Read()) _tiposTercero[Convert.ToInt32(reader["Id"])] = reader["Nombre"].ToString() ?? "";
            using (var reader = connSingleton.ExecuteReader("SELECT Id, Nombre FROM Ciudad"))
                while (reader.Read()) _ciudades[Convert.ToInt32(reader["Id"])] = reader["Nombre"].ToString() ?? "";
            using (var reader = connSingleton.ExecuteReader("SELECT Id, Nombre FROM Categoria")) // Para productos
                while (reader.Read()) _categoriasProducto[Convert.ToInt32(reader["Id"])] = reader["Nombre"].ToString() ?? "";
             Console.WriteLine("Catálogos cargados.");
        }


        static void MostrarMenuPrincipal()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("======================================");
            Console.WriteLine("   TallerC - MENÚ PRINCIPAL");
            Console.WriteLine("======================================");
            Console.ResetColor();
            Console.WriteLine("1. Gestionar Productos");
            Console.WriteLine("2. Gestionar Terceros");
            Console.WriteLine("3. Registrar Nueva Compra");
            Console.WriteLine("4. Ver Compras Realizadas");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("S. Salir");
            Console.WriteLine("======================================");
        }

        // --- GESTIÓN DE PRODUCTOS ---
        static void GestionarProductos()
        {
            bool volver = false;
            while(!volver)
            {
                Console.Clear();
                Console.WriteLine("--- Gestión de Productos ---");
                Console.WriteLine("1. Ver todos los productos");
                Console.WriteLine("2. Crear nuevo producto");
                Console.WriteLine("3. Modificar producto (básico)");
                Console.WriteLine("4. Eliminar producto");
                Console.WriteLine("5. Buscar producto por nombre/barcode");
                Console.WriteLine("V. Volver al menú principal");
                Console.Write("Opción Productos: ");
                string opcionProd = Console.ReadLine()?.ToLower() ?? "";

                switch(opcionProd)
                {
                    case "1": VerTodosLosProductos(); break;
                    case "2": CrearNuevoProducto(); break;
                    case "3": ModificarProducto(); break;
                    case "4": EliminarProducto(); break;
                    case "5": BuscarProductos(); break;
                    case "v": volver = true; break;
                    default: Console.WriteLine("Opción de productos no válida."); break;
                }
                 if (!volver) { Console.WriteLine("\nPresione tecla para continuar en Gestión de Productos..."); Console.ReadKey(); }
            }
        }

        static void VerTodosLosProductos()
        {
            Console.Clear();
            Console.WriteLine("--- Listado de Productos ---");
            var productos = _productoService!.GetAllProductos();
            if (productos.Any())
            {
                Console.WriteLine($"{"ID",-3} | {"Nombre",-30} | {"Stock",-7} | {"Precio U.",-12} | {"Categoría", -20}");
                Console.WriteLine(new string('-', 80));
                foreach (var p in productos)
                {
                    string nombreCategoria = p.CategoriaId.HasValue && _categoriasProducto.ContainsKey(p.CategoriaId.Value)
                                             ? _categoriasProducto[p.CategoriaId.Value]
                                             : "N/A";
                    Console.WriteLine($"{p.Id,-3} | {p.Nombre,-30} | {p.StockActual,-7} | {p.PrecioUnitario,11:C} | {nombreCategoria, -20}");
                }
            }
            else Console.WriteLine("No hay productos registrados.");
        }

        static void CrearNuevoProducto()
        {
            Console.Clear();
            Console.WriteLine("--- Crear Nuevo Producto ---");
            try
            {
                Producto nuevoProducto = new Producto();
                Console.Write("Nombre del producto: "); nuevoProducto.Nombre = Console.ReadLine() ?? "";
                Console.Write("Stock Inicial: "); nuevoProducto.StockActual = int.Parse(Console.ReadLine() ?? "0");
                Console.Write("Precio Unitario (ej: 1500.50): "); nuevoProducto.PrecioUnitario = decimal.Parse(Console.ReadLine() ?? "0", CultureInfo.InvariantCulture);
                Console.Write("Stock Mínimo: "); nuevoProducto.StockMinimo = int.Parse(Console.ReadLine() ?? "0");
                Console.Write("Stock Máximo: "); nuevoProducto.StockMaximo = int.Parse(Console.ReadLine() ?? "100");
                Console.Write("Código de Barras (opcional): ");
                string barcode = Console.ReadLine() ?? "";
#pragma warning disable CS8601 // Possible null reference assignment.
                nuevoProducto.Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode;
#pragma warning restore CS8601 // Possible null reference assignment.

                Console.WriteLine("Categorías Disponibles:");
                foreach(var cat in _categoriasProducto) Console.WriteLine($"{cat.Key}: {cat.Value}");
                Console.Write("ID de Categoría (opcional, 0 si no aplica): ");
                int catId = int.Parse(Console.ReadLine() ?? "0");
                nuevoProducto.CategoriaId = catId > 0 ? catId : (int?)null;


                int productoId = _productoService!.CreateProducto(nuevoProducto);
                if (productoId > 0) Console.WriteLine($"Producto '{nuevoProducto.Nombre}' creado con ID: {productoId} exitosamente.", ConsoleColor.Green);
                else Console.WriteLine("Error al crear el producto.", ConsoleColor.Red);
            }
            catch (FormatException) { Console.WriteLine("Error: Formato de número inválido.", ConsoleColor.Red); }
            catch (Exception ex) { Console.WriteLine($"Error inesperado: {ex.Message}", ConsoleColor.Red); }
        }

        static void ModificarProducto()
        {
            Console.Clear();
            Console.WriteLine("--- Modificar Producto (Básico) ---");
            Console.Write("Ingrese ID del producto a modificar: ");
            if (!int.TryParse(Console.ReadLine(), out int idModificar) || idModificar <=0)
            {
                Console.WriteLine("ID inválido.", ConsoleColor.Red); return;
            }

            var producto = _productoService!.GetProductoById(idModificar);
            if (producto == null) { Console.WriteLine("Producto no encontrado.", ConsoleColor.Red); return; }

            Console.WriteLine($"Modificando: {producto.Nombre} (Stock actual: {producto.StockActual})");
            Console.Write($"Nuevo Nombre (actual: {producto.Nombre}): "); producto.Nombre = Console.ReadLine() ?? producto.Nombre;
            Console.Write($"Nuevo Stock Actual (actual: {producto.StockActual}): ");
            if (int.TryParse(Console.ReadLine(), out int nuevoStock)) producto.StockActual = nuevoStock;
            // ... pedir más campos a modificar ...
            // Por ahora, solo nombre y stock.

            if (_productoService.UpdateProducto(producto)) Console.WriteLine("Producto actualizado.", ConsoleColor.Green);
            else Console.WriteLine("Error al actualizar producto.", ConsoleColor.Red);
        }

        static void EliminarProducto()
        {
            Console.Clear();
            Console.WriteLine("--- Eliminar Producto ---");
            Console.Write("Ingrese ID del producto a eliminar: ");
             if (!int.TryParse(Console.ReadLine(), out int idEliminar) || idEliminar <=0)
            {
                Console.WriteLine("ID inválido.", ConsoleColor.Red); return;
            }
            // Aquí deberías confirmar antes de eliminar
            if (_productoService!.DeleteProducto(idEliminar)) Console.WriteLine("Producto eliminado.", ConsoleColor.Green);
            else Console.WriteLine("Error al eliminar producto (puede estar en uso o no existir).", ConsoleColor.Red);
        }
        
        static void BuscarProductos()
        {
            Console.Clear();
            Console.WriteLine("--- Buscar Productos ---");
            Console.Write("Ingrese texto a buscar en Nombre o Código de Barras: ");
            string texto = Console.ReadLine() ?? "";
            if(string.IsNullOrWhiteSpace(texto)) { Console.WriteLine("Texto de búsqueda vacío."); return;}

            var productos = _productoService!.SearchProductos(texto);
            if (productos.Any())
            {
                Console.WriteLine($"{"ID",-3} | {"Nombre",-30} | {"Stock",-7} | {"Precio U.",-12} | {"Categoría", -20}");
                Console.WriteLine(new string('-', 80));
                foreach (var p in productos)
                {
                    string nombreCategoria = p.CategoriaId.HasValue && _categoriasProducto.ContainsKey(p.CategoriaId.Value)
                                             ? _categoriasProducto[p.CategoriaId.Value]
                                             : "N/A";
                    Console.WriteLine($"{p.Id,-3} | {p.Nombre,-30} | {p.StockActual,-7} | {p.PrecioUnitario,11:C} | {nombreCategoria, -20}");
                }
            }
            else Console.WriteLine("No se encontraron productos con ese criterio.");
        }

        // --- GESTIÓN DE TERCEROS ---
        static void GestionarTerceros()
        {
             bool volver = false;
            while(!volver)
            {
                Console.Clear();
                Console.WriteLine("--- Gestión de Terceros ---");
                Console.WriteLine("1. Ver todos los terceros");
                Console.WriteLine("2. Ver Clientes");
                Console.WriteLine("3. Ver Proveedores");
                Console.WriteLine("4. Ver Empleados");
                Console.WriteLine("5. Crear nuevo tercero");
                Console.WriteLine("6. Modificar tercero (básico)");
                Console.WriteLine("7. Eliminar tercero");
                Console.WriteLine("8. Buscar tercero por nombre");
                Console.WriteLine("V. Volver al menú principal");
                Console.Write("Opción Terceros: ");
                string opcionTer = Console.ReadLine()?.ToLower() ?? "";

                switch(opcionTer)
                {
                    case "1": VerTercerosPorTipo(null); break; // null para todos
                    case "2": VerTercerosPorTipo(1); break; // 1: Cliente (ajusta ID si es diferente)
                    case "3": VerTercerosPorTipo(2); break; // 2: Proveedor
                    case "4": VerTercerosPorTipo(3); break; // 3: Empleado
                    case "5": CrearNuevoTercero(); break;
                    case "6": ModificarTercero(); break;
                    case "7": EliminarTercero(); break;
                    case "8": BuscarTerceros(); break;
                    case "v": volver = true; break;
                    default: Console.WriteLine("Opción de terceros no válida."); break;
                }
                if (!volver) { Console.WriteLine("\nPresione tecla para continuar en Gestión de Terceros..."); Console.ReadKey(); }
            }
        }

        static void VerTercerosPorTipo(int? tipoTerceroId)
        {
            Console.Clear();
            string titulo = tipoTerceroId.HasValue ? $"--- Listado de {_tiposTercero.GetValueOrDefault(tipoTerceroId.Value, "Desconocidos")} ---" : "--- Listado de Todos los Terceros ---";
            Console.WriteLine(titulo);

            List<Tercero> terceros;
            if (tipoTerceroId.HasValue)
            {
                terceros = _terceroService!.GetByTipo(tipoTerceroId.Value); // Necesitas GetByTipo en ITerceroService y su impl.
            }
            else
            {
                terceros = _terceroService!.GetAllTerceros();
            }

            if (terceros.Any())
            {
                Console.WriteLine($"{"ID",-3} | {"Nombre Completo",-35} | {"Documento",-15} | {"Tipo",-15} | {"Ciudad",-15}");
                Console.WriteLine(new string('-', 90));
                foreach (var t in terceros)
                {
                    string tipoDoc = _tiposDocumento.GetValueOrDefault(t.TipoDocumentoId, "N/A");
                    string tipoTer = _tiposTercero.GetValueOrDefault(t.TipoTerceroId, "N/A");
                    string ciudad = _ciudades.GetValueOrDefault(t.CiudadId, "N/A");
                    Console.WriteLine($"{t.Id,-3} | {t.NombreCompleto(),-35} | {tipoDoc + ":" + t.NumeroDocumento,-15} | {tipoTer,-15} | {ciudad,-15}");
                }
            }
            else Console.WriteLine("No hay terceros registrados para este tipo.");
        }
        
        static void CrearNuevoTercero()
        {
            Console.Clear();
            Console.WriteLine("--- Crear Nuevo Tercero ---");
            try
            {
                Tercero nuevoTercero = new Tercero();
                Console.Write("Nombre: "); nuevoTercero.Nombre = Console.ReadLine() ?? "";
                Console.Write("Apellido (opcional): "); 
                string apellido = Console.ReadLine() ?? "";
#pragma warning disable CS8601 // Possible null reference assignment.
                nuevoTercero.Apellido = string.IsNullOrWhiteSpace(apellido) ? null : apellido;
#pragma warning restore CS8601 // Possible null reference assignment.

                Console.Write("Email (opcional): "); 
                string email = Console.ReadLine() ?? "";
#pragma warning disable CS8601 // Possible null reference assignment.
                nuevoTercero.Email = string.IsNullOrWhiteSpace(email) ? null : email;
#pragma warning restore CS8601 // Possible null reference assignment.

                Console.Write("Número de Documento: "); nuevoTercero.NumeroDocumento = Console.ReadLine() ?? "";

                Console.WriteLine("Tipos de Documento:");
                foreach(var td in _tiposDocumento) Console.WriteLine($"{td.Key}: {td.Value}");
                Console.Write("ID Tipo Documento: "); nuevoTercero.TipoDocumentoId = int.Parse(Console.ReadLine() ?? "0");

                Console.WriteLine("Tipos de Tercero:");
                foreach(var tt in _tiposTercero) Console.WriteLine($"{tt.Key}: {tt.Value}");
                Console.Write("ID Tipo Tercero: "); nuevoTercero.TipoTerceroId = int.Parse(Console.ReadLine() ?? "0");

                Console.WriteLine("Ciudades:");
                foreach(var c in _ciudades) Console.WriteLine($"{c.Key}: {c.Value}");
                Console.Write("ID Ciudad: "); nuevoTercero.CiudadId = int.Parse(Console.ReadLine() ?? "0");

                int terceroId = _terceroService!.CreateTercero(nuevoTercero);
                if (terceroId > 0) Console.WriteLine($"Tercero '{nuevoTercero.NombreCompleto()}' creado con ID: {terceroId}.", ConsoleColor.Green);
                else Console.WriteLine("Error al crear el tercero. Verifique si el documento ya existe o los IDs de catálogo son válidos.", ConsoleColor.Red);
            }
            catch (FormatException) { Console.WriteLine("Error: Formato de número inválido para un ID.", ConsoleColor.Red); }
            catch (Exception ex) { Console.WriteLine($"Error inesperado: {ex.Message}", ConsoleColor.Red); }
        }

        static void ModificarTercero()
        {
            Console.Clear();
            Console.WriteLine("--- Modificar Tercero (Básico) ---");
            Console.Write("Ingrese ID del tercero a modificar: ");
             if (!int.TryParse(Console.ReadLine(), out int idModificar) || idModificar <=0)
            {
                Console.WriteLine("ID inválido.", ConsoleColor.Red); return;
            }
            var tercero = _terceroService!.GetTerceroById(idModificar);
            if (tercero == null) { Console.WriteLine("Tercero no encontrado.", ConsoleColor.Red); return; }

            Console.WriteLine($"Modificando: {tercero.NombreCompleto()}");
            Console.Write($"Nuevo Nombre (actual: {tercero.Nombre}): "); tercero.Nombre = Console.ReadLine() ?? tercero.Nombre;
            // Pedir más campos...
            
            if (_terceroService.UpdateTercero(tercero)) Console.WriteLine("Tercero actualizado.", ConsoleColor.Green);
            else Console.WriteLine("Error al actualizar tercero.", ConsoleColor.Red);
        }

        static void EliminarTercero()
        {
            Console.Clear();
            Console.WriteLine("--- Eliminar Tercero ---");
            Console.Write("Ingrese ID del tercero a eliminar: ");
            if (!int.TryParse(Console.ReadLine(), out int idEliminar) || idEliminar <=0)
            {
                Console.WriteLine("ID inválido.", ConsoleColor.Red); return;
            }
            if (_terceroService!.DeleteTercero(idEliminar)) Console.WriteLine("Tercero eliminado.", ConsoleColor.Green);
            else Console.WriteLine("Error al eliminar tercero (puede estar en uso o no existir).", ConsoleColor.Red);
        }

        static void BuscarTerceros()
        {
            Console.Clear();
            Console.WriteLine("--- Buscar Terceros por Nombre/Apellido ---");
            Console.Write("Ingrese texto a buscar: ");
            string texto = Console.ReadLine() ?? "";
            if(string.IsNullOrWhiteSpace(texto)) { Console.WriteLine("Texto de búsqueda vacío."); return;}

            var terceros = _terceroService!.SearchTerceros(texto);
             if (terceros.Any())
            {
                Console.WriteLine($"{"ID",-3} | {"Nombre Completo",-35} | {"Documento",-15} | {"Tipo",-15} | {"Ciudad",-15}");
                Console.WriteLine(new string('-', 90));
                foreach (var t in terceros)
                {
                    string tipoDoc = _tiposDocumento.GetValueOrDefault(t.TipoDocumentoId, "N/A");
                    string tipoTer = _tiposTercero.GetValueOrDefault(t.TipoTerceroId, "N/A");
                    string ciudad = _ciudades.GetValueOrDefault(t.CiudadId, "N/A");
                    Console.WriteLine($"{t.Id,-3} | {t.NombreCompleto(),-35} | {tipoDoc + ":" + t.NumeroDocumento,-15} | {tipoTer,-15} | {ciudad,-15}");
                }
            }
            else Console.WriteLine("No se encontraron terceros con ese criterio.");
        }

        // --- GESTIÓN DE COMPRAS ---
        static void RegistrarCompra() // Tu lógica existente, con pequeños ajustes
        {
            Console.Clear();
            Console.WriteLine("--- Registrar Nueva Compra ---");
            try
            {
                Console.WriteLine("Proveedores Disponibles:");
                VerTercerosPorTipo(2); // Asumiendo 2 es Proveedor
                Console.Write("ID del Proveedor: ");
                int proveedorId = int.Parse(Console.ReadLine() ?? "0");
                var proveedor = _terceroService!.GetTerceroById(proveedorId);
                if (proveedor == null || proveedor.TipoTerceroId != 2) { Console.WriteLine("Proveedor no válido.", ConsoleColor.Red); return; }

                Console.WriteLine("Empleados Disponibles:");
                VerTercerosPorTipo(3); // Asumiendo 3 es Empleado
                Console.Write("ID del Empleado que registra: ");
                int empleadoId = int.Parse(Console.ReadLine() ?? "0");
                var empleado = _terceroService!.GetTerceroById(empleadoId);
                if (empleado == null || empleado.TipoTerceroId != 3) { Console.WriteLine("Empleado no válido.", ConsoleColor.Red); return; }

                Console.Write("Número de Factura: ");
                string numFactura = Console.ReadLine() ?? "";

                Compra nuevaCompra = new Compra { ProveedorId = proveedorId, EmpleadoId = empleadoId, NumeroFactura = numFactura };

                bool agregarMasProductos = true;
                while (agregarMasProductos)
                {
                    Console.WriteLine("Productos Disponibles (parcial):"); // Mostrar algunos productos para ayudar
                    var prodsMuestra = _productoService!.GetAllProductos().Take(5);
                    foreach(var pMuestra in prodsMuestra) Console.WriteLine($"ID: {pMuestra.Id} - {pMuestra.Nombre}");

                    Console.Write("ID del Producto a comprar (0 para terminar): ");
                    int productoId = int.Parse(Console.ReadLine() ?? "0");
                    if (productoId == 0) { agregarMasProductos = false; continue; }

                    var producto = _productoService!.GetProductoById(productoId);
                    if (producto == null) { Console.WriteLine("Producto no encontrado.", ConsoleColor.Red); continue; }

                    Console.Write($"Cantidad de '{producto.Nombre}': "); int cantidad = int.Parse(Console.ReadLine() ?? "0");
                    Console.Write($"Precio de compra unitario para '{producto.Nombre}': "); decimal precioCompra = decimal.Parse(Console.ReadLine() ?? "0", CultureInfo.InvariantCulture);
                    if(cantidad <=0 || precioCompra <0) { Console.WriteLine("Cantidad o precio inválido.", ConsoleColor.Red); continue;}

                    nuevaCompra.AgregarDetalle(new DetalleCompra { ProductoId = productoId, Cantidad = cantidad, Valor = precioCompra });
                }

                if (!nuevaCompra.Detalles.Any()) { Console.WriteLine("No se agregaron productos. Compra cancelada."); return; }

                int compraId = _compraService!.CreateCompra(nuevaCompra);
                if (compraId > 0) Console.WriteLine($"Compra registrada con ID: {compraId}. Stock actualizado.", ConsoleColor.Green);
                else Console.WriteLine("Error al registrar la compra.", ConsoleColor.Red);
            }
            catch (FormatException) { Console.WriteLine("Error: Formato de número inválido.", ConsoleColor.Red); }
            catch (Exception ex) { Console.WriteLine($"Error inesperado: {ex.Message}", ConsoleColor.Red); }
        }

        static void VerCompras()
        {
            Console.Clear();
            Console.WriteLine("--- Listado de Compras ---");
            var compras = _compraService!.GetAllCompras();
            if (compras.Any())
            {
                Console.WriteLine($"{"ID",-3} | {"Fecha",-11} | {"Proveedor",-25} | {"Factura #",-12} | {"Estado",-12} | {"Total",12}");
                Console.WriteLine(new string('-', 90));
                foreach (var c in compras)
                {
                    var proveedor = _terceroService!.GetTerceroById(c.ProveedorId);
                    string nombreProveedor = proveedor?.NombreCompleto() ?? "N/A";
                    // Para calcular el total, necesitaríamos cargar los detalles aquí
                    // o que la entidad Compra lo calcule si los detalles ya están cargados.
                    // Por ahora, no mostramos total para simplificar.
                    // Si GetCompraConDetalles carga los detalles, podríamos hacer:
                    // var compraConDetalles = _compraService.GetCompraById(c.Id);
                    // decimal totalCompra = compraConDetalles?.CalcularTotal() ?? 0;
                    
                    Console.WriteLine($"{c.Id,-3} | {c.Fecha.ToShortDateString(),-11} | {nombreProveedor,-25} | {c.NumeroFactura,-12} | {EstadoCompraToString(c.Estado),-12} | {"PENDIENTE",12}");
                }
            }
            else Console.WriteLine("No hay compras registradas.");
        }
        
        static string EstadoCompraToString(int estado)
        {
            return estado switch { 1 => "Pendiente", 2 => "Completada", 3 => "Cancelada", _ => "Desconocido" };
        }

        // Helper para escribir con color y resetear
        static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }
    }
}