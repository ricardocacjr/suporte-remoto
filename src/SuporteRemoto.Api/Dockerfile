# Build context: raiz do repositório (não a pasta deste projeto).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/SuporteRemoto.Api/SuporteRemoto.Api.csproj", "src/SuporteRemoto.Api/"]
COPY ["src/SuporteRemoto.Application/SuporteRemoto.Application.csproj", "src/SuporteRemoto.Application/"]
COPY ["src/SuporteRemoto.Infrastructure/SuporteRemoto.Infrastructure.csproj", "src/SuporteRemoto.Infrastructure/"]
COPY ["src/SuporteRemoto.Domain/SuporteRemoto.Domain.csproj", "src/SuporteRemoto.Domain/"]
COPY ["src/SuporteRemoto.Shared/SuporteRemoto.Shared.csproj", "src/SuporteRemoto.Shared/"]
RUN dotnet restore "src/SuporteRemoto.Api/SuporteRemoto.Api.csproj"

COPY src/SuporteRemoto.Api/ src/SuporteRemoto.Api/
COPY src/SuporteRemoto.Application/ src/SuporteRemoto.Application/
COPY src/SuporteRemoto.Infrastructure/ src/SuporteRemoto.Infrastructure/
COPY src/SuporteRemoto.Domain/ src/SuporteRemoto.Domain/
COPY src/SuporteRemoto.Shared/ src/SuporteRemoto.Shared/

RUN dotnet publish "src/SuporteRemoto.Api/SuporteRemoto.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SuporteRemoto.Api.dll"]
