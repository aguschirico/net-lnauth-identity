FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM node:20-slim as spabuild
WORKDIR /app

RUN npm i -g pnpm
ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
RUN corepack enable
COPY src/NetCore.Identity.LnAuth.Api/spa/package.json .
RUN pnpm install
COPY src/NetCore.Identity.LnAuth.Api/spa/ .

RUN pnpm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/NetCore.Identity.LnAuth.Api/NetCore.Identity.LnAuth.Api.csproj", "src/NetCore.Identity.LnAuth.Api/"]
RUN dotnet restore "src/NetCore.Identity.LnAuth.Api/NetCore.Identity.LnAuth.Api.csproj"
COPY . .

WORKDIR "/src/NetCore.Identity.LnAuth.Api"
RUN dotnet build "NetCore.Identity.LnAuth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCore.Identity.LnAuth.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=spabuild /app/dist/ ./spa/dist/
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NetCore.Identity.LnAuth.Api.dll"]
