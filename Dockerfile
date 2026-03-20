# SỬ DỤNG .NET 8 SDK ĐỂ BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# COPY VÀ RESTORE
COPY *.csproj ./
RUN dotnet restore

# COPY TOÀN BỘ VÀ PUBLISH
COPY . ./
RUN dotnet publish -c Release -o out

# DÙNG RUNTIME NHẸ ĐỂ CHẠY
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# CỔNG CHẠY MẶC ĐỊNH
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "DoAn.dll"]
