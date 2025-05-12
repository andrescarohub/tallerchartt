-- Inserts para tablas sin dependencias externas (o dependencias ya insertadas)

-- 1. tipotercero
INSERT INTO tipotercero (Nombre, Descripcion) VALUES
('Cliente', 'Persona o empresa que realiza compras'),
('Proveedor', 'Persona o empresa que abastece productos'),
('Empleado', 'Persona que trabaja en la empresa');
-- IDs generados: 1, 2, 3

-- 2. tipodocumento
INSERT INTO tipodocumento (Nombre, Descripcion) VALUES
('Cédula de Ciudadanía', 'Documento Nacional de Identidad Colombia'),
('NIT', 'Número de Identificación Tributaria'),
('Pasaporte', 'Documento de viaje internacional'),
('Cédula Extranjería', 'Documento para extranjeros residentes');
-- IDs generados: 1, 2, 3, 4

-- 3. ciudad
INSERT INTO ciudad (Nombre) VALUES
('Bogotá D.C.'),
('Medellín'),
('Cali'),
('Barranquilla');
-- IDs generados: 1, 2, 3, 4

-- 4. categoria
INSERT INTO categoria (Nombre, Descripcion) VALUES
('Portátiles', 'Computadores portátiles y ultrabooks'),
('Monitores', 'Pantallas para computador'),
('Accesorios', 'Teclados, mouses, cámaras web, etc.'),
('Software', 'Licencias de sistemas operativos y aplicaciones');
-- IDs generados: 1, 2, 3, 4

-- 5. planpromocional
INSERT INTO planpromocional (Nombre, FechaInicio, FechaFin, Descripcion, Activo) VALUES
('Promo Aniversario', '2024-07-01 00:00:00', '2024-07-31 23:59:59', 'Descuentos especiales por aniversario', 1),
('Vuelta al Cole', '2024-08-15 00:00:00', '2024-09-15 23:59:59', 'Ofertas en equipos para estudiantes', 1),
('Black Friday Anticipado', '2023-11-01 00:00:00', '2023-11-15 23:59:59', 'Promoción pasada', 0);
-- IDs generados: 1, 2, 3

-- Inserts para tablas con dependencias

-- 6. tercero (Depende de tipotercero, tipodocumento, ciudad)
INSERT INTO tercero (Nombre, Apellido, Email, NumeroDocumento, TipoDocumentoId, TipoTerceroId, CiudadId) VALUES
('Juan', 'Perez', 'juan.perez@email.com', '10203040', 1, 1, 1), -- Cliente (TipoTerceroId=1), CC (TipoDocumentoId=1), Bogotá (CiudadId=1)
('Maria', 'Lopez', 'maria.lopez@email.com', '50607080', 1, 1, 2), -- Cliente (TipoTerceroId=1), CC (TipoDocumentoId=1), Medellín (CiudadId=2)
('Tech Supplies S.A.S', NULL, 'ventas@techsupplies.com', '900123456-7', 2, 2, 1), -- Proveedor (TipoTerceroId=2), NIT (TipoDocumentoId=2), Bogotá (CiudadId=1)
('Carlos', 'Ramirez', 'carlos.ramirez@miempresa.com', '79809010', 1, 3, 1); -- Empleado (TipoTerceroId=3), CC (TipoDocumentoId=1), Bogotá (CiudadId=1)
-- IDs generados: 1, 2, 3, 4

-- 7. producto (Depende de categoria)
INSERT INTO producto (Nombre, StockActual, StockMinimo, StockMaximo, Barcode, PrecioUnitario, CategoriaId) VALUES
('Laptop Core i5 14"', 50, 10, 100, '7701112223331', 2500000.00, 1), -- CategoriaId=1 (Portátiles)
('Monitor Curvo 27"', 30, 5, 50, '7701112223332', 1200000.00, 2), -- CategoriaId=2 (Monitores)
('Teclado Mecánico RGB', 100, 20, 200, '7701112223333', 350000.00, 3), -- CategoriaId=3 (Accesorios)
('Mouse Inalámbrico Ergo', 150, 30, 300, '7701112223334', 120000.00, 3), -- CategoriaId=3 (Accesorios)
('Licencia Windows 11 Pro', 1000, 0, 0, NULL, 600000.00, 4); -- CategoriaId=4 (Software)
-- IDs generados: 1, 2, 3, 4, 5

-- 8. compra (Depende de tercero - Proveedor y Empleado)
INSERT INTO compra (ProveedorId, EmpleadoId, Fecha, NumeroFactura, Observaciones, Estado) VALUES
(3, 4, '2024-05-10 14:00:00', 'FSUP-00123', 'Pedido de reposición stock portátiles y teclados', 1), -- ProveedorId=3, EmpleadoId=4, Estado=1 (Recibido/Pendiente/etc.)
(3, 4, '2024-05-20 10:30:00', 'FSUP-00145', 'Compra monitores', 2); -- ProveedorId=3, EmpleadoId=4, Estado=2 (Completado/Pagado/etc.)
-- IDs generados: 1, 2

-- 9. detallecompra (Depende de compra, producto)
-- Detalles de la compra 1
INSERT INTO detallecompra (CompraId, ProductoId, Cantidad, Valor) VALUES
(1, 1, 20, 2350000.00), -- CompraId=1, ProductoId=1 (Laptop)
(1, 3, 50, 310000.00);  -- CompraId=1, ProductoId=3 (Teclado)
-- Detalles de la compra 2
INSERT INTO detallecompra (CompraId, ProductoId, Cantidad, Valor) VALUES
(2, 2, 15, 1150000.00); -- CompraId=2, ProductoId=2 (Monitor)
-- IDs generados: 1, 2, 3

-- 10. planpromocionalproducto (Depende de planpromocional, producto)
-- Productos en la "Promo Aniversario" (PlanPromocionalId=1)
INSERT INTO planpromocionalproducto (PlanPromocionalId, ProductoId, PorcentajeDescuento) VALUES
(1, 1, 10.00), -- PlanId=1, ProductoId=1 (Laptop) con 10% descuento
(1, 3, 15.00); -- PlanId=1, ProductoId=3 (Teclado) con 15% descuento
-- Productos en "Vuelta al Cole" (PlanPromocionalId=2)
INSERT INTO planpromocionalproducto (PlanPromocionalId, ProductoId, PrecioPromocional) VALUES
(2, 1, 2400000.00), -- PlanId=2, ProductoId=1 (Laptop) con precio fijo promocional
(2, 4, 100000.00);  -- PlanId=2, ProductoId=4 (Mouse) con precio fijo promocional
-- IDs generados: 1, 2, 3, 4