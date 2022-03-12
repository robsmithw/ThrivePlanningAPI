FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build-env
WORKDIR /ThrivePlanningAPI

COPY /ThrivePlanningAPI/*.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal
WORKDIR /ThrivePlanningAPI
EXPOSE 80
COPY --from=build-env ThrivePlanningAPI/out .
RUN chmod +x ThrivePlanningAPI.dll

ENTRYPOINT ["dotnet", "ThrivePlanningAPI.dll"]