# IRS Assistant API - Enterprise Setup Guide

## 🏗️ Architecture Overview

This API follows **Clean Architecture** principles with **CQRS** (Command Query Responsibility Segregation) and **MediatR** patterns, designed for enterprise-scale applications.

### **Architecture Layers:**

```
┌─────────────────────────────────────────┐
│              Controllers                │ ← API Layer (v1)
├─────────────────────────────────────────┤
│            Application                  │ ← CQRS Commands/Queries
├─────────────────────────────────────────┤
│              Domain                     │ ← Entities & Business Logic
├─────────────────────────────────────────┤
│            Infrastructure               │ ← Azure Services & Data Access
└─────────────────────────────────────────┘
```

## 🚀 **Quick Start**

### **1. Install Dependencies**

```bash
cd IRSAssistantAPI
dotnet restore
```

### **2. Configure Azure Services**

#### **Azure SQL Database:**

1. Create Azure SQL Database
2. Update connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=IRSAssistantDb;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

#### **Azure OpenAI:**

1. Create Azure OpenAI resource
2. Deploy GPT-4 model
3. Update configuration:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "Key": "your-azure-openai-key",
    "DeploymentName": "gpt-4"
  }
}
```

#### **Azure Application Insights (Optional):**

```json
{
  "ConnectionStrings": {
    "ApplicationInsights": "your-application-insights-connection-string"
  }
}
```

### **3. Run Database Migrations**

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### **4. Run the API**

```bash
dotnet run
```

## 📋 **API Endpoints**

### **Chat Endpoints:**

- `POST /api/v1/chat/query` - Process AI queries
- `POST /api/v1/chat/sessions` - Create chat session
- `GET /api/v1/chat/sessions/{id}` - Get session with messages
- `GET /api/v1/chat/sessions` - Get user sessions

### **Health Endpoints:**

- `GET /health` - Comprehensive health check
- `GET /health/ping` - Simple ping for load balancers

### **Documentation:**

- `GET /` - Swagger UI (Development only)

## 🏢 **Enterprise Features**

### **✅ CQRS Pattern**

- **Commands**: `ProcessQueryCommand`, `CreateChatSessionCommand`
- **Queries**: `GetChatSessionQuery`, `GetUserChatSessionsQuery`
- **Handlers**: Separate command/query handlers for scalability

### **✅ MediatR Integration**

- Decoupled request/response handling
- Built-in validation pipeline
- Easy testing and mocking

### **✅ Azure Services Integration**

- **Azure OpenAI**: Natural language to SQL conversion
- **Azure SQL**: Secure data storage
- **Azure Application Insights**: Monitoring and telemetry

### **✅ Enterprise Logging**

- **Serilog** with structured logging
- Console and Application Insights sinks
- Request/response logging middleware

### **✅ Health Monitoring**

- Database connectivity checks
- SQL Server health checks
- Ready/Live health endpoints for Kubernetes

### **✅ Resilience Patterns**

- **Polly** for HTTP retry policies
- Circuit breaker patterns
- Timeout handling

### **✅ Security**

- CORS configuration
- JWT Bearer authentication ready
- SQL injection prevention

### **✅ Performance**

- Entity Framework with proper indexing
- Memory and Redis caching
- Connection pooling

## 🔧 **Configuration**

### **Environment Variables:**

```bash
# Azure SQL
ConnectionStrings__DefaultConnection="your-connection-string"

# Azure OpenAI
AzureOpenAI__Endpoint="https://your-resource.openai.azure.com/"
AzureOpenAI__Key="your-key"
AzureOpenAI__DeploymentName="gpt-4"

# Application Insights
ConnectionStrings__ApplicationInsights="your-connection-string"
```

### **Docker Support:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "IRSAssistantAPI.dll"]
```

## 📊 **Monitoring & Observability**

### **Health Checks:**

- Database connectivity
- SQL Server health
- Application readiness

### **Logging:**

- Structured JSON logging
- Request/response correlation
- Error tracking and alerting

### **Metrics:**

- Application Insights integration
- Custom telemetry events
- Performance counters

## 🧪 **Testing Strategy**

### **Unit Tests:**

- Command/Query handlers
- Service layer testing
- Domain logic validation

### **Integration Tests:**

- API endpoint testing
- Database integration
- Azure service mocking

### **Load Testing:**

- Performance benchmarking
- Scalability testing
- Stress testing

## 🚀 **Deployment Options**

### **Azure App Service:**

1. Create App Service
2. Configure connection strings
3. Deploy via Azure DevOps/GitHub Actions

### **Azure Container Instances:**

1. Build Docker image
2. Push to Azure Container Registry
3. Deploy container instance

### **Azure Kubernetes Service:**

1. Create AKS cluster
2. Deploy with Helm charts
3. Configure ingress and services

## 📈 **Scaling Considerations**

### **Horizontal Scaling:**

- Stateless API design
- Load balancer ready
- Session management

### **Database Scaling:**

- Read replicas for queries
- Connection pooling
- Query optimization

### **Caching Strategy:**

- Redis for session data
- Memory cache for frequent queries
- CDN for static content

## 🔒 **Security Best Practices**

### **Data Protection:**

- Encrypted connections
- Secure configuration
- PII data handling

### **API Security:**

- Rate limiting
- Input validation
- Authentication/Authorization

### **Infrastructure Security:**

- Network security groups
- Private endpoints
- Key vault integration

## 📚 **Next Steps**

1. **Configure Azure Services** with your credentials
2. **Run the application** and test endpoints
3. **Set up monitoring** and alerting
4. **Implement authentication** if needed
5. **Add more business logic** as required
6. **Deploy to production** environment

## 🆘 **Troubleshooting**

### **Common Issues:**

- **Database Connection**: Check connection string and firewall rules
- **Azure OpenAI**: Verify endpoint and key configuration
- **CORS Issues**: Update CORS policy for your frontend domain
- **Health Check Failures**: Check service dependencies

### **Logs:**

- Check Application Insights for detailed logs
- Console output for development debugging
- Health check endpoints for service status

---

**🎯 This API is production-ready and follows Microsoft's recommended practices for enterprise applications!**
