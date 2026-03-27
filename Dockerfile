FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /src

COPY global.json .
COPY Restall.slnx .
COPY src/Restall.Domain/Restall.Domain.csproj             src/Restall.Domain/
COPY src/Restall.Application/Restall.Application.csproj   src/Restall.Application/
COPY src/Restall.Infrastructure/Restall.Infrastructure.csproj src/Restall.Infrastructure/
COPY src/Restall.UI/Restall.UI.csproj                     src/Restall.UI/

FROM base AS restore-linux
RUN dotnet restore src/Restall.UI/Restall.UI.csproj -r linux-x64

FROM base AS restore-windows
RUN dotnet restore src/Restall.UI/Restall.UI.csproj -r win-x64

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