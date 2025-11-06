# Test script to validate deployment scripts work correctly
# This script tests syntax and path resolution without actually deploying

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Testing Deployment Scripts" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir

Write-Host "Scripts directory: $scriptDir" -ForegroundColor Gray
Write-Host "Project root: $projectRoot" -ForegroundColor Gray
Write-Host ""

# Test 1: Check if all scripts exist
Write-Host "Test 1: Checking if all scripts exist..." -ForegroundColor Yellow
$scripts = @("deploy-backend.ps1", "deploy-frontend.ps1", "deploy-all.ps1")
$allExist = $true

foreach ($script in $scripts) {
    $scriptPath = Join-Path $scriptDir $script
    if (Test-Path $scriptPath) {
        Write-Host "   [OK] $script exists" -ForegroundColor Green
    } else {
        Write-Host "   [FAIL] $script NOT FOUND" -ForegroundColor Red
        $allExist = $false
    }
}

if (-not $allExist) {
    Write-Error "Some scripts are missing!"
    exit 1
}

Write-Host ""

# Test 2: Test script syntax (PowerShell parsing)
Write-Host "Test 2: Validating PowerShell syntax..." -ForegroundColor Yellow

foreach ($script in $scripts) {
    $scriptPath = Join-Path $scriptDir $script
    try {
        $errors = @()
        $null = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$null, [ref]$errors)
        if ($errors.Count -eq 0) {
            Write-Host "   [OK] $script syntax is valid" -ForegroundColor Green
        } else {
            Write-Host "   [FAIL] $script has syntax errors" -ForegroundColor Red
            $allExist = $false
        }
    } catch {
        Write-Host "   [WARN] Could not validate $script syntax (continuing...)" -ForegroundColor Yellow
    }
}

Write-Host ""

# Test 3: Test path navigation (simulate what scripts do)
Write-Host "Test 3: Testing path navigation..." -ForegroundColor Yellow

Push-Location $scriptDir
$currentDir = Get-Location
Set-Location ..
$projectRootTest = Get-Location
Pop-Location

if ($projectRootTest.Path -eq $projectRoot) {
    Write-Host "   [OK] Path navigation works correctly" -ForegroundColor Green
    Write-Host "      Script dir -> Project root: $projectRootTest" -ForegroundColor Gray
} else {
    Write-Host "   [FAIL] Path navigation failed!" -ForegroundColor Red
    Write-Host "      Expected: $projectRoot" -ForegroundColor Red
    Write-Host "      Got: $projectRootTest" -ForegroundColor Red
    $allExist = $false
}

Write-Host ""

# Test 4: Test that deploy-all.ps1 can find other scripts
Write-Host "Test 4: Testing deploy-all.ps1 script references..." -ForegroundColor Yellow

$deployAllPath = Join-Path $scriptDir "deploy-all.ps1"
$deployAllContent = Get-Content $deployAllPath -Raw

if ($deployAllContent -match '\$PSScriptRoot\\deploy-backend\.ps1') {
    Write-Host "   [OK] deploy-all.ps1 references deploy-backend.ps1 correctly" -ForegroundColor Green
} else {
    Write-Host "   [FAIL] deploy-all.ps1 may have incorrect reference to deploy-backend.ps1" -ForegroundColor Red
    $allExist = $false
}

if ($deployAllContent -match '\$PSScriptRoot\\deploy-frontend\.ps1') {
    Write-Host "   [OK] deploy-all.ps1 references deploy-frontend.ps1 correctly" -ForegroundColor Green
} else {
    Write-Host "   [FAIL] deploy-all.ps1 may have incorrect reference to deploy-frontend.ps1" -ForegroundColor Red
    $allExist = $false
}

Write-Host ""

# Test 5: Test that scripts can find Dockerfiles (relative to project root)
Write-Host "Test 5: Testing Dockerfile path resolution..." -ForegroundColor Yellow

Push-Location $projectRoot

$backendDockerfile = "src/backend/WebAPI/Dockerfile"
$frontendDockerfile = "src/frontend/Dockerfile.prod"
$frontendDockerfileFallback = "src/frontend/Dockerfile"

if (Test-Path $backendDockerfile) {
    Write-Host "   [OK] Backend Dockerfile found: $backendDockerfile" -ForegroundColor Green
} else {
    Write-Host "   [WARN] Backend Dockerfile not found (may be OK if not created yet)" -ForegroundColor Yellow
}

if (Test-Path $frontendDockerfile) {
    Write-Host "   [OK] Frontend Dockerfile.prod found: $frontendDockerfile" -ForegroundColor Green
} elseif (Test-Path $frontendDockerfileFallback) {
    Write-Host "   [OK] Frontend Dockerfile found (fallback): $frontendDockerfileFallback" -ForegroundColor Green
} else {
    Write-Host "   [WARN] Frontend Dockerfile not found (may be OK if not created yet)" -ForegroundColor Yellow
}

Pop-Location

Write-Host ""

# Test 6: Test parameter help (Get-Help)
Write-Host "Test 6: Testing script parameter definitions..." -ForegroundColor Yellow

foreach ($script in $scripts) {
    $scriptPath = Join-Path $scriptDir $script
    try {
        $help = Get-Help $scriptPath -ErrorAction SilentlyContinue
        if ($help) {
            Write-Host "   [OK] $script has valid parameter definitions" -ForegroundColor Green
        } else {
            Write-Host "   [WARN] $script help not available (may be OK)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "   [WARN] Could not get help for $script (may be OK)" -ForegroundColor Yellow
    }
}

Write-Host ""

# Summary
Write-Host "========================================" -ForegroundColor Cyan
if ($allExist) {
    Write-Host "  All Tests Passed!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Scripts are ready to use!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Example usage:" -ForegroundColor Cyan
    Write-Host "  .\scripts\deploy-backend.ps1 -AcrName testacr -Tag latest" -ForegroundColor Gray
    Write-Host "  .\scripts\deploy-frontend.ps1 -AcrName testacr -Tag latest -ApiUrl https://api.example.com" -ForegroundColor Gray
    Write-Host "  .\scripts\deploy-all.ps1 -AcrName testacr -Tag latest -ApiUrl https://api.example.com" -ForegroundColor Gray
    exit 0
} else {
    Write-Host "  Some Tests Failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}
