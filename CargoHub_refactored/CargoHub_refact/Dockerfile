#------------------------------------------------------------------------------------
# 1: build
# Base image .Net: 
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

#------------------------------------------------------------------------------------

# Set the working directory for all subsequent instructions
WORKDIR /src

#------------------------------------------------------------------------------------

# Copy project file, downloasd the packages 
COPY ["CargoHub_refact.csproj", "./"]
RUN dotnet restore "CargoHub_refact.csproj"

#------------------------------------------------------------------------------------

# copy rest of application
COPY . .
#------------------------------------------------------------------------------------

# Publish application
RUN dotnet publish "CargoHub_refact.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

#------------------------------------------------------------------------------------
#2: run
#heeft de runtime van .net 
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

#------------------------------------------------------------------------------------

WORKDIR /app

#------------------------------------------------------------------------------------

#create directory database
RUN mkdir -p /app/data

#------------------------------------------------------------------------------------

#copy published application
COPY --from=build /app/publish .
#------------------------------------------------------------------------------------
#kopie database buiten volume
COPY --from=build /app/publish/DataSources/cargohub.db /app/cargohub.db
#dockerfile kopieert script van projectmap naar docker image
COPY docker_entrypoint.sh /app/docker-entrypoint.sh
#normaliseert Windows-regelindelingen en geeft het script uitvoerrechten
RUN sed -i 's/\r$//' /app/docker-entrypoint.sh \
    && chmod +x /app/docker-entrypoint.sh

#------------------------------------------------------------------------------------

#configure ASP.NET Core
ENV ASPNETCORE_URLS=http://+:5000

ENV CARGOHUB_DB_PATH=/app/data/cargohub.db

#------------------------------------------------------------------------------------

#documenteert poort
EXPOSE 5000

#------------------------------------------------------------------------------------

#start api
ENTRYPOINT ["/app/docker-entrypoint.sh"]

#------------------------------------------------------------------------------------