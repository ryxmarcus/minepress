FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

# Restore & publish the Razor Pages project
RUN dotnet restore "src/erp.minepress.web/erp.minepress.web.csproj"
RUN dotnet publish "src/erp.minepress.web/erp.minepress.web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "erp.minepress.web.dll"]
