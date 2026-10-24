# Build: SDK 10.0.402 satisfies global.json (10.0.400, latestFeature) and bundles runtime 10.0.13.
# The SDK runs natively on the build machine and publishes for the target architecture only,
# which drops the Windows and macOS native libraries from the output.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.402-noble AS build
ARG TARGETARCH
WORKDIR /src

# Restore in its own layer so it is cached until a project file or package version changes.
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig *.slnx ./
COPY src/AttendanceSystem.Api/AttendanceSystem.Api.csproj src/AttendanceSystem.Api/
COPY src/AttendanceSystem.Application/AttendanceSystem.Application.csproj src/AttendanceSystem.Application/
COPY src/AttendanceSystem.Domain/AttendanceSystem.Domain.csproj src/AttendanceSystem.Domain/
COPY src/AttendanceSystem.Infrastructure/AttendanceSystem.Infrastructure.csproj src/AttendanceSystem.Infrastructure/
RUN dotnet restore src/AttendanceSystem.Api/AttendanceSystem.Api.csproj -a $TARGETARCH

COPY src/ src/
RUN dotnet publish src/AttendanceSystem.Api/AttendanceSystem.Api.csproj \
    -c Release -a $TARGETARCH --no-restore -o /app/publish -p:UseAppHost=false

# Run: the full (non-chiseled) Ubuntu image ships tzdata and ICU, which the IANA time zone
# lookups (America/Toronto) need, and bash for the health check.
FROM mcr.microsoft.com/dotnet/aspnet:10.0.13-noble AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Files stay owned by root, so the non-root app user can read but not modify them.
COPY --from=build /app/publish .

USER $APP_UID

# The image has no curl or wget; bash's /dev/tcp sends the request without extra packages.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --start-interval=2s --retries=3 \
    CMD ["bash", "-c", "exec 3<>/dev/tcp/127.0.0.1/8080 && printf 'GET /health HTTP/1.1\\r\\nHost: localhost\\r\\nConnection: close\\r\\n\\r\\n' >&3 && head -n 1 <&3 | grep -q ' 200 '"]

ENTRYPOINT ["dotnet", "AttendanceSystem.Api.dll"]
