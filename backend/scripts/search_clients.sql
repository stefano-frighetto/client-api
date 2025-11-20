CREATE OR REPLACE FUNCTION search_clients(search_term VARCHAR)
RETURNS SETOF clientes AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM clientes
    WHERE unaccent(nombre) ILIKE '%' || unaccent(search_term) || '%';
END;
$$ LANGUAGE plpgsql;