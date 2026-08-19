# Root Dockerfile for building the TravelManagement API image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TravelManagement.API/TravelManagement.API.csproj", "TravelManagement.API/"]
COPY ["TravelManagement.Infrastructure/TravelManagement.Infrastructure.csproj", "TravelManagement.Infrastructure/"]
COPY ["TravelManagement.Application/TravelManagement.Application.csproj", "TravelManagement.Application/"]
COPY ["TravelManagement.Domain/TravelManagement.Domain.csproj", "TravelManagement.Domain/"]
RUN dotnet restore "TravelManagement.API/TravelManagement.API.csproj"
COPY . .
WORKDIR /src/TravelManagement.API
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TravelManagement.API.dll"]
