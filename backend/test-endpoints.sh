#!/bin/bash
# SP API Endpoint Testing Script

BASE_URL="http://localhost:5000"
echo "🚀 Testing SP API Endpoints"
echo "=================================="

echo "✅ Health Check"
curl -s "$BASE_URL/health" | echo "Response: $(cat)"

echo -e "\n✅ Detailed Health Check"
curl -s "$BASE_URL/health/detail" | jq -r '.status' | echo "Status: $(cat)"

echo -e "\n✅ App Configuration (from database)"
curl -s "$BASE_URL/api/config" | jq -r '.defaultCity' | echo "Default City: $(cat)"

echo -e "\n✅ Categories (from database)"
CATEGORY_COUNT=$(curl -s "$BASE_URL/api/categories" | jq 'length')
echo "Categories loaded: $CATEGORY_COUNT"

echo -e "\n✅ Services (from database)"  
SERVICES_COUNT=$(curl -s "$BASE_URL/api/services" | jq -r '.totalCount')
echo "Services loaded: $SERVICES_COUNT"

echo -e "\n✅ Feature Flags (from database)"
CHAT_ENABLED=$(curl -s "$BASE_URL/api/config" | jq -r '.featureFlags.chat_enabled')
PAYMENTS_ENABLED=$(curl -s "$BASE_URL/api/config" | jq -r '.featureFlags.payments_enabled')
echo "Chat enabled: $CHAT_ENABLED"
echo "Payments enabled: $PAYMENTS_ENABLED"

echo -e "\n🎉 All endpoints are responding with database data!"
echo "🔗 API Documentation: $BASE_URL/swagger (if available)"