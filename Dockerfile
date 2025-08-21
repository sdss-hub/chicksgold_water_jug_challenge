FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["WaterJugChallenge/WaterJugChallenge.csproj", "WaterJugChallenge/"]
COPY ["WaterJugChallenge.Tests/WaterJugChallenge.Tests.csproj", "WaterJugChallenge.Tests/"]
RUN dotnet restore "WaterJugChallenge/WaterJugChallenge.csproj"

COPY . .
WORKDIR "/src/WaterJugChallenge"
RUN dotnet build "WaterJugChallenge.csproj" -c Release -o /app/build

WORKDIR "/src"
RUN dotnet test "WaterJugChallenge.Tests/WaterJugChallenge.Tests.csproj" --configuration Release --no-restore

FROM build AS publish
WORKDIR "/src/WaterJugChallenge"
RUN dotnet publish "WaterJugChallenge.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "WaterJugChallenge.dll"]