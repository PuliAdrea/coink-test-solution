CREATE OR REPLACE FUNCTION sp_RegisterUser(
    _Name VARCHAR,
    _Phone VARCHAR,
    _Address VARCHAR,
    _MunicipalityId INT
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    _NewId INT;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Municipalities" WHERE "Id" = _MunicipalityId) THEN
        RAISE EXCEPTION 'Invalid MunicipalityId: % does not exist.', _MunicipalityId;
    END IF;

    INSERT INTO "Users" (
        "Name", 
        "Phone", 
        "Address", 
        "MunicipalityId"
    )
    VALUES (
        _Name, 
        _Phone, 
        _Address, 
        _MunicipalityId
    )
    RETURNING "Id" INTO _NewId;

    RETURN _NewId;
END;
$$;

-- 
CREATE OR REPLACE FUNCTION sp_GetAllUsers()
RETURNS TABLE (
    Id INT,
    Name VARCHAR,
    Phone VARCHAR,
    Address VARCHAR,
    MunicipalityId INT,
    MunicipalityName VARCHAR,
    DepartmentName VARCHAR,
    CountryName VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u."Id", u."Name", u."Phone", u."Address", u."MunicipalityId",
        m."Name", d."Name", c."Name"
    FROM "Users" u
    JOIN "Municipalities" m ON u."MunicipalityId" = m."Id"
    JOIN "Departments" d ON m."DepartmentId" = d."Id"
    JOIN "Countries" c ON d."CountryId" = c."Id";
END;
$$ LANGUAGE plpgsql;

-- 
CREATE OR REPLACE FUNCTION sp_GetUserById(_Id INT)
RETURNS TABLE (
    Id INT,
    Name VARCHAR,
    Phone VARCHAR,
    Address VARCHAR,
    MunicipalityId INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT u."Id", u."Name", u."Phone", u."Address", u."MunicipalityId"
    FROM "Users" u
    WHERE u."Id" = _Id;
END;
$$ LANGUAGE plpgsql;

-- 
CREATE OR REPLACE FUNCTION sp_UpdateUser(
    _Id INT,
    _Name VARCHAR,
    _Phone VARCHAR,
    _Address VARCHAR,
    _MunicipalityId INT
) RETURNS VOID AS $$
BEGIN
    UPDATE "Users"
    SET "Name" = _Name,
        "Phone" = _Phone,
        "Address" = _Address,
        "MunicipalityId" = _MunicipalityId
    WHERE "Id" = _Id;
END;
$$ LANGUAGE plpgsql;

-- 
CREATE OR REPLACE FUNCTION sp_DeleteUser(_Id INT) 
RETURNS VOID AS $$
BEGIN
    DELETE FROM "Users" WHERE "Id" = _Id;
END;
$$ LANGUAGE plpgsql;