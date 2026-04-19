FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Cadence.API/Cadence.API.csproj src/Cadence.API/
RUN dotnet restore src/Cadence.API/Cadence.API.csproj
COPY src/ src/
RUN dotnet publish src/Cadence.API -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cadence.API.dll"]
