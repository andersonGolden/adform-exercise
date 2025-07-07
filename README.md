# adform-exercise
order management sql exercise for Adform recruitment process

### Prerequisites
In order to run this project, you will need the following tools installed
- Docker and Docker Compose
- .NET 8 hosting bundle 
- sql scritps for schema creation and data seeding are in the queries.sql file

### 1. Start the Database
```bash
docker-compose up -d postgres
```

This will:
- Start PostgreSQL 15 on port 5432
- Create the database schema automatically &
- Seed dummy data with 10,000 customers, 8,000 products, and 100,000 orders

### 2. Test the Database
Connect to the database and run queries:
```bash
# Connect to PostgreSQL
docker exec -it order-management-db psql -U admin -d order_management

# Run sample queries
\i queries.sql
```

### 3. Run the API 
```bash
cd OrderManagement.Api
dotnet run
```

The API will be available at:
- Swagger UI: http://localhost:63912

## Database Schema

### Tables
- **customers**: Customer information with JSON details as instructed
- **products**: Product catalog with name, category, and price
- **orders**: Customer orders with creation timestamp
- **order_items**: Order line items with many-to-many relationship

### Performance Indexes
- `orders.customer_id`
- `products.name`, `products.category`
- `customers.details->>'city'` (added a GIN index for better JSON search)
- `order_items.order_id`, `order_items.product_id`
- `orders.created_at`

## API Endpoints

### Invoice Generation
```
GET /api/OrderManagement/invoice/{orderId}?search={searchTerm}
```
Returns invoice details for a specific order with optional parameter for product/category search.

**Example:**
```bash
curl "http://localhost:63912/api/OrderManagement/invoice/3?search=laptop"
```

### Orders Distribution Report
```
GET /api/OrderManagement/ordersdistributionreport?city={cityFilter}&sort={sortDirection}
```

Returns order statistics by city with optional filtering and sorting options.

**Examples:**
```bash
# All cities, sorted by orders DESC (desc is the defaul sorting order)
curl "http://localhost:63912/api/OrderManagement/ordersdistributionreport"

# Filter by city
curl "http://localhost:63912/api/OrderManagement/ordersdistributionreport?city=Vilnius"

# Sort by ascending order
curl "http://localhost:63912/api/OrderManagement/ordersdistributionreport?sort=asc"
```

## Testing

### Run API Tests
```bash
cd OrderManagement.Api.Tests
dotnet test
```

### Database Performance Tests
```bash
# Connect to database and run performance queries
docker exec -it order-management-db psql -U admin -d order_management -f queries.sql
```

## Data Insights

The system includes:
- **10,000 customers** with realistic names and locations
- **8,000 products** across 10 categories
- **100,000 orders** with 1-100 products each
- **Realistic data distribution** across Lithuanian cities

### Development & Project Structure
```
 adform-exercise/
├── docker-compose.yml          # Database setup with docker
├── tableSchema.sql             # Database schema
├── dataSeeder.sql              # Mock data generation
├── queries.sql                 # SQL queries and tests
├── OrderManagement.Api/        # .NET 8 REST API
|   ├── Controllers            # API controllers
│   ├── Program.cs             # API setup
│   ├── Models/                # Data models
│   ├── Services/              # Order management service with data access
│   └── appsettings.json       # App Configuration
├── OrderManagement.Api.Tests/  # Integration tests
└── README.md                  # This file
```

### Key Features
-  **PostgreSQL** with optimized indexes
-  **Bulk data generation** with performance optimizations
-  **JSON support** for flexible customer details
-  **Text search** on product names and categories
-  **City-based filtering** and sorting
-  **Docker setup** for easy deployment
-  **Swagger UI** for API documentation
-  **Integration tests** for all endpoints

##  Configuration

### Database Connection
Default connection string (configurable in `appsettings.json`):
```
Host=localhost;Database=order_management;Username=admin;Password=password123;Port=5432
```

### Docker Services
- **PostgreSQL**: Port 5432

## Sample Queries

### Get Invoice for Order 1
```sql

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
    WHERE o.id = 1
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
WHERE p.name ILIKE '%' || 'storage' || '%' 
   OR p.category ILIKE '%' || 'storage' || '%'
ORDER BY p.name;
```

### Get Orders Distribution by City
```sql
WITH city_stats AS (
    SELECT 
        c.details->>'city' as customer_city,
        COUNT(DISTINCT o.id) as number_of_orders,
        SUM(oi.quantity * p.price) as total_amount
    FROM customers c
    JOIN orders o ON c.id = o.customer_id
    JOIN order_items oi ON o.id = oi.order_id
    JOIN products p ON oi.product_id = p.id
    WHERE c.details->>'city' ILIKE '%' || 'Kaunas' || '%'
    GROUP BY c.details->>'city'
)
SELECT 
    customer_city,
    number_of_orders,
    ROUND(total_amount::NUMERIC, 2) as total_amount_usd
FROM city_stats
ORDER BY 
    CASE WHEN 'desc' = 'desc' THEN number_of_orders END DESC,
    CASE WHEN 'asc' = 'asc' THEN number_of_orders END ASC;
```


