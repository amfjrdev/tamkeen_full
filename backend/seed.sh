#!/bin/bash

BASE="http://localhost:5000/api"

ADMIN_TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhMTU4ZGViOC0wY2NkLTQ0ODQtYjBlNy0wMDYxMDdjZTAxNzIiLCJlbWFpbCI6ImFkbWluQHRlc3QuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiJhMTU4ZGViOC0wY2NkLTQ0ODQtYjBlNy0wMDYxMDdjZTAxNzIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZXN0LmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiZDZkMjBkYTAtNTZkNC00ZDI0LTg3OTgtYzcxNWUwZDUzZjRiIiwiZXhwIjoxNzc5MTUyNTI0LCJpc3MiOiJTUC5BUEkiLCJhdWQiOiJTUC5DbGllbnQifQ.nb8LjvyzRT6ZKy7vTGEx-FoUZZd6jk-kCu6k1G0CfJ0"

echo "=== Creating Categories ==="

CAT1=$(curl -s -X POST "$BASE/categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"name":"Plumbing","description":"Water pipes, leaks, installations and repairs"}')
echo "Plumbing: $CAT1"
CAT1_ID=$(echo $CAT1 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

CAT2=$(curl -s -X POST "$BASE/categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"name":"Electrical","description":"Wiring, installations, and electrical repairs"}')
echo "Electrical: $CAT2"
CAT2_ID=$(echo $CAT2 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

CAT3=$(curl -s -X POST "$BASE/categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"name":"Cleaning","description":"Home and office deep cleaning services"}')
echo "Cleaning: $CAT3"
CAT3_ID=$(echo $CAT3 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

CAT4=$(curl -s -X POST "$BASE/categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"name":"Painting","description":"Interior and exterior painting services"}')
echo "Painting: $CAT4"
CAT4_ID=$(echo $CAT4 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

CAT5=$(curl -s -X POST "$BASE/categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"name":"Carpentry","description":"Furniture, woodwork and custom carpentry"}')
echo "Carpentry: $CAT5"
CAT5_ID=$(echo $CAT5 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

echo ""
echo "Category IDs:"
echo "Plumbing:   $CAT1_ID"
echo "Electrical: $CAT2_ID"
echo "Cleaning:   $CAT3_ID"
echo "Painting:   $CAT4_ID"
echo "Carpentry:  $CAT5_ID"

echo ""
echo "=== Provider Tokens ==="

P1_TOKEN=$(curl -s -X POST "$BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"ahmed.plumber@test.com","password":"Provider123!"}' | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
echo "Ahmed token: ${P1_TOKEN:0:30}..."

P2_TOKEN=$(curl -s -X POST "$BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"sara.electrician@test.com","password":"Provider123!"}' | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
echo "Sara token: ${P2_TOKEN:0:30}..."

P3_TOKEN=$(curl -s -X POST "$BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"karim.cleaner@test.com","password":"Provider123!"}' | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
echo "Karim token: ${P3_TOKEN:0:30}..."

P4_TOKEN=$(curl -s -X POST "$BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"lina.painter@test.com","password":"Provider123!"}' | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
echo "Lina token: ${P4_TOKEN:0:30}..."

P5_TOKEN=$(curl -s -X POST "$BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"youcef.carpenter@test.com","password":"Provider123!"}' | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
echo "Youcef token: ${P5_TOKEN:0:30}..."

echo ""
echo "=== Creating Services ==="

echo "Ahmed - Plumbing services:"
curl -s -X POST "$BASE/services" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $P1_TOKEN" \
  -d "{\"categoryId\":\"$CAT1_ID\",\"name\":\"Pipe Leak Repair\",\"description\":\"Fast and reliable pipe leak detection and repair for homes and offices.\",\"price\":2500,\"durationMinutes\":90}"
echo ""

curl -s -X POST "$BASE/services" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $P1_TOKEN" \
  -d "{\"categoryId\":\"$CAT1_ID\",\"name\":\"Bathroom Installation\",\"description\":\"Full bathroom plumbing installation including sink, toilet and shower.\",\"price\":8000,\"durationMinutes\":240}"
echo ""

echo "Sara - Electrical services:"
curl -s -X POST "$BASE/services" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $P2_TOKEN" \
  -d "{\"categoryId\":\"$CAT2_ID\",\"name\":\"Electrical Wiring\",\"description\":\"Safe and certified home electrical wiring and rewiring services.\",\"price\":5000,\"durationMinutes\":180}"
echo ""

curl -s -X POST "$BASE/services" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $P2_TOKEN" \
  -d "{\"categoryId\":\"$CAT2_ID\",\"name\":\"Circuit Breaker Repair\",\"description\":\"Diagnosis and repair of faulty circuit breakers and electrical panels.\",\"price\":3000,\"durationMinutes\":120}"
echo ""

echo "Karim - Cleaning services:"
curl -s -X POST "$BASE/services" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $P3_TOKEN" \
  -d "{\"categoryId\":\"$CAT3_ID\",\"name\":\"Deep Home Cleaning\",\"description\":\"Thorough deep cleaning of your entire home including kitchen and bathrooms.\",\"price\":4500,\"durationMinutes\":300}"
echo ""

curl -s -X POST "$BASE/services" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $P3_TOKEN" \
  -d "{\"categoryId\":\"$CAT3_ID\",\"name\":\"Office Cleaning\",\"description\":\"Professional office cleaning service, daily or weekly packages available.\",\"price\":3500,\"durationMinutes\":180}"
echo ""

echo "Lina - Painting services:"
curl -s -X POST "$BASE/services" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $P4_TOKEN" \
  -d "{\"categoryId\":\"$CAT4_ID\",\"name\":\"Interior Wall Painting\",\"description\":\"High quality interior painting with premium paints and clean finish.\",\"price\":6000,\"durationMinutes\":360}"
echo ""

curl -s -X POST "$BASE/services" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $P4_TOKEN" \
  -d "{\"categoryId\":\"$CAT4_ID\",\"name\":\"Exterior House Painting\",\"description\":\"Weather-resistant exterior painting to protect and beautify your home.\",\"price\":12000,\"durationMinutes\":480}"
echo ""

echo "Youcef - Carpentry services:"
curl -s -X POST "$BASE/services" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $P5_TOKEN" \
  -d "{\"categoryId\":\"$CAT5_ID\",\"name\":\"Custom Furniture\",\"description\":\"Handcrafted custom furniture built to your exact specifications.\",\"price\":15000,\"durationMinutes\":480}"
echo ""

curl -s -X POST "$BASE/services" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $P5_TOKEN" \
  -d "{\"categoryId\":\"$CAT5_ID\",\"name\":\"Door & Window Repair\",\"description\":\"Repair and replacement of wooden doors, windows and frames.\",\"price\":3500,\"durationMinutes\":150}"
echo ""

echo ""
echo "=== Verifying: Fetching all services ==="
curl -s "$BASE/services" | grep -o '"name":"[^"]*"'

echo ""
echo "=== Verifying: Fetching all categories ==="
curl -s "$BASE/categories" | grep -o '"name":"[^"]*"'

echo ""
echo "=== SEEDING COMPLETE ==="
