-- 1. Asegurar que la columna Observaciones exista (necesaria para el estado del carrito)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'Observaciones' AND Object_ID = Object_ID(N'Venta'))
BEGIN
    ALTER TABLE [dbo].[Venta]
    ADD Observaciones NVARCHAR(255) NULL;
END

ALTER TABLE [dbo].[Venta]
ALTER COLUMN MetodoPago NVARCHAR(50) NULL;