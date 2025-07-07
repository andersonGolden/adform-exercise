-- Seed Data for Order Management System

-- Sample data arrays for generation
DO $$
DECLARE
    first_names TEXT[] := ARRAY['John', 'Jane', 'Michael', 'Sarah', 'David', 'Emily', 'Robert', 'Lisa', 'James', 'Maria', 'William', 'Jennifer', 'Richard', 'Linda', 'Thomas', 'Patricia', 'Christopher', 'Barbara', 'Daniel', 'Elizabeth'];
    last_names TEXT[] := ARRAY['Smith', 'Johnson', 'Williams', 'Brown', 'Jones', 'Garcia', 'Miller', 'Davis', 'Rodriguez', 'Martinez', 'Hernandez', 'Lopez', 'Gonzalez', 'Wilson', 'Anderson', 'Thomas', 'Taylor', 'Moore', 'Jackson', 'Martin'];
    cities TEXT[] := ARRAY['Vilnius', 'Kaunas', 'Klaipeda', 'Siauliai', 'Panevezys', 'Alytus', 'Marijampole', 'Mazeikiai', 'Jonava', 'Utena', 'Kedainiai', 'Telsiai', 'Visaginas', 'Taurage', 'Ukmerge', 'Plunge', 'Kretinga', 'Silute', 'Radviliskis', 'Palanga'];
    countries TEXT[] := ARRAY['Lithuania', 'Latvia', 'Estonia', 'Poland', 'Germany', 'Sweden', 'Denmark', 'Finland', 'Norway', 'Netherlands'];
    product_names TEXT[] := ARRAY['Laptop', 'Smartphone', 'Tablet', 'Headphones', 'Mouse', 'Keyboard', 'Monitor', 'Printer', 'Scanner', 'Webcam', 'Microphone', 'Speaker', 'Router', 'Switch', 'Cable', 'Adapter', 'Charger', 'Battery', 'Case', 'Stand'];
    categories TEXT[] := ARRAY['Electronics', 'Computers', 'Mobile', 'Accessories', 'Audio', 'Networking', 'Storage', 'Gaming', 'Office', 'Home'];
    i INTEGER;
    j INTEGER;
    customer_id INTEGER;
    product_id INTEGER;
    order_id INTEGER;
    quantity INTEGER;
    random_date TIMESTAMP;
BEGIN
    -- Generate 10,000 customers
    FOR i IN 1..10000 LOOP
        INSERT INTO customers (first_name, last_name, email, details)
        VALUES (
            first_names[1 + (i % array_length(first_names, 1))],
            last_names[1 + (i % array_length(last_names, 1))],
            'customer' || i || '@example.com',
            jsonb_build_object(
                'country', countries[1 + (i % array_length(countries, 1))],
                'city', cities[1 + (i % array_length(cities, 1))],
                'zip_code', 10000 + (i % 90000),
                'phone', '+370' || (60000000 + (i % 9999999))
            )
        );
    END LOOP;

    -- Generate 8,000 products
    FOR i IN 1..8000 LOOP
        INSERT INTO products (name, category, price)
        VALUES (
            product_names[1 + (i % array_length(product_names, 1))] || ' ' || i,
            categories[1 + (i % array_length(categories, 1))],
            (10 + (i % 990))::NUMERIC(10,2)
        );
    END LOOP;

    -- Generate 100,000 orders with 1-100 products each
    FOR i IN 1..100000 LOOP
        -- Select random customer
        customer_id := 1 + (i % 10000);
        
        -- Generate random date within last 2 years
        random_date := CURRENT_TIMESTAMP - INTERVAL '1 day' * (i % 730);
        
        INSERT INTO orders (customer_id, created_at)
        VALUES (customer_id, random_date);
        
        order_id := i;
        
        -- Add 1-100 unique products to each order. need to track used product ids to avoid duplicates that violate unique constraint
        DECLARE
            used_product_ids INTEGER[] := ARRAY[]::INTEGER[];
            num_products INTEGER := 1 + (i % 100);
            try_count INTEGER;
        BEGIN
            FOR j IN 1..num_products LOOP
                try_count := 0;
                LOOP
                    -- Generate a candidate product_id
                    IF j <= 20 THEN
                        product_id := 1 + ((i + j) % 8000);
                    ELSE
                        product_id := 1 + (floor(random() * 8000))::INTEGER;
                    END IF;
                    -- Check uniqueness
                    IF NOT product_id = ANY(used_product_ids) THEN
                        used_product_ids := array_append(used_product_ids, product_id);
                        quantity := 1 + (j % 50);
                        INSERT INTO order_items (order_id, product_id, quantity)
                        VALUES (order_id, product_id, quantity);
                        EXIT;
                    END IF;
                    try_count := try_count + 1;
                    IF try_count > 10 THEN
                        -- Give up after 10 tries to avoid infinite loop
                        EXIT;
                    END IF;
                END LOOP;
            END LOOP;
        END;
    END LOOP;
END $$;

