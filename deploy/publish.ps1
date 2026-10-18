# Build frontend + publish backend vào thư mục ./publish (chạy từ bất kỳ đâu)
$root = Split-Path $PSScriptRoot -Parent
$out  = Join-Path $root 'publish'

Push-Location (Join-Path $root 'frontend')
npm ci; if ($LASTEXITCODE) { Pop-Location; exit 1 }
npm run build; if ($LASTEXITCODE) { Pop-Location; exit 1 }
Pop-Location

dotnet publish (Join-Path $root 'backend\WatchShop.Api') -c Release -o $out
if ($LASTEXITCODE) { exit 1 }

# Chép bản build React vào wwwroot của bản publish
New-Item -ItemType Directory -Force (Join-Path $out 'wwwroot') | Out-Null
Copy-Item (Join-Path $root 'frontend\dist\*') (Join-Path $out 'wwwroot') -Recurse -Force
Write-Host "Xong: $out"
