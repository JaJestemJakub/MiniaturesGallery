# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /source

# copy csproj and restore as distinct layers
COPY miniatures_gallery/*.csproj .
RUN dotnet restore -a $TARGETARCH

# copy and publish app and libraries
COPY miniatures_gallery/. .
RUN dotnet publish -a $TARGETARCH --no-restore -o /app
COPY miniatures_gallery/DB/MiniaturesGallerySQLite.db DB/MiniaturesGallerySQLite.db

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
RUN adduser app_user
RUN chown -R app_user /app
USER app_user
ENTRYPOINT ["./MiniaturesGallery"]
