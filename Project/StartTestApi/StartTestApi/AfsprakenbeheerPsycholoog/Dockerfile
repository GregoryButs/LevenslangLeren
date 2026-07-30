# Stage 1: Build React Frontend (ClientApp)
FROM node:20-alpine AS client-build
WORKDIR /app/ClientApp
COPY ClientApp/package*.json ./
RUN npm install
COPY ClientApp/ ./
RUN npm run build

# Stage 2: Build C# Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . ./
COPY --from=client-build /app/wwwroot ./wwwroot
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
RUN mkdir -p /app/data
ENTRYPOINT ["dotnet", "AfsprakenbeheerPsycholoog.dll"]
