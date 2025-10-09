FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FitnessCal.Worker/FitnessCal.Worker.csproj", "FitnessCal.Worker/"]
COPY ["FitnessCal.BLL/FitnessCal.BLL.csproj", "FitnessCal.BLL/"]
COPY ["FitnessCal.Domain/FitnessCal.Domain.csproj", "FitnessCal.Domain/"]
COPY ["FitnessCal.DAL/FitnessCal.DAL.csproj", "FitnessCal.DAL/"]
RUN dotnet restore "FitnessCal.Worker/FitnessCal.Worker.csproj"
COPY . .
WORKDIR "/src/FitnessCal.Worker"
RUN dotnet build "./FitnessCal.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FitnessCal.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FitnessCal.Worker.dll"]
