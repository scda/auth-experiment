FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /source

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /source/out .

ARG user=authservice
ARG group=authservice
ARG uid=1008
ARG gid=1008
ARG port=5000
ARG managementport=5001

RUN groupadd -g ${gid} ${group} && useradd -u ${uid} -g ${group} -s /bin/sh ${user}

USER ${user}

ENTRYPOINT ["dotnet", "authservice.dll"]

ENV ASPNETCORE_URLS=http://*:${port};http://*:${managementport}
ENV ASPNETCORE_MANAGEMENTPORT=${managementport}

EXPOSE ${port} ${managementport}