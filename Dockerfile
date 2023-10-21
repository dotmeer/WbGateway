FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

EXPOSE 8000

COPY . .

WORKDIR /src/WbGateway
RUN dotnet restore "WbGateway.csproj"
RUN dotnet build "WbGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WbGateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WbGateway.dll"]
