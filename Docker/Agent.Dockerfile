FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
# COPY *.csproj ./loly-agent/
# WORKDIR /app/loly-agent
# RUN dotnet clean && dotnet restore
COPY ./*.sln ./
COPY ./Loly.Models/*.csproj ./Loly.Models/
RUN cd Loly.Models && dotnet clean && dotnet restore && cd ..
COPY ./Loly.Analysers/*.csproj ./Loly.Analysers/
RUN cd Loly.Analysers && dotnet clean && dotnet restore && cd ..
COPY ./Loly.Kafka/*.csproj ./Loly.Kafka/
RUN cd Loly.Kafka && dotnet clean && dotnet restore && cd ..
COPY ./Loly.Agent/*.csproj ./Loly.Agent/
RUN cd Loly.Agent && dotnet clean && dotnet restore && cd ..

# copy everything else and build app
COPY . ./
RUN dotnet publish -c ReleaseAgent -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/Loly.Agent/out ./
RUN rm ./*.Development.json
# RUN apt-get update && apt-get install apt-utils -y && apt-get install libmagic1 -y
#RUN apt-get update && apt-get install htop -y
ENV IS_DOCKER=true

VOLUME [ "/app/Configs" ]
EXPOSE 80
ENTRYPOINT ["dotnet", "Loly.Agent.dll"]