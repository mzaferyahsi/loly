dotnet test /p:CollectCoverage=true /p:Include=\"[Loly*]*\" /p:CoverletOutputFormat=\"opencover,lcov\" /p:ExcludeByAttribute=\"ExcludeFromCoverageAttribute\" /p:CoverletOutput=../lcov && \
  dotnet ~/.nuget/packages/reportgenerator/4.2.12/tools/netcoreapp2.1/ReportGenerator.dll  -reports:lcov.opencover.xml -targetdir:./coverage -assemblyfilters:+Loly.*