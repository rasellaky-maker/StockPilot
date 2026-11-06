FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "SEP_WebApplication.csproj"

RUN dotnet publish "SEP_WebApplication.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet SEP_WebApplication.dll --urls http://0.0.0.0:${PORT:-10000}"]