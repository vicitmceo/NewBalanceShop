FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY NewBalanceShop.csproj .
RUN dotnet restore NewBalanceShop.csproj

COPY . .
RUN dotnet publish NewBalanceShop.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "NewBalanceShop.dll"]
