dotnet test /p:CollectCoverage=true /p:Include=\"[Loly*]*\" /p:CoverletOutputFormat=\"opencover,lcov\" /p:ExcludeByAttribute=\"ExcludeFromCoverageAttribute\" /p:CoverletOutput=./lcov && \
  cd Loly.Agent.Tests/ && \
  dotnet reportgenerator -reports:lcov.opencover.xml -targetdir:./coverage -assemblyfilters:+Loly.* && \
  cd ..