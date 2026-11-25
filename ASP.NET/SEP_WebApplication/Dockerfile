FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# restore first: this layer is cached as long as the csproj does not change
COPY SEP_WebApplication.csproj .
COPY Attributes/*.dll Attributes/
RUN dotnet restore "SEP_WebApplication.csproj"

COPY . .
RUN dotnet publish "SEP_WebApplication.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
# Program.cs reads PORT (hosters override it)
ENV PORT=10000
EXPOSE 10000

# do not run as root
USER $APP_UID

ENTRYPOINT ["dotnet", "SEP_WebApplication.dll"]
