FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
# COPY *.csproj ./loly-agent/
# WORKDIR /app/loly-agent
# RUN dotnet clean && dotnet restore
COPY ./*.sln ./
COPY ./Loly.Models/*.csproj ./Loly.Models/
RUN cd Loly.Models && dotnet clean && dotnet restore && cd ..
COPY ./Loly.Configuration/*.csproj ./Loly.Configuration/
RUN cd Loly.Configuration && dotnet clean && dotnet restore && cd ..
COPY ./Loly.Analysers/*.csproj ./Loly.Analysers/
RUN cd Loly.Analysers && dotnet clean && dotnet restore && cd ..
COPY ./Loly.Streaming/*.csproj ./Loly.Streaming/
RUN cd Loly.Streaming && dotnet clean && dotnet restore && cd ..
COPY ./Loly.Agent/*.csproj ./Loly.Agent/
RUN cd Loly.Agent && dotnet clean && dotnet restore && cd ..

# copy everything else and build app
COPY . ./
RUN dotnet publish -c ReleaseAgent -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-bionic AS runtime
WORKDIR /app
COPY --from=build /app/Loly.Agent/out ./
RUN mkdir /usr/share/ca-certificates/yahsi/
COPY Docker/yahsi-root-ca.crt  /usr/share/ca-certificates/yahsi/yahsi-root-ca.crt
RUN echo "yahsi/yahsi-root-ca.crt" >> /etc/ca-certificates.conf
COPY Docker/yahsi-tls-ca.crt  /usr/share/ca-certificates/yahsi/yahsi-tls-ca.crt
RUN echo "yahsi/yahsi-tls-ca.crt" >> /etc/ca-certificates.conf
RUN update-ca-certificates
RUN rm ./*.Development.json
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install apt-utils -yq --no-install-recommends
#RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install htop -yq
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install iputils-ping -yq --no-install-recommends
ENV IS_DOCKER=true

VOLUME [ "/app/Configs" ]
EXPOSE 80
ENTRYPOINT ["dotnet", "Loly.Agent.dll"]