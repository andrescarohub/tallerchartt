![image](https://github.com/user-attachments/assets/d3adac2f-fd0e-4b6d-ad87-6283eb5b1c20)
Sistema de Gestión Taller de Chart
Descripción del proyecto
El Sistema de Gestión Taller de Chart es una aplicación de escritorio desarrollada en C# que proporciona una solución integral para la gestión de inventario, compras y terceros para pequeñas y medianas empresas.
Características principales

Gestión de Terceros : Administración de clientes, proveedores y empleados
Control de Inventario : Seguimiento detallado de productos y stock
Gestión de Compras : Registro y seguimiento de compras de productos
Arquitectura Modular : Diseñado con una arquitectura en capas para facilitar el mantenimiento y escalabilidad.

Tecnologías utilizadas

Lenguaje : C# (.NET)
Base de datos : MySQL
Librería de conexión : MySqlConnector
Patrón de diseño :

Singleton para conexión de base de datos
Repositorio para acceso a datos
Servicios de negocios



Estructura del Proyecto
tallerc/
├── domain/
│   ├── entities/          // Modelos de datos
│   ├── repositories/      // Interfaces de repositorios
│   └── services/          // Lógica de negocio
├── infrastructure/
│   ├── mysql/             // Conexión a la base de datos
│   └── repositories/      // Implementaciones de repositorios
└── presentation/
    └── forms/             // Interfaz de usuario
Requisitos previos

.NET Framework 4.7.2 o superior
MySQL Server 5.7 o superior
Conector MySQL

Configuración del proyecto
Base de datos

Cree una llamada base de datos MySQLtallerdechart
Ejecute los scripts SQL proporcionados en la carpeta database/para crear las tablas necesarias.

Conexión a la base de datos
Modifique la cadena de conexión en infrastructure/mysql/ConexionSingleton.cs:
csharp_connectionString = "Server=localhost;Database=tallerdechart;User=root;Password=;";
Características de las Entidades
Tercero

Gestión de clientes, proveedores y empleados.
Información detallada como nombre, documento, contacto

Producto

Control de inventario
Seguimiento de stock mínimo y máximo
Código de barras

Compra

Registro de compras de productos
Seguimiento de estado (pendiente, completada, cancelada)
Detalles de compra con productos

Servicios de negocio

TerceroService: Gestión de terceros
ProductoService: Administración de productos
CompraService: Manejo de compras y actualización de inventario

Funcionalidades principales

Registro de terceros
Administración de productos
Registro de compras
Actualización automática de existencias
Validaciones de negocio integradas

Contribución

Haga un tenedor del repositorio
Cree una nueva rama ( git checkout -b feature/nueva-funcionalidad)
Realice sus cambios
Confirme sus cambios ( git commit -am 'Añadir nueva funcionalidad')
Suba la rama ( git push origin feature/nueva-funcionalidad)
Abra una solicitud de extracción

Configuración de desarrollo
Dependencias

Instale las dependencias mediante NuGet

Conector MySQL
Sistema.Datos.SqlClient



Compilación
intentodotnet restore
dotnet build
Pruebas
intentodotnet test
Consideraciones de seguridad

Utilice siempre parámetros parametrizados para prevenir la inyección SQL
Mantenga las credenciales de base de datos seguras
Implemente autenticación y autorización en versiones futuras

Licencia
[Especifique la licencia del proyecto, por ejemplo MIT, Apache, etc.]
Contacto
[Información de contacto del desarrollador o equipo]

Nota : Este sistema está en desarrollo continuo. Se recomienda realizar pruebas exhaustivas antes de su implementación en producción.
