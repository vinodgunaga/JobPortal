FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JobPortal.API/JobPortal.API.csproj",                         "JobPortal.API/"]
COPY ["JobPortal.Application/JobPortal.Application.csproj",         "JobPortal.Application/"]
COPY ["JobPortal.Domain/JobPortal.Domain.csproj",                   "JobPortal.Domain/"]
COPY ["JobPortal.Infrastructure/JobPortal.Infrastructure.csproj",   "JobPortal.Infrastructure/"]
RUN dotnet restore "JobPortal.API/JobPortal.API.csproj"
COPY . .
RUN dotnet build "JobPortal.API/JobPortal.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "JobPortal.API/JobPortal.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobPortal.API.dll"]