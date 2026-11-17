FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet tool install --global dotnet-ef --version "10.*"
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet restore
RUN if [ ! -d Migrations/Postgres ]; then dotnet ef migrations add Initial --context PostgresAppDbContext --output-dir Migrations/Postgres; fi
RUN if [ ! -d Migrations/Sqlite ]; then dotnet ef migrations add Initial --context SqliteAppDbContext --output-dir Migrations/Sqlite; fi
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["sh", "-c", "dotnet $(basename $(ls *.runtimeconfig.json) .runtimeconfig.json).dll"]
