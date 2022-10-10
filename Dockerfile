# Use the chassis as the base image
FROM peterkneale/peterkneale.microservices.chassis AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Use the SDK for building the service
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

# publish the service
FROM build AS publish
WORKDIR /src
RUN dotnet publish -c Release -o /app/publish

# run the chassis
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microservices.Chassis.dll"]
