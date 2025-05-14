# --- build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# копируем весь репозиторий внутрь контейнера
COPY . .

# публикуем проект (путь до твоего csproj)
RUN dotnet publish AttestationProject/AttestationProject.csproj -c Release -o /app/publish

# --- runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render передаёт порт в переменной PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT
CMD ["dotnet", "AttestationProject.dll"]   # ← если dll названа иначе — подправь
