
FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /src

COPY . .

RUN dotnet restore
RUN dotnet build --no-restore

CMD ["dotnet", "test", "--no-build"]
