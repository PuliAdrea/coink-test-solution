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