-- Insert Countries
INSERT INTO "Countries" ("Name", "PhoneCode") VALUES 
('Colombia', '+57'),
('USA', '+1');

-- Insert Departments (Assuming IDs 1 and 2 for Countries based on insertion order)
-- Colombia (Id: 1)
INSERT INTO "Departments" ("Name", "CountryId") VALUES 
('Antioquia', 1),
('Cundinamarca', 1);

-- USA (Id: 2)
INSERT INTO "Departments" ("Name", "CountryId") VALUES 
('California', 2),
('Florida', 2);

-- Insert Municipalities
-- Antioquia (Id: 1)
INSERT INTO "Municipalities" ("Name", "DepartmentId") VALUES 
('Medellín', 1), 
('Envigado', 1);

-- Cundinamarca (Id: 2)
INSERT INTO "Municipalities" ("Name", "DepartmentId") VALUES 
('Bogotá', 2), 
('Soacha', 2);

-- California (Id: 3)
INSERT INTO "Municipalities" ("Name", "DepartmentId") VALUES 
('Los Angeles', 3), 
('San Francisco', 3);

-- Florida (Id: 4)
INSERT INTO "Municipalities" ("Name", "DepartmentId") VALUES 
('Miami', 4), 
('Orlando', 4);