# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY EnterpriseLogIntelligence.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish EnterpriseLogIntelligence.csproj -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Render sends traffic to port 10000 and terminates HTTPS in front of the app.
ENV ASPNETCORE_HTTP_PORTS=10000 \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["dotnet", "EnterpriseLogIntelligence.dll"]
