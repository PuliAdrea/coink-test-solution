-- 1. Create Parametric Table: Countries
CREATE TABLE "Countries" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "PhoneCode" VARCHAR(10) NOT NULL,
    CONSTRAINT "UQ_Countries_Name" UNIQUE ("Name")
);

COMMENT ON TABLE "Countries" IS 'Top level administrative division.';

-- 2. Create Parametric Table: Departments (States/Regions)
CREATE TABLE "Departments" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "CountryId" INT NOT NULL,
    CONSTRAINT "FK_Departments_Countries" 
        FOREIGN KEY ("CountryId") 
        REFERENCES "Countries" ("Id") 
        ON DELETE CASCADE,
    CONSTRAINT "UQ_Departments_Name_Country" UNIQUE ("Name", "CountryId") 
);

CREATE INDEX "IX_Departments_CountryId" ON "Departments" ("CountryId"); 

-- 3. Create Parametric Table: Municipalities (Cities)
CREATE TABLE "Municipalities" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "DepartmentId" INT NOT NULL,
    CONSTRAINT "FK_Municipalities_Departments" 
        FOREIGN KEY ("DepartmentId") 
        REFERENCES "Departments" ("Id") 
        ON DELETE CASCADE
);

CREATE INDEX "IX_Municipalities_DepartmentId" ON "Municipalities" ("DepartmentId");

-- 4. Create Core Table: Users
CREATE TABLE "Users" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(150) NOT NULL,
    "Phone" VARCHAR(20) NOT NULL,
    "Address" VARCHAR(250) NOT NULL,
    "MunicipalityId" INT NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Users_Municipalities" 
        FOREIGN KEY ("MunicipalityId") 
        REFERENCES "Municipalities" ("Id")
        ON DELETE RESTRICT 
);

CREATE INDEX "IX_Users_MunicipalityId" ON "Users" ("MunicipalityId");