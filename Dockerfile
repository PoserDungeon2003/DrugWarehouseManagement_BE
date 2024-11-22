#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DrugWarehouseManagement.API/DrugWarehouseManagement.API.csproj", "DrugWarehouseManagement.API/"]
COPY ["DrugWarehouseManagement.Service/DrugWarehouseManagement.Service.csproj", "DrugWarehouseManagement.Service/"]
COPY ["DrugWarehouseManagement.Repository/DrugWarehouseManagement.Repository.csproj", "DrugWarehouseManagement.Repository/"]
COPY ["DrugWarehouseManagement.Common/DrugWarehouseManagement.Common.csproj", "DrugWarehouseManagement.Common/"]
RUN dotnet restore "./DrugWarehouseManagement.API/DrugWarehouseManagement.API.csproj"
COPY . ./
WORKDIR "/src/DrugWarehouseManagement.API"
RUN dotnet build "./DrugWarehouseManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DrugWarehouseManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DrugWarehouseManagement.API.dll"]
