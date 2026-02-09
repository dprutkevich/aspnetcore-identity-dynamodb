@echo off
setlocal enabledelayedexpansion

REM UMS Identity Infrastructure Deployment Script for Windows
REM This script helps deploy the UMS Identity DynamoDB infrastructure

set SCRIPT_DIR=%~dp0
set TERRAFORM_DIR=%SCRIPT_DIR%

REM Check if terraform is installed
terraform version >nul 2>&1
if !errorlevel! neq 0 (
    echo Error: Terraform is not installed. Please install it first.
    exit /b 1
)

REM Check AWS credentials
aws sts get-caller-identity >nul 2>&1
if !errorlevel! neq 0 (
    echo Warning: AWS credentials might not be configured.
    echo You can configure them using: aws configure
)

echo.
echo 🚀 UMS Identity Infrastructure Management
echo ========================================

set ACTION=%1
if "%ACTION%"=="" set ACTION=help

if "%ACTION%"=="init" goto :init
if "%ACTION%"=="validate" goto :validate
if "%ACTION%"=="plan" goto :plan
if "%ACTION%"=="apply" goto :apply
if "%ACTION%"=="destroy" goto :destroy
if "%ACTION%"=="outputs" goto :outputs
goto :help

:init
echo ℹ️  Initializing Terraform...
cd /d "%TERRAFORM_DIR%"
terraform init
if !errorlevel! equ 0 (
    echo ✅ Terraform initialized
) else (
    echo ❌ Failed to initialize Terraform
    exit /b 1
)
goto :end

:validate
echo ℹ️  Validating Terraform configuration...
cd /d "%TERRAFORM_DIR%"
terraform validate
if !errorlevel! equ 0 (
    echo ✅ Terraform configuration is valid
) else (
    echo ❌ Terraform configuration is invalid
    exit /b 1
)
goto :end

:plan
echo ℹ️  Planning infrastructure changes...
cd /d "%TERRAFORM_DIR%"
if exist "terraform.tfvars" (
    terraform plan -var-file="terraform.tfvars"
) else (
    echo ⚠️  No terraform.tfvars file found. Using default values.
    terraform plan
)
goto :end

:apply
echo ℹ️  Applying infrastructure changes...
cd /d "%TERRAFORM_DIR%"
if exist "terraform.tfvars" (
    terraform apply -var-file="terraform.tfvars"
) else (
    echo ⚠️  No terraform.tfvars file found. Using default values.
    terraform apply
)
if !errorlevel! equ 0 (
    echo ✅ Infrastructure applied successfully
    echo.
    echo ℹ️  Showing outputs...
    terraform output
) else (
    echo ❌ Failed to apply infrastructure
    exit /b 1
)
goto :end

:destroy
echo ⚠️  This will destroy all UMS Identity infrastructure!
set /p confirm=Are you sure you want to continue? (type 'yes' to confirm): 
if "!confirm!"=="yes" (
    echo ℹ️  Destroying infrastructure...
    cd /d "%TERRAFORM_DIR%"
    if exist "terraform.tfvars" (
        terraform destroy -var-file="terraform.tfvars"
    ) else (
        terraform destroy
    )
    if !errorlevel! equ 0 (
        echo ✅ Infrastructure destroyed
    ) else (
        echo ❌ Failed to destroy infrastructure
        exit /b 1
    )
) else (
    echo ℹ️  Operation cancelled
)
goto :end

:outputs
echo ℹ️  Showing Terraform outputs...
cd /d "%TERRAFORM_DIR%"
terraform output
goto :end

:help
echo Usage: %0 {init^|validate^|plan^|apply^|destroy^|outputs^|help}
echo.
echo Commands:
echo   init      - Initialize Terraform
echo   validate  - Validate Terraform configuration
echo   plan      - Plan infrastructure changes
echo   apply     - Apply infrastructure changes
echo   destroy   - Destroy infrastructure
echo   outputs   - Show Terraform outputs
echo   help      - Show this help message
echo.
echo Examples:
echo   %0 plan     # See what will be created
echo   %0 apply    # Create the infrastructure
echo   %0 outputs  # Show connection details
echo.
echo Configuration:
echo   Copy terraform.tfvars.example to terraform.tfvars and customize
goto :end

:end
endlocal
