# Quick Development Deployment Script
# This script automatically gets your IP and deploys the development environment

param(
    [switch]$SkipIpDetection,
    [string]$ManualIp = ""
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     MyLeague Development Environment Deployment          ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Change to terraform directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# Check if terraform.tfvars.dev exists
if (-not (Test-Path "terraform.tfvars.dev")) {
    Write-Error "terraform.tfvars.dev not found! Make sure you're in the terraform directory."
    exit 1
}

# Get current IP address if not skipped
if (-not $SkipIpDetection) {
    Write-Host "🌐 Detecting your public IP address..." -ForegroundColor Yellow
    
    if ($ManualIp) {
        $myIp = $ManualIp
        Write-Host "   Using manually provided IP: $myIp" -ForegroundColor Green
    } else {
        try {
            $myIp = (Invoke-WebRequest -Uri "https://ifconfig.me" -TimeoutSec 10).Content.Trim()
            Write-Host "   Your IP: $myIp" -ForegroundColor Green
        } catch {
            Write-Warning "Could not auto-detect IP address."
            Write-Host ""
            Write-Host "Please provide your IP manually:" -ForegroundColor Yellow
            Write-Host "  Option 1: Run with -ManualIp parameter:" -ForegroundColor Gray
            Write-Host "            .\deploy-dev.ps1 -ManualIp `"123.45.67.89`"" -ForegroundColor Gray
            Write-Host ""
            Write-Host "  Option 2: Get your IP manually:" -ForegroundColor Gray
            Write-Host "            Visit: https://ifconfig.me" -ForegroundColor Gray
            Write-Host "            Then edit terraform.tfvars.dev and set:" -ForegroundColor Gray
            Write-Host "            allowed_ip_addresses = [`"YOUR_IP_HERE`"]" -ForegroundColor Gray
            Write-Host ""
            Write-Host "  Option 3: Skip IP detection and use existing config:" -ForegroundColor Gray
            Write-Host "            .\deploy-dev.ps1 -SkipIpDetection" -ForegroundColor Gray
            exit 1
        }
    }
    
    Write-Host ""
    Write-Host "📝 Updating terraform.tfvars.dev with your IP..." -ForegroundColor Yellow
    
    # Read the file
    $content = Get-Content "terraform.tfvars.dev" -Raw
    
    # Update the allowed_ip_addresses line
    $content = $content -replace 'allowed_ip_addresses\s*=\s*\[.*?\]', "allowed_ip_addresses = [`"$myIp`"]"
    
    # Write back
    Set-Content "terraform.tfvars.dev" -Value $content -NoNewline
    
    Write-Host "   ✓ Updated allowed_ip_addresses = [`"$myIp`"]" -ForegroundColor Green
} else {
    Write-Host "⏭️  Skipping IP detection (using existing config)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🚀 Deploying development environment..." -ForegroundColor Cyan
Write-Host ""

# Check if Terraform is initialized
if (-not (Test-Path ".terraform")) {
    Write-Host "📦 Initializing Terraform (first time setup)..." -ForegroundColor Yellow
    terraform init
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Terraform init failed"
        exit 1
    }
    Write-Host ""
}

# Run terraform apply
Write-Host "Running: terraform apply -var-file=`"terraform.tfvars.dev`"" -ForegroundColor Gray
Write-Host ""

terraform apply -var-file="terraform.tfvars.dev"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Terraform apply failed"
    exit 1
}

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║          Development Environment Deployed! ✓              ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

Write-Host "📊 Getting connection details..." -ForegroundColor Cyan
Write-Host ""

# Get outputs
$dbHost = terraform output -raw postgres_server_fqdn
$dbUser = terraform output -raw postgres_admin_user
$dbPassword = terraform output -raw postgres_admin_password
$backendUrl = terraform output -raw backend_url
$frontendUrl = terraform output -raw frontend_url

Write-Host "🌐 Application URLs:" -ForegroundColor Yellow
Write-Host "   Frontend: $frontendUrl" -ForegroundColor White
Write-Host "   Backend:  $backendUrl" -ForegroundColor White
Write-Host ""

Write-Host "🗄️  Database Connection (DBeaver):" -ForegroundColor Yellow
Write-Host "   Host:     $dbHost" -ForegroundColor White
Write-Host "   Port:     5432" -ForegroundColor White
Write-Host "   Database: myleague" -ForegroundColor White
Write-Host "   Username: $dbUser" -ForegroundColor White
Write-Host "   Password: $dbPassword" -ForegroundColor White
Write-Host "   SSL Mode: require" -ForegroundColor White
Write-Host ""

Write-Host "💡 Tips:" -ForegroundColor Cyan
Write-Host "   • Connect to the database directly from DBeaver (no SSH needed!)" -ForegroundColor Gray
Write-Host "   • Container Apps scale to zero when idle to save costs" -ForegroundColor Gray
Write-Host "   • Your IP is whitelisted: $myIp" -ForegroundColor Gray
Write-Host ""

Write-Host "📚 Next Steps:" -ForegroundColor Cyan
Write-Host "   1. Build and push Docker images:" -ForegroundColor White
Write-Host "      cd .." -ForegroundColor Gray
Write-Host "      .\scripts\deploy-backend.ps1 -AcrName (terraform output -raw container_registry_name)" -ForegroundColor Gray
Write-Host "      .\scripts\deploy-frontend.ps1 -AcrName (terraform output -raw container_registry_name)" -ForegroundColor Gray
Write-Host ""
Write-Host "   2. Open frontend URL in browser: $frontendUrl" -ForegroundColor White
Write-Host ""
Write-Host "   3. Connect to database with DBeaver using details above" -ForegroundColor White
Write-Host ""

Write-Host "📖 Documentation:" -ForegroundColor Cyan
Write-Host "   • Full guide: DEV-VS-PROD-DEPLOYMENT.md" -ForegroundColor Gray
Write-Host "   • Main README: README.md" -ForegroundColor Gray
Write-Host ""

Write-Host "Done! 🎉" -ForegroundColor Green

