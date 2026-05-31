#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/erp.minepress.web/erp.minepress.web.csproj", "src/erp.minepress.web/"]
COPY ["src/erp.minepress.application/erp.minepress.application.csproj", "src/erp.minepress.application/"]
COPY ["src/erp.minepress.domain/erp.minepress.domain.csproj", "src/erp.minepress.domain/"]
COPY ["src/erp.minepress.persistence/erp.minepress.persistence.csproj", "src/erp.minepress.persistence/"]
COPY ["src/erp.minepress.frameworks/erp.minepress.frameworks.csproj", "src/erp.minepress.frameworks/"]
COPY ["src/erp.minepress.notification/erp.minepress.notification.csproj", "src/erp.minepress.notification/"]
COPY ["src/erp.minepress.infrastructure/erp.minepress.infrastructure.csproj", "src/erp.minepress.infrastructure/"]
COPY ["src/erp.minepress.tenants/erp.minepress.tenants.csproj", "src/erp.minepress.tenants/"]
COPY ["src/erp.minepress.webapi/erp.minepress.webapi.csproj", "src/erp.minepress.webapi/"]
COPY ["src/erp.minepress.bff.service/erp.minepress.bff.service.csproj", "src/erp.minepress.bff.service/"]
COPY ["src/erp.minepress.bff/erp.minepress.bff.csproj", "src/erp.minepress.bff/"]
COPY ["src/erp.minepress.printingcostingengine/erp.minepress.printingcostingengine.csproj", "src/erp.minepress.printingcostingengine/"]
COPY ["src/erp.minepress.app/erp.minepress.app.csproj", "src/erp.minepress.app/"]


RUN dotnet restore "src/erp.minepress.web/erp.minepress.web.csproj"
COPY . .
WORKDIR "/src/src/erp.minepress.web"
RUN dotnet build "erp.minepress.web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "erp.minepress.web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "erp.minepress.web.dll"]
