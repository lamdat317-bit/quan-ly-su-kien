# SỬ DỤNG .NET 8 SDK ĐỂ BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy TOÀN BỘ file từ GitHub vào Docker
COPY . .

# Tự động tìm và khôi phục các thư viện (Restore)
# Nếu file nằm trong thư mục con, lệnh này vẫn sẽ tìm thấy
RUN dotnet restore

# Publish ứng dụng
RUN dotnet publish -c Release -o out

# DÙNG RUNTIME NHẸ ĐỂ CHẠY
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# CỔNG CHẠY MẶC ĐỊNH CỦA RENDER
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Chạy file DoAn.dll (Lưu ý viết đúng hoa thường)
ENTRYPOINT ["dotnet", "DoAn.dll"]
