# To start the project

## Stop everything and clean up
Set-Location -Path "F:\Tasks\Travel-Management-System-Task\backend"
docker compose down -v

## Start the backend:
docker compose up -d --build
Start-Sleep -Seconds 20

## Check backend health:
Invoke-RestMethod -Uri http://localhost:5000/health

## Test login:
$body = @{ email = "demo@company.com"; password = "Password123!" } | ConvertTo-Json
$resp = Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/auth/login -ContentType "application/json" -Body $body
$resp.token


## In a NEW PowerShell window, start the frontend:
Set-Location -Path "F:\Tasks\Travel-Management-System-Task\frontend"
npm start
