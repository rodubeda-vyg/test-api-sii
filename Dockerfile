FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

RUN sed -i '/\[openssl_init\]/a ssl_conf = ssl_sect' /etc/ssl/openssl.cnf
RUN printf "\n[ssl_sect]\nsystem_default = system_default_sect\n" >> /etc/ssl/openssl.cnf
RUN printf "\n[system_default_sect]\nMinProtocol = TLSv1.2\nCipherString = DEFAULT@SECLEVEL=0" >> /etc/ssl/openssl.cnf

RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["vyg-api-sii.csproj", "./"]
RUN dotnet restore "vyg-api-sii.csproj" --force
COPY . .
WORKDIR "/src/."
RUN dotnet build "vyg-api-sii.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "vyg-api-sii.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UserAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN mkdir -p /app/Files
RUN chown -R appuser /app/
USER appuser

ENTRYPOINT ["dotnet", "vyg-api-sii.dll"]