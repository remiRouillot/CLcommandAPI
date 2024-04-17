
FROM registry.access.redhat.com/ubi8/dotnet-80
USER app
WORKDIR /app
EXPOSE 5000

ENV CSTR=""

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CLcommandAPI.csproj", "."]
RUN dotnet restore "./././CLcommandAPI.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "./CLcommandAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CLcommandAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM registry.access.redhat.com/ubi8/dotnet-80
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CLcommandAPI.dll"]