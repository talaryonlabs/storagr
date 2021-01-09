# 2021 Copyright Talaryon Studios


#
# Runtime
#
FROM mcr.microsoft.com/dotnet/aspnet:5.0 as runtime
WORKDIR /
EXPOSE 80 443

RUN mkdir -p /etc/storagr /usr/storagr/bin /usr/storagr/config /usr/storagr/wwwroot


RUN ln -s /usr/storagr/bin/storagr /usr/bin/storagrcli \
 && ln -s /usr/storagr/bin/storagrserver /usr/bin/storagrserver \
 && ln -s /usr/storagr/bin/storagrstore /usr/bin/storagrstore \
 && ln -s /usr/storagr/bin/storagrui /usr/bin/storagrui


RUN ln -s /usr/storagr/config/storagr.json /etc/storagr/storagr.json \
 && ln -s /usr/storagr/config/storagr.store.json /etc/storagr/storagr.store.json \
 && ln -s /usr/storagr/config/storagr.ui.json /etc/storagr/storagr.ui.json \
 && ln -s /usr/storagr/config/storagr.cli.json /etc/storagr/storagr.cli.json


#
# Build
#
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
RUN mkdir -p /app/bin /app/config /app/wwwroot

COPY src/Storagr.Shared Storagr.Shared
COPY src/Storagr.Client Storagr.Client

COPY src/Storagr Storagr
RUN dotnet publish "Storagr/Storagr.csproj" -c Release -o /publish \
 && cp /publish/Storagr /app/bin \
 && cp /publish/appsettings.json /bin/config/storagr.json \
 && rm /publish/appsettings.json

COPY src/Storagr.CLI Storagr.CLI
RUN dotnet publish "Storagr.CLI/Storagr.CLI.csproj" -c Release -o /publish \
 && cp /publish/Storagr.CLI /app/bin \
 && cp /publish/appsettings.json /app/config/storagr.cli.json \
 && rm /publish/appsettings.json
 
COPY src/Storagr.Store Storagr.Store
RUN dotnet publish "Storagr.Store/Storagr.Store.csproj" -c Release -o /publish \
 && cp /publish/Storagr.Store /app/bin \
 && cp /publish/appsettings.json /app/config/storagr.store.json \
 && rm /publish/appsettings.json
 
COPY src/Storagr.UI Storagr.UI
RUN dotnet publish "Storagr.UI/Storagr.UI.csproj" -c Release -o /publish \
 && cp /publish/Storagr.UI /app/bin \
 && cp /publish/appsettings.json /app/config/storagr.ui.json \
 && rm /publish/appsettings.json

RUN cp -Rf /publish/*.dll /app/bin \
 && cp -Rf /publish/*.deps.json /app/bin \
 && cp -Rf /publish/*.runtimeconfig.json /app/bin \
 && cp -Rf /publish/wwwroot/* /app/wwwroot


#
# Storagr
#
FROM runtime

COPY --from=build /app/bin/* /usr/storagr/bin/
COPY --from=build /app/config/* /usr/storagr/config/
COPY --from=build /app/wwwroot/* /usr/storagr/wwwroot/

ENV STORAGR_SQLITE_DATASOURCE /var/lib/storagr.db
ENV STORE_ROOTPATH /var/lib/store

RUN mkdir -p $STORE_ROOTPATH

CMD bash
#CMD storagr