FROM mcr.microsoft.com/dotnet/sdk:10.0.400-noble AS base
WORKDIR /build

COPY global.json .
COPY Restall.slnx .
COPY src/Restall.Domain/Restall.Domain.csproj             src/Restall.Domain/
COPY src/Restall.Application/Restall.Application.csproj   src/Restall.Application/
COPY src/Restall.Infrastructure/Restall.Infrastructure.csproj src/Restall.Infrastructure/
COPY src/Restall.UI/Restall.UI.csproj                     src/Restall.UI/

FROM base AS restore-linux
RUN dotnet restore src/Restall.UI/Restall.UI.csproj -r linux-x64 --locked-mode

FROM base AS restore-windows
RUN dotnet restore src/Restall.UI/Restall.UI.csproj -r win-x64 --locked-mode

FROM restore-linux AS linux
COPY . .
RUN dotnet publish src/Restall.UI/Restall.UI.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained \
    --no-restore \
    -o /out

FROM restore-windows AS windows
COPY . .
RUN dotnet publish src/Restall.UI/Restall.UI.csproj \
    -c Release \
    -r win-x64 \
    --self-contained \
    --no-restore \
    -o /out

FROM scratch AS linux-export
COPY --from=linux /out /

FROM scratch AS windows-export
COPY --from=windows /out /

FROM base AS ci-restore
COPY Directory.Build.props .
COPY src/Restall.Domain/packages.lock.json         src/Restall.Domain/
COPY src/Restall.Application/packages.lock.json    src/Restall.Application/
COPY src/Restall.Infrastructure/packages.lock.json src/Restall.Infrastructure/
COPY src/Restall.UI/packages.lock.json             src/Restall.UI/
COPY tests/Restall.Application.Tests/Restall.Application.Tests.csproj       tests/Restall.Application.Tests/
COPY tests/Restall.Application.Tests/packages.lock.json                     tests/Restall.Application.Tests/
COPY tests/Restall.Infrastructure.Tests/Restall.Infrastructure.Tests.csproj tests/Restall.Infrastructure.Tests/
COPY tests/Restall.Infrastructure.Tests/packages.lock.json                  tests/Restall.Infrastructure.Tests/
RUN dotnet restore Restall.slnx --locked-mode

FROM ci-restore AS ci-source
COPY . .

FROM ci-source AS format
RUN dotnet format Restall.slnx --verify-no-changes --no-restore

FROM ci-source AS build
RUN dotnet build Restall.slnx -c Release --no-restore

FROM build AS test
RUN dotnet test --solution Restall.slnx -c Release --no-build
