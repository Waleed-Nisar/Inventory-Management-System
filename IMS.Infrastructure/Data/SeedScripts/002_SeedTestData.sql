-- Seed Categories
SET IDENTITY_INSERT Categories ON

INSERT INTO Categories (Id, Name, Description, IsActive, CreatedDate) VALUES
(1, 'Electronics', 'Electronic devices and accessories', 1, GETUTCDATE()),
(2, 'Furniture', 'Office and home furniture', 1, GETUTCDATE()),
(3, 'Stationery', 'Office supplies and stationery', 1, GETUTCDATE()),
(4, 'Hardware', 'Tools and hardware items', 1, GETUTCDATE()),
(5, 'Software', 'Software licenses and subscriptions', 1, GETUTCDATE())

SET IDENTITY_INSERT Categories OFF

-- Seed Suppliers
SET IDENTITY_INSERT Suppliers ON

INSERT INTO Suppliers (Id, Name, ContactPerson, Phone, Email, Address, IsActive, CreatedDate) VALUES
(1, 'Tech Supplies Co.', 'John Smith', '555-0101', 'contact@techsupplies.com', '123 Tech Street, Silicon Valley, CA', 1, GETUTCDATE()),
(2, 'Office Depot Inc.', 'Sarah Johnson', '555-0102', 'sales@officedepot.com', '456 Commerce Blvd, New York, NY', 1, GETUTCDATE()),
(3, 'Hardware Hub', 'Mike Wilson', '555-0103', 'info@hardwarehub.com', '789 Industrial Park, Chicago, IL', 1, GETUTCDATE())

SET IDENTITY_INSERT Suppliers OFF

-- Seed Products (Sample selection)
SET IDENTITY_INSERT Products ON

INSERT INTO Products (Id, Name, SKU, Description, UnitPrice, CurrentStock, MinimumStock, CategoryId, SupplierId, IsActive, CreatedDate) VALUES
(1, 'Wireless Mouse', 'ELEC-001', 'Ergonomic wireless mouse', 29.99, 45, 10, 1, 1, 1, GETUTCDATE()),
(2, 'USB Keyboard', 'ELEC-002', 'Standard USB keyboard', 39.99, 30, 10, 1, 1, 1, GETUTCDATE()),
(3, 'Office Chair', 'FURN-001', 'Ergonomic office chair with lumbar support', 249.99, 12, 5, 2, 2, 1, GETUTCDATE()),
(4, 'A4 Paper Ream', 'STAT-001', '500 sheets premium A4 paper', 8.99, 120, 30, 3, 2, 1, GETUTCDATE()),
(5, 'USB-C Cable', 'ELEC-004', 'High-speed USB-C cable 2m', 12.99, 8, 15, 1, 1, 1, GETUTCDATE())

SET IDENTITY_INSERT Products OFF

PRINT 'Test data seeded successfully'
PRINT 'WARNING: This script should ONLY be used in Development/Testing environments'
GO