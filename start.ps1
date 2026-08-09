if (-not (Test-Path .env)) {
    Copy-Item .env.example .env
    Write-Host "Создан .env из шаблона. Для публичного развёртывания замените JWT_KEY."
}
docker compose up --build
