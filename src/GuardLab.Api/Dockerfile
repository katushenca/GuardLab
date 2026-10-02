FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/GuardLab.Core/GuardLab.Core.csproj src/GuardLab.Core/
COPY src/GuardLab.Application/GuardLab.Application.csproj src/GuardLab.Application/
COPY src/GuardLab.Infrastructure/GuardLab.Infrastructure.csproj src/GuardLab.Infrastructure/
COPY src/GuardLab.Api/GuardLab.Api.csproj src/GuardLab.Api/
RUN dotnet restore src/GuardLab.Api/GuardLab.Api.csproj
COPY src/ src/
RUN dotnet publish src/GuardLab.Api/GuardLab.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GuardLab.Api.dll"]
