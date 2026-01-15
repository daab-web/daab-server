FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Daab.Web/Daab.Web.csproj", "src/Daab.Web/"]
COPY ["host/Daab.ServiceDefaults/Daab.ServiceDefaults.csproj", "host/Daab.ServiceDefaults/"]
COPY ["src/Modules/Scientists/Daab.Modules.Scientists/Daab.Modules.Scientists.csproj", "src/Modules/Scientists/Daab.Modules.Scientists/"]
COPY ["src/Daab.SharedKernel/Daab.SharedKernel.csproj", "src/Daab.SharedKernel/"]
RUN dotnet restore "src/Daab.Web/Daab.Web.csproj"
COPY . .
WORKDIR "/src/src/Daab.Web"
RUN dotnet build "./Daab.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Daab.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Daab.Web.dll"]
