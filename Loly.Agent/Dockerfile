FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./loly-agent/
WORKDIR /app/loly-agent
RUN dotnet clean && dotnet restore

# copy everything else and build app
COPY . ./
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/loly-agent/out ./
RUN rm ./*.Development.json
RUN apt-get update && apt-get install apt-utils -y && apt-get install libmagic1 -y
ENV IS_DOCKER=true

VOLUME [ "/app/Configs" ]
EXPOSE 80
ENTRYPOINT ["dotnet", "Loly.Agent.dll"]