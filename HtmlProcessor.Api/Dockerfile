FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["HtmlProcessor.Api.csproj", "./"]
RUN dotnet restore "HtmlProcessor.Api.csproj"

COPY . .
RUN rm -rf bin obj
RUN dotnet publish "HtmlProcessor.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8090

ENV ASPNETCORE_URLS=http://+:8090

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HtmlProcessor.Api.dll"]