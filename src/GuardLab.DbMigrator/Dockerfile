FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/GuardLab.Core/GuardLab.Core.csproj src/GuardLab.Core/
COPY src/GuardLab.Application/GuardLab.Application.csproj src/GuardLab.Application/
COPY src/GuardLab.Infrastructure/GuardLab.Infrastructure.csproj src/GuardLab.Infrastructure/
COPY src/GuardLab.DbMigrator/GuardLab.DbMigrator.csproj src/GuardLab.DbMigrator/
RUN dotnet restore src/GuardLab.DbMigrator/GuardLab.DbMigrator.csproj
COPY infra/ infra/
COPY src/ src/
RUN dotnet publish src/GuardLab.DbMigrator/GuardLab.DbMigrator.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GuardLab.DbMigrator.dll"]
