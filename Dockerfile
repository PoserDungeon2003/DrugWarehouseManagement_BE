# See https://aka.ms/customizecontainer to learn how to customize your debug container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only project files first to leverage layer caching for package restore
COPY ["DrugWarehouseManagement.API/DrugWarehouseManagement.API.csproj", "DrugWarehouseManagement.API/"]
COPY ["DrugWarehouseManagement.Service/DrugWarehouseManagement.Service.csproj", "DrugWarehouseManagement.Service/"]
COPY ["DrugWarehouseManagement.Repository/DrugWarehouseManagement.Repository.csproj", "DrugWarehouseManagement.Repository/"]
COPY ["DrugWarehouseManagement.Common/DrugWarehouseManagement.Common.csproj", "DrugWarehouseManagement.Common/"]
COPY ["DrugWarehouseManagement.UnitTest/DrugWarehouseManagement.UnitTest.csproj", "DrugWarehouseManagement.UnitTest/"]

# Restore packages as a separate layer
RUN dotnet restore "./DrugWarehouseManagement.API/DrugWarehouseManagement.API.csproj" --verbosity minimal
RUN dotnet restore "./DrugWarehouseManagement.UnitTest/DrugWarehouseManagement.UnitTest.csproj" --verbosity minimal

# Copy the rest of the code
COPY . ./

# Build with performance optimizations
WORKDIR "/src/DrugWarehouseManagement.API"
RUN dotnet build "./DrugWarehouseManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore /p:UseSharedCompilation=true /maxcpucount:8

# Run tests in a separate stage
FROM build AS test
WORKDIR "/src/DrugWarehouseManagement.UnitTest"
RUN dotnet test -c $BUILD_CONFIGURATION --no-restore --logger:trx

# Publish
FROM build AS publish
WORKDIR "/src/DrugWarehouseManagement.API"
RUN dotnet publish "./DrugWarehouseManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DrugWarehouseManagement.API.dll"]