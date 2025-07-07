-- Order Management System SQL Queries

-- ============================================================================
-- INVOICE QUERY
-- ============================================================================
-- For a given order ID, return product details with total amount
-- Supports partial text search on product name and category

-- this is my base query for the invoice report
WITH order_summary AS (
    SELECT 
        o.id as order_id,
        o.created_at,
        c.first_name,
        c.last_name,
        c.email,
        c.details->>'city' as customer_city,
        c.details->>'country' as customer_country
    FROM orders o
    JOIN customers c ON o.customer_id = c.id
    WHERE o.id = :order_id
)
SELECT 
    os.order_id,
    os.created_at,
    os.first_name || ' ' || os.last_name as customer_name,
    os.email as customer_email,
    os.customer_city,
    os.customer_country,
    p.name as product_name,
    p.category as product_category,
    oi.quantity,
    p.price as unit_price,
    (oi.quantity * p.price) as line_total,
    SUM(oi.quantity * p.price) OVER (PARTITION BY os.order_id) as order_total
FROM order_summary os
JOIN order_items oi ON os.order_id = oi.order_id
JOIN products p ON oi.product_id = p.id
WHERE p.name ILIKE '%' || :search_term || '%' 
   OR p.category ILIKE '%' || :search_term || '%'
ORDER BY p.name;

-- I'm going to save this query as a function for ease of use
-- ============================================================================
-- Function name: get_order_invoice(order_id integer, search_term text)
-- ============================================================================
-- Returns invoice details for a given order, optionally filtered by search_term for product name or category

CREATE OR REPLACE FUNCTION get_order_invoice(
    p_order_id int8,
    p_search_term text DEFAULT NULL
)
RETURNS TABLE (
    order_id int8,
    created_at timestamp,
    customer_name text,
    customer_email text,
    customer_city text,
    customer_country text,
    product_name text,
    product_category text,
    quantity integer,
    unit_price numeric,
    line_total numeric,
    order_total numeric
) AS $$
BEGIN
    RETURN QUERY
    WITH order_summary AS (
        SELECT 
            o.id as order_id,
            o.created_at,
            c.first_name,
            c.last_name,
            c.email,
            c.details->>'city' as customer_city,
            c.details->>'country' as customer_country
        FROM orders o
        JOIN customers c ON o.customer_id = c.id
        WHERE o.id = p_order_id
    )
    SELECT 
        os.order_id,
        os.created_at,
        os.first_name || ' ' || os.last_name as customer_name,
        os.email as customer_email,
        os.customer_city,
        os.customer_country,
        p.name as product_name,
        p.category as product_category,
        oi.quantity,
        p.price as unit_price,
        (oi.quantity * p.price) as line_total,
        SUM(oi.quantity * p.price) OVER (PARTITION BY os.order_id) as order_total
    FROM order_summary os
    JOIN order_items oi ON os.order_id = oi.order_id
    JOIN products p ON oi.product_id = p.id
    WHERE (
        p_search_term IS NULL OR trim(p_search_term) = ''
        OR p.name ILIKE '%' || p_search_term || '%'
        OR p.category ILIKE '%' || p_search_term || '%'
    )
    ORDER BY p.name;
END;
$$ LANGUAGE plpgsql STABLE;

-- ============================================================================
-- ORDERS DISTRIBUTION REPORT
-- ============================================================================
-- Show customer city, number of orders, total amount
-- Supports filtering by city and ordering by number of orders

-- This is my base query for order distribution report
WITH city_stats AS (
    SELECT 
        c.details->>'city' as customer_city,
        COUNT(DISTINCT o.id) as number_of_orders,
        SUM(oi.quantity * p.price) as total_amount
    FROM customers c
    JOIN orders o ON c.id = o.customer_id
    JOIN order_items oi ON o.id = oi.order_id
    JOIN products p ON oi.product_id = p.id
    WHERE c.details->>'city' ILIKE '%' || :city_filter || '%'
    GROUP BY c.details->>'city'
)
SELECT 
    customer_city,
    number_of_orders,
    ROUND(total_amount::NUMERIC, 2) as total_amount_usd
FROM city_stats
ORDER BY 
    CASE WHEN :sort_direction = 'desc' THEN number_of_orders END DESC,
    CASE WHEN :sort_direction = 'asc' THEN number_of_orders END ASC;

-- I'm going to save this query as a function for ease of use
-- ============================================================================
-- Function name: get_orders_distribution_report(city_filter text, sort_direction text)
-- ============================================================================
-- Returns city, number of orders, and total amount, with optional filtering and sorting

CREATE OR REPLACE FUNCTION get_orders_distribution_report(
    p_city_filter text DEFAULT NULL,
    p_sort_direction text DEFAULT 'desc'
)
RETURNS TABLE (
    customer_city text,
    number_of_orders int8,
    total_amount_usd numeric
) AS $$
BEGIN
    RETURN QUERY
    WITH city_stats AS (
        SELECT 
            c.details->>'city' as city,
            COUNT(DISTINCT o.id) as num_of_orders,
            SUM(oi.quantity * p.price) as total_amount
        FROM customers c
        JOIN orders o ON c.id = o.customer_id
        JOIN order_items oi ON o.id = oi.order_id
        JOIN products p ON oi.product_id = p.id
        WHERE (
            p_city_filter IS NULL OR trim(p_city_filter) = ''
            OR c.details->>'city' ILIKE '%' || p_city_filter || '%'
        )
        GROUP BY c.details->>'city'
    )
    SELECT 
        city AS customer_city,
        num_of_orders AS number_of_orders,
        ROUND(total_amount::NUMERIC, 2) as total_amount_usd
    FROM city_stats
    ORDER BY 
        CASE WHEN lower(p_sort_direction) = 'asc' THEN number_of_orders END ASC,
        CASE WHEN lower(p_sort_direction) <> 'asc' THEN number_of_orders END DESC;
END;
$$ LANGUAGE plpgsql STABLE;



