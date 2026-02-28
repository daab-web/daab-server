FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION
WORKDIR /src

COPY ["Daab.slnx", "."]
COPY ["src/Directory.Build.props", "./src"]
COPY ["src/Directory.Packages.props", "./src"]

COPY ["src/Daab.Web/Daab.Web.csproj", "src/Daab.Web/"]
COPY ["src/Daab.SharedKernel/Daab.SharedKernel.csproj", "src/Daab.SharedKernel/"]
COPY ["src/Daab.Infrastructure/Daab.Infrastructure.csproj", "src/Daab.Infrastructure/"]
COPY ["src/Modules/Scientists/Daab.Modules.Scientists.csproj", "src/Modules/Scientists/"]
COPY ["src/Modules/Auth/Daab.Modules.Auth.csproj", "src/Modules/Auth/"]
COPY ["src/Modules/Activities/Daab.Modules.Activities.csproj", "src/Modules/Activities/"]

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