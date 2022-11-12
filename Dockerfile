LABEL org.opencontainers.image.source=https://github.com/SimonPrinz/PingWebhook

FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder
WORKDIR /app

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

COPY --from=builder /app/out .

ENTRYPOINT ["dotnet", "PingWebhook.dll"]
