# syntax=docker/dockerfile:1
# One image definition for every .NET service in the solution.
# docker-compose passes PROJECT (which .csproj to publish) per service,
# and the DLL to run via `command:`.

# ---- build stage: full SDK, restores + publishes ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG PROJECT
WORKDIR /src
COPY . .
RUN dotnet publish "$PROJECT" -c Release -o /app

# ---- runtime stage: slim ASP.NET runtime, just the published output ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# entrypoint is the runtime; each service supplies its DLL via `command:` in compose
ENTRYPOINT ["dotnet"]
