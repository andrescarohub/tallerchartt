CREATE DATABASE tallerdechart;
-- USE tallerdechart;

-- Tabla: tipotercero
CREATE TABLE tipotercero (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(255) NULL -- Asumiendo 255 por el '25...'
);

-- Tabla: tipodocumento
CREATE TABLE tipodocumento (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(255) NULL -- Asumiendo 255 por el '25...'
);

-- Tabla: ciudad
CREATE TABLE ciudad (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(100) NOT NULL -- Asumiendo 100 por el '10...'
);

-- Tabla: categoria
CREATE TABLE categoria (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(100) NOT NULL, -- Asumiendo 100 por el '10...'
    Descripcion TEXT NULL
);

-- Tabla: planpromocional
CREATE TABLE planpromocional (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(150) NOT NULL,
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME NOT NULL,
    Descripcion TEXT NULL,
    Activo TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Tabla: tercero (Depende de tipotercero, tipodocumento, ciudad)
CREATE TABLE tercero (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(150) NOT NULL,
    Apellido VARCHAR(150) NULL,
    Email VARCHAR(100) UNIQUE NULL,
    NumeroDocumento VARCHAR(50) NOT NULL,
    TipoDocumentoId INT NOT NULL,
    TipoTerceroId INT NOT NULL,
    CiudadId INT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (TipoDocumentoId) REFERENCES tipodocumento(Id),
    FOREIGN KEY (TipoTerceroId) REFERENCES tipotercero(Id),
    FOREIGN KEY (CiudadId) REFERENCES ciudad(Id)
    -- Considerar agregar un índice UNIQUE en (TipoDocumentoId, NumeroDocumento)
    -- UNIQUE INDEX UQ_tercero_documento (TipoDocumentoId, NumeroDocumento)
);

-- Tabla: producto (Depende de categoria)
CREATE TABLE producto (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(255) NOT NULL,
    StockActual INT NOT NULL DEFAULT 0,
    StockMinimo INT NULL DEFAULT 0,
    StockMaximo INT NULL DEFAULT 0,
    Barcode VARCHAR(100) UNIQUE NULL,
    PrecioUnitario DECIMAL(12,2) NOT NULL,
    CategoriaId INT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (CategoriaId) REFERENCES categoria(Id)
);

-- Tabla: compra (Depende de tercero)
CREATE TABLE compra (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    ProveedorId INT NOT NULL,
    EmpleadoId INT NOT NULL,
    Fecha DATETIME NOT NULL,
    NumeroFactura VARCHAR(50) NULL, -- Asumiendo 50 por el '5...'
    Observaciones TEXT NULL,
    Estado INT NOT NULL, -- Considera usar una tabla de estados o un ENUM si tu SGBD lo soporta
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP, -- Agregado por consistencia
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- Agregado por consistencia
    FOREIGN KEY (ProveedorId) REFERENCES tercero(Id),
    FOREIGN KEY (EmpleadoId) REFERENCES tercero(Id)
    -- Aquí podrías añadir restricciones para asegurar que ProveedorId y EmpleadoId
    -- correspondan a terceros con el TipoTerceroId adecuado si es necesario.
);

-- Tabla: detallecompra (Depende de compra, producto)
CREATE TABLE detallecompra (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    CompraId INT NOT NULL,
    ProductoId INT NOT NULL,
    Cantidad INT NOT NULL,
    Valor DECIMAL(12,2) NOT NULL, -- Podría ser el precio unitario o el total de la línea
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP, -- Agregado por consistencia
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- Agregado por consistencia
    FOREIGN KEY (CompraId) REFERENCES compra(Id),
    FOREIGN KEY (ProductoId) REFERENCES producto(Id),
    CHECK (Cantidad > 0) -- Asegurar que la cantidad sea positiva
);

-- Tabla: planpromocionalproducto (Depende de planpromocional, producto)
CREATE TABLE planpromocionalproducto (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    PlanPromocionalId INT NOT NULL,
    ProductoId INT NOT NULL,
    PrecioPromocional DECIMAL(12,2) NULL,
    PorcentajeDescuento DECIMAL(5,2) NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- Agregado por consistencia
    FOREIGN KEY (PlanPromocionalId) REFERENCES planpromocional(Id),
    FOREIGN KEY (ProductoId) REFERENCES producto(Id),
    UNIQUE INDEX UQ_planpromo_producto (PlanPromocionalId, ProductoId), -- Un producto solo debe estar una vez por plan
    CHECK (PrecioPromocional >= 0 OR PorcentajeDescuento BETWEEN 0 AND 100) -- Validaciones básicas
);