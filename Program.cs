using System;
using System.Collections.Generic; // Para List
using System.Globalization; // Para CultureInfo
using tallerc.domain.entities;
using tallerc.domain.repositories;
using tallerc.domain.services;
using tallerc.infrastructure.mysql;
using tallerc.infrastructure.repositories;

namespace tallerc
{
    static class Program
    {
        // Declaramos los servicios aquí para que sean accesibles por los métodos del menú
        private static TerceroService?  _terceroService;
        private static ProductoService?  _productoService;
        private static CompraService? _compraService;
        // private static VentaService _ventaService; // Cuando lo implementemos
        // private static CajaService _cajaService; // Cuando lo implementemos
        // private static PlanPromocionalService _planPromocionalService; // Cuando lo implementemos


        static void Main(string[] args) // Cambiado a Main(string[] args) por convención de consola
        {
            // Configurar la cultura para el formato de números y fechas si es necesario
            CultureInfo.CurrentCulture = new CultureInfo("es-CO"); // Ejemplo para Colombia
            CultureInfo.CurrentUICulture = new CultureInfo("es-CO");

            Console.WriteLine("Iniciando TallerC - Sistema de Gestión (Consola)...");

            // --- Configuración de Dependencias ---
            try
            {
                ConexionSingleton dbSingleton = ConexionSingleton.Instance;
                using (var testConn = dbSingleton.GetNuevaConexion())
                {
                    Console.WriteLine("Conexión a la base de datos establecida exitosamente.");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error crítico al conectar con la base de datos: {ex.Message}");
                Console.WriteLine("La aplicación no puede continuar. Presione cualquier tecla para salir.");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            ITerceroRepository terceroRepository = new TerceroRepository();
            IProductoRepository productoRepository = new ProductoRepository();
            // IDetalleCompraRepository detalleCompraRepository = new DetalleCompraRepository(); // No es estrictamente necesario si CompraRepo lo maneja
            ICompraRepository compraRepository = new CompraRepository();

            _terceroService = new TerceroService(terceroRepository);
            _productoService = new ProductoService(productoRepository);
            _compraService = new CompraService(compraRepository, productoRepository, terceroRepository);

            // --- Bucle Principal del Menú ---
            bool salir = false;
            while (!salir)
            {
                MostrarMenuPrincipal();
                Console.Write("Seleccione una opción: ");
                string opcion = Console.ReadLine() ?? "";

                switch (opcion.ToLower())
                {
                    case "1":
                        GestionarProductos();
                        break;
                    case "2":
                        GestionarTerceros();
                        break;
                    case "3":
                        RegistrarCompra();
                        break;
                    case "4":
                        VerCompras();
                        break;
                    // Añadir más casos para ventas, caja, planes, etc.
                    case "s":
                    case "salir":
                        salir = true;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Opción no válida. Intente de nuevo.");
                        Console.ResetColor();
                        break;
                }
                if (!salir)
                {
                    Console.WriteLine("\nPresione cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
            Console.WriteLine("Saliendo de la aplicación...");
        }

        static void MostrarMenuPrincipal()
        {
            Console.Clear(); // Limpiar la consola
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("======================================");
            Console.WriteLine("   TallerC - MENÚ PRINCIPAL");
            Console.WriteLine("======================================");
            Console.ResetColor();
            Console.WriteLine("1. Gestionar Productos");
            Console.WriteLine("2. Gestionar Terceros (Clientes/Proveedores/Empleados)");
            Console.WriteLine("3. Registrar Nueva Compra");
            Console.WriteLine("4. Ver Compras Realizadas");
            // Console.WriteLine("5. Registrar Nueva Venta");
            // Console.WriteLine("6. Gestionar Caja");
            // Console.WriteLine("7. Gestionar Planes Promocionales");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("S. Salir");
            Console.WriteLine("======================================");
        }

        // --- Métodos para cada opción del menú ---

        static void GestionarProductos()
        {
            Console.Clear();
            Console.WriteLine("--- Gestión de Productos ---");
            // Aquí iría un submenú: Ver todos, Buscar, Crear, Modificar, Eliminar
            Console.WriteLine("1. Ver todos los productos");
            Console.WriteLine("2. Crear nuevo producto");
            Console.WriteLine("3. Modificar producto");
            Console.WriteLine("4. Eliminar producto");
            Console.WriteLine("V. Volver al menú principal");
            Console.Write("Opción Productos: ");
            string opcionProd = Console.ReadLine()?.ToLower() ?? "";

            switch(opcionProd)
            {
                case "1":
                    VerTodosLosProductos();
                    break;
                case "2":
                    CrearNuevoProducto();
                    break;
                // Implementar casos 3 y 4
                case "v":
                    return;
                default:
                    Console.WriteLine("Opción de productos no válida.");
                    break;
            }
        }

        static void VerTodosLosProductos()
        {
            Console.Clear();
            Console.WriteLine("--- Listado de Productos ---");
            var productos = _productoService.GetAllProductos();
            if (productos.Any())
            {
                Console.WriteLine("ID | Nombre                     | Stock | Precio Unit.");
                Console.WriteLine("-------------------------------------------------------");
                foreach (var p in productos)
                {
                    Console.WriteLine($"{p.Id,-2} | {p.Nombre,-26} | {p.StockActual,-5} | {p.PrecioUnitario,10:C}");
                }
            }
            else
            {
                Console.WriteLine("No hay productos registrados.");
            }
        }

        static void CrearNuevoProducto()
        {
            Console.Clear();
            Console.WriteLine("--- Crear Nuevo Producto ---");
            try
            {
                Console.Write("Nombre del producto: ");
                string nombre = Console.ReadLine() ?? "";
                
                Console.Write("Stock Inicial: ");
                int stock = int.Parse(Console.ReadLine() ?? "0");
                
                Console.Write("Precio Unitario (ej: 1500.50): ");
                decimal precio = decimal.Parse(Console.ReadLine() ?? "0", CultureInfo.InvariantCulture); // Usar InvariantCulture para decimales con punto

                Console.Write("Stock Mínimo: ");
                int stockMin = int.Parse(Console.ReadLine() ?? "0");

                Console.Write("Stock Máximo: ");
                int stockMax = int.Parse(Console.ReadLine() ?? "100");

                Console.Write("Código de Barras (opcional): ");
                string barcode = Console.ReadLine() ?? "";


                Producto nuevoProducto = new Producto
                {
                    Nombre = nombre,
                    StockActual = stock,
                    PrecioUnitario = precio,
                    StockMinimo = stockMin,
                    StockMaximo = stockMax,
                    Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode
                };

                // Validaciones del servicio (EsValido, duplicado por barcode) se ejecutan dentro de CreateProducto
                int productoId = _productoService.CreateProducto(nuevoProducto);

                if (productoId > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Producto '{nuevoProducto.Nombre}' creado con ID: {productoId} exitosamente.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error al crear el producto. Verifique los datos o si el código de barras ya existe.");
                    Console.ResetColor();
                }
            }
            catch (FormatException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Formato de número inválido.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error inesperado: {ex.Message}");
                Console.ResetColor();
            }
        }


        static void GestionarTerceros()
        {
            Console.Clear();
            Console.WriteLine("--- Gestión de Terceros ---");
            // Similar a productos: Ver, Crear, Modificar, Eliminar
            // Por simplicidad, solo mostraremos un ejemplo de creación
            Console.WriteLine("1. Crear nuevo tercero");
            Console.WriteLine("V. Volver al menú principal");
            Console.Write("Opción Terceros: ");
            string opcionTer = Console.ReadLine()?.ToLower() ?? "";
             switch(opcionTer)
            {
                case "1":
                    CrearNuevoTercero();
                    break;
                case "v":
                    return;
                default:
                    Console.WriteLine("Opción de terceros no válida.");
                    break;
            }
        }
        
        static void CrearNuevoTercero()
        {
            // ... (Implementar lógica para pedir datos del tercero y llamar a _terceroService.CreateTercero)
            // Necesitarás pedir Nombre, Apellido, Email, NumeroDocumento, TipoDocumentoId, TipoTerceroId, CiudadId
            // Y la entidad Tercero.cs debe tener estas propiedades.
            Console.WriteLine("FUNCIONALIDAD CrearNuevoTercero PENDIENTE DE IMPLEMENTAR.");
        }


        static void RegistrarCompra()
        {
            Console.Clear();
            Console.WriteLine("--- Registrar Nueva Compra ---");
            try
            {
                Console.Write("ID del Proveedor: ");
                int proveedorId = int.Parse(Console.ReadLine() ?? "0");
                // Validar que el proveedor exista y sea tipo proveedor
                var proveedor = _terceroService.GetTerceroById(proveedorId);
                if (proveedor == null || proveedor.TipoTerceroId != 2) // Asumiendo 2 es Proveedor
                {
                    Console.WriteLine("Proveedor no válido o no encontrado.");
                    return;
                }

                Console.Write("ID del Empleado que registra: ");
                int empleadoId = int.Parse(Console.ReadLine() ?? "0");
                 // Validar que el empleado exista y sea tipo empleado
                var empleado = _terceroService.GetTerceroById(empleadoId);
                if (empleado == null || empleado.TipoTerceroId != 3) // Asumiendo 3 es Empleado
                {
                    Console.WriteLine("Empleado no válido o no encontrado.");
                    return;
                }

                Console.Write("Número de Factura: ");
                string numFactura = Console.ReadLine() ?? "";

                Compra nuevaCompra = new Compra
                {
                    ProveedorId = proveedorId,
                    EmpleadoId = empleadoId,
                    NumeroFactura = numFactura,
                    Fecha = DateTime.Now, // El constructor de Compra ya lo hace
                    Estado = 1 // Pendiente, el constructor de Compra ya lo hace
                };

                List<DetalleCompra> detalles = new List<DetalleCompra>();
                bool agregarMasProductos = true;
                while (agregarMasProductos)
                {
                    Console.Write("ID del Producto a comprar (0 para terminar): ");
                    int productoId = int.Parse(Console.ReadLine() ?? "0");
                    if (productoId == 0)
                    {
                        agregarMasProductos = false;
                        continue;
                    }

                    var producto = _productoService.GetProductoById(productoId);
                    if (producto == null)
                    {
                        Console.WriteLine("Producto no encontrado.");
                        continue;
                    }

                    Console.Write($"Cantidad de '{producto.Nombre}': ");
                    int cantidad = int.Parse(Console.ReadLine() ?? "0");
                    Console.Write($"Precio de compra unitario para '{producto.Nombre}': ");
                    decimal precioCompra = decimal.Parse(Console.ReadLine() ?? "0", CultureInfo.InvariantCulture);

                    detalles.Add(new DetalleCompra
                    {
                        ProductoId = productoId,
                        Cantidad = cantidad,
                        Valor = precioCompra // 'Valor' en DetalleCompra es el precio de compra
                    });
                }

                if (!detalles.Any())
                {
                    Console.WriteLine("No se agregaron productos. Compra cancelada.");
                    return;
                }
                nuevaCompra.Detalles = detalles;

                int compraId = _compraService.CreateCompra(nuevaCompra); // CreateCompra actualiza stock
                if (compraId > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Compra registrada con ID: {compraId}. Stock actualizado.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error al registrar la compra. Verifique los datos.");
                    Console.ResetColor();
                }
            }
            catch (FormatException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Formato de número inválido.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error inesperado: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void VerCompras()
        {
            Console.Clear();
            Console.WriteLine("--- Listado de Compras ---");
            var compras = _compraService.GetAllCompras(); // Este GetAll no carga detalles por defecto
            if (compras.Any())
            {
                Console.WriteLine("ID | Fecha      | ProveedorID | EmpleadoID | Factura #   | Estado");
                Console.WriteLine("--------------------------------------------------------------------");
                foreach (var c in compras)
                {
                    // Para ver nombres de proveedor/empleado necesitaríamos cargarlos
                    Console.WriteLine($"{c.Id,-2} | {c.Fecha.ToShortDateString(),-10} | {c.ProveedorId,-11} | {c.EmpleadoId,-10} | {c.NumeroFactura,-11} | {EstadoCompraToString(c.Estado)}");
                    // Para ver detalles, llamarías a _compraService.GetCompraById(c.Id) que sí los carga
                }
            }
            else
            {
                Console.WriteLine("No hay compras registradas.");
            }
        }
        
        static string EstadoCompraToString(int estado)
        {
            switch (estado)
            {
                case 1: return "Pendiente";
                case 2: return "Completada";
                case 3: return "Cancelada";
                default: return "Desconocido";
            }
        }

    }
}