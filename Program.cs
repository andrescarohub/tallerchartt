using System;
using MySqlConnector;
using System.Data;

namespace TallerC
{
    class Program
    {
        static string connectionString = "Server=localhost;Database=tallerdechart;User=root;Password=;";

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA DE GESTIÓN ===");
                Console.WriteLine("1. Gestión de Terceros");
                Console.WriteLine("2. Gestión de Productos");
                Console.WriteLine("3. Registrar Compra");
                Console.WriteLine("4. Registrar Venta");
                Console.WriteLine("5. Movimientos de Caja");
                Console.WriteLine("6. Planes Promocionales");
                Console.WriteLine("0. Salir");
                Console.Write("Seleccione una opción: ");

                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        GestionTerceros();
                        break;
                    case "2":
                        GestionProductos();
                        break;
                    case "3":
                        RegistrarCompra();
                        break;
                    case "4":
                        RegistrarVenta();
                        break;
                    case "5":
                        MovimientosCaja();
                        break;
                    case "6":
                        PlanesPromocionales();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
                Console.WriteLine("\nPresione cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }

        // === FUNCIONES CRUD BÁSICAS ===
        static void GestionTerceros()
        {
            Console.Clear();
            Console.WriteLine("=== GESTIÓN DE TERCEROS ===");
            Console.WriteLine("1. Listar");
            Console.WriteLine("2. Agregar");
            Console.WriteLine("3. Editar");
            Console.WriteLine("4. Eliminar");
            string opcion = Console.ReadLine();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                switch (opcion)
                {
                    case "1":
                        var cmd = new MySqlCommand("SELECT * FROM Terceros", connection);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine($"ID: {reader["Id"]}, Nombre: {reader["Nombre"]}, Tipo: {reader["TipoTerceroId"]}");
                            }
                        }
                        break;
                    case "2":
                        Console.Write("Nombre: ");
                        string nombre = Console.ReadLine();
                        Console.Write("Tipo (1=Cliente, 2=Proveedor, 3=Empleado): ");
                        int tipo = int.Parse(Console.ReadLine());

                        var insertCmd = new MySqlCommand(
                            "INSERT INTO Terceros (Nombre, TipoTerceroId) VALUES (@nombre, @tipo)", 
                            connection
                        );
                        insertCmd.Parameters.AddWithValue("@nombre", nombre);
                        insertCmd.Parameters.AddWithValue("@tipo", tipo);
                        insertCmd.ExecuteNonQuery();
                        Console.WriteLine("Tercero agregado!");
                        break;
                    // ... (implementar Editar/Eliminar de forma similar)
                }
            }
        }

        // === FUNCIONES CLAVE DEL SISTEMA ===
        static void RegistrarCompra()
        {
            Console.Clear();
            Console.WriteLine("=== REGISTRAR COMPRA ===");
            
            // 1. Seleccionar proveedor
            Console.WriteLine("Proveedores:");
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new MySqlCommand("SELECT Id, Nombre FROM Terceros WHERE TipoTerceroId = 2", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Id"]}. {reader["Nombre"]}");
                    }
                }
            }
            Console.Write("ID Proveedor: ");
            int proveedorId = int.Parse(Console.ReadLine());

            // 2. Registrar productos comprados (simplificado)
            Console.Write("ID Producto: ");
            int productoId = int.Parse(Console.ReadLine());
            Console.Write("Cantidad: ");
            int cantidad = int.Parse(Console.ReadLine());

            // 3. Actualizar stock y registrar compra (transacción)
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Actualizar stock
                        var updateStock = new MySqlCommand(
                            "UPDATE Productos SET StockActual = StockActual + @cantidad WHERE Id = @id",
                            connection, transaction
                        );
                        updateStock.Parameters.AddWithValue("@cantidad", cantidad);
                        updateStock.Parameters.AddWithValue("@id", productoId);
                        updateStock.ExecuteNonQuery();

                        // Registrar compra
                        var insertCompra = new MySqlCommand(
                            "INSERT INTO Compras (ProveedorId, Fecha, Total) VALUES (@proveedorId, @fecha, @total); SELECT LAST_INSERT_ID();",
                            connection, transaction
                        );
                        insertCompra.Parameters.AddWithValue("@proveedorId", proveedorId);
                        insertCompra.Parameters.AddWithValue("@fecha", DateTime.Now);
                        insertCompra.Parameters.AddWithValue("@total", 0); // Simplificado
                        int compraId = Convert.ToInt32(insertCompra.ExecuteScalar());

                        transaction.Commit();
                        Console.WriteLine("¡Compra registrada y stock actualizado!");
                    }
                    catch
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error. Se revertieron los cambios.");
                    }
                }
            }
        }

        static void MovimientosCaja()
        {
            Console.Clear();
            Console.WriteLine("=== MOVIMIENTOS DE CAJA ===");
            Console.WriteLine("1. Apertura de caja");
            Console.WriteLine("2. Cierre de caja");
            Console.WriteLine("3. Registrar movimiento");
            string opcion = Console.ReadLine();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                switch (opcion)
                {
                    case "1":
                        Console.Write("Monto inicial: ");
                        decimal montoInicial = decimal.Parse(Console.ReadLine());
                        
                        var cmd = new MySqlCommand(
                            "INSERT INTO Caja (FechaApertura, MontoInicial, Estado) VALUES (@fecha, @monto, 'Abierta')",
                            connection
                        );
                        cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                        cmd.Parameters.AddWithValue("@monto", montoInicial);
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Caja abierta correctamente.");
                        break;
                    
                    case "3":
                        Console.WriteLine("Tipos: 1=Ingreso, 2=Egreso");
                        Console.Write("Tipo: ");
                        int tipo = int.Parse(Console.ReadLine());
                        Console.Write("Monto: ");
                        decimal monto = decimal.Parse(Console.ReadLine());
                        Console.Write("Descripción: ");
                        string desc = Console.ReadLine();

                        var insertMov = new MySqlCommand(
                            "INSERT INTO MovimientosCaja (Tipo, Monto, Descripcion, Fecha) VALUES (@tipo, @monto, @desc, @fecha)",
                            connection
                        );
                        insertMov.Parameters.AddWithValue("@tipo", tipo);
                        insertMov.Parameters.AddWithValue("@monto", monto);
                        insertMov.Parameters.AddWithValue("@desc", desc);
                        insertMov.Parameters.AddWithValue("@fecha", DateTime.Now);
                        insertMov.ExecuteNonQuery();

                        string accion = tipo == 1 ? "Ingreso" : "Egreso";
                        Console.WriteLine($"{accion} registrado!");
                        break;
                }
            }
        }

        // ... (implementar las demás funciones como RegistrarVenta() y PlanesPromocionales())
    }
}