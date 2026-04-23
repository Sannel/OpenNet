FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY *.sln .
COPY src/OpenNet.Core/OpenNet.Core.csproj src/OpenNet.Core/
COPY src/OpenNet.Api/OpenNet.Api.csproj src/OpenNet.Api/
COPY src/OpenNet.Agent/OpenNet.Agent.csproj src/OpenNet.Agent/
COPY src/OpenNet.Web/OpenNet.Web.csproj src/OpenNet.Web/

RUN dotnet restore src/OpenNet.Api/OpenNet.Api.csproj

COPY . .

RUN dotnet publish src/OpenNet.Api/OpenNet.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "OpenNet.Api.dll"]
