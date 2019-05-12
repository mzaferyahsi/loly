FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./loly-agent/
WORKDIR /app/loly-agent
RUN dotnet restore

# copy everything else and build app
COPY . ./
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/loly-agent/out ./
RUN rm ./*.Development.json

VOLUME [ "/app/Configs" ]
EXPOSE 80
ENTRYPOINT ["dotnet", "Loly.Agent.dll"]