# Azure Cognitive Search Setup Guide

## 🚀 **Quick Start for CEO Demo**

### **⚠️ Important: Use Azure AI Search (Core Service), NOT Marketplace Apps**

When you search for search services in Azure, you'll see several options. **Use the core Microsoft service**:

- ✅ **"Azure AI Search"** - Microsoft's core search service (what we want)
- ❌ **"Azure Cognitive Search solution for Sentinel"** - For security logs only
- ❌ **"Evoke Workspace Search"** - Third-party solution
- ❌ **"BIG Search For Documents"** - Third-party solution
- ❌ **"SearchBlox Enterprise Search"** - VM-based solution

**Why Azure AI Search (Core Service)?**

- Microsoft's flagship search service
- Enterprise-grade reliability
- Perfect integration with your .NET API
- Professional for CEO demo

### **Step 1: Create Azure AI Search Service (Core Microsoft Service)**

#### **Using Azure Portal:**

1. Go to [Azure Portal](https://portal.azure.com)
2. Click **"Create a resource"**
3. Search for **"Azure AI Search"** (NOT the marketplace apps)
4. Click **"Create"**
5. Fill in the details:
   - **Resource Group**: Create new or use existing
   - **Service Name**: `irs-tax-search` (must be globally unique)
   - **Location**: Choose your preferred region
   - **Pricing Tier**: **Standard S1** (recommended for demo)
6. Click **"Review + create"** then **"Create"**

> **⚠️ Important**: Make sure you select **"Azure AI Search"** (the core Microsoft service), NOT any of the marketplace applications like "Azure Cognitive Search solution for Sentinel" or third-party apps.

#### **Using Azure CLI:**

```bash
# Create resource group (if needed)
az group create --name irs-tax-rg --location eastus

# Create Cognitive Search service
az search service create \
  --name irs-tax-search \
  --resource-group irs-tax-rg \
  --sku Standard \
  --partition-count 1 \
  --replica-count 1
```

### **Step 2: Get Your Search Service Details**

1. Go to your Cognitive Search service in Azure Portal
2. Copy the **Endpoint URL** (e.g., `https://irs-tax-search.search.windows.net`)
3. Go to "Keys" and copy the **Primary admin key**

### **Step 3: Update Your API Configuration**

Update `appsettings.json`:

```json
{
  "AzureSearch": {
    "Endpoint": "https://irs-tax-search.search.windows.net",
    "Key": "your-primary-admin-key-here",
    "IndexName": "tax-data-index"
  }
}
```

### **Step 4: Initialize the Search Index**

#### **Option A: Using API Endpoints (Recommended)**

```bash
# 1. Create the search index
curl -X POST "https://localhost:7000/api/v1/searchindex/create" \
  -H "Content-Type: application/json"

# 2. Index your tax data
curl -X POST "https://localhost:7000/api/v1/searchindex/index-data" \
  -H "Content-Type: application/json"

# 3. Check document count
curl -X GET "https://localhost:7000/api/v1/searchindex/count"
```

#### **Option B: Using Swagger UI**

1. Run your API: `dotnet run`
2. Go to `https://localhost:7000` (Swagger UI)
3. Use the `/api/v1/searchindex` endpoints

### **Step 5: Test Your Search**

#### **Test Query Examples:**

```bash
# Test basic search
curl -X POST "https://localhost:7000/api/v1/chat/process-query" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "What was my total income last year?",
    "sessionId": "00000000-0000-0000-0000-000000000000"
  }'

# Test property search
curl -X POST "https://localhost:7000/api/v1/chat/process-query" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Show me all my properties and their current values",
    "sessionId": "00000000-0000-0000-0000-000000000000"
  }'
```

---

## 🎯 **CEO Demo Script**

### **1. Show the Architecture:**

- **"We're using Azure Cognitive Search - Microsoft's enterprise-grade AI search service"**
- **"Same technology used by Netflix, Microsoft, and Fortune 500 companies"**
- **"Built-in semantic search and AI capabilities"**

### **2. Demonstrate Search Capabilities:**

- **Natural language queries**: "What was my total income last year?"
- **Complex queries**: "Show me all my rental properties and their income"
- **Tax analysis**: "Compare my tax liability across different years"

### **3. Highlight Enterprise Features:**

- **Security**: Row-level security built-in
- **Scalability**: Handles millions of documents
- **AI-powered**: Semantic understanding of tax concepts
- **Real-time**: Data stays current

---

## 🔧 **Troubleshooting**

### **Common Issues:**

#### **1. Index Creation Fails**

- Check your Azure Search service is running
- Verify your endpoint and key are correct
- Ensure you have admin permissions

#### **2. No Search Results**

- Run the index-data endpoint to populate the index
- Check if your database has tax data
- Verify the search index exists

#### **3. Authentication Errors**

- Double-check your search key
- Ensure the key has admin permissions
- Verify the endpoint URL is correct

### **Debug Commands:**

```bash
# Check if index exists
curl -X GET "https://localhost:7000/api/v1/searchindex/exists"

# Get document count
curl -X GET "https://localhost:7000/api/v1/searchindex/count"

# Check API health
curl -X GET "https://localhost:7000/health"
```

---

## 💰 **Cost Estimation**

### **For CEO Demo:**

- **Azure Cognitive Search S1**: ~$250/month
- **Storage**: ~$0.10/GB/month
- **Queries**: ~$0.50 per 1,000 queries

### **Total Monthly Cost**: ~$300-400 for demo purposes

---

## 🚀 **Next Steps After Demo**

### **Phase 1: Production Setup**

1. **Upgrade to S2/S3 tier** for production
2. **Add Azure Key Vault** for secure key management
3. **Implement proper authentication** and authorization
4. **Add monitoring** with Application Insights

### **Phase 2: Advanced Features**

1. **Custom analyzers** for tax-specific terms
2. **Faceted search** for filtering
3. **Auto-complete** and suggestions
4. **Search analytics** and reporting

### **Phase 3: Enterprise Integration**

1. **Power BI integration** for dashboards
2. **Azure Data Factory** for data pipeline
3. **ML models** for predictive insights
4. **Multi-tenant** architecture

---

## ✅ **Success Checklist**

- [ ] Azure Cognitive Search service created
- [ ] API configuration updated
- [ ] Search index created
- [ ] Tax data indexed
- [ ] Search queries working
- [ ] CEO demo prepared
- [ ] Cost monitoring set up

**Your CEO demo is now ready with enterprise-grade Azure Cognitive Search!** 🎉
