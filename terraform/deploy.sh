#!/bin/bash

# UMS Identity Infrastructure Deployment Script
# This script helps deploy the UMS Identity DynamoDB infrastructure

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TERRAFORM_DIR="$SCRIPT_DIR"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."
    
    # Check if terraform is installed
    if ! command -v terraform &> /dev/null; then
        log_error "Terraform is not installed. Please install it first."
        exit 1
    fi
    
    # Check if AWS CLI is installed
    if ! command -v aws &> /dev/null; then
        log_warning "AWS CLI is not installed. Consider installing it for easier AWS configuration."
    fi
    
    # Check AWS credentials
    if ! aws sts get-caller-identity &> /dev/null; then
        log_error "AWS credentials are not configured. Please configure them first."
        echo "You can use: aws configure"
        exit 1
    fi
    
    log_success "Prerequisites check passed"
}

# Initialize Terraform
init_terraform() {
    log_info "Initializing Terraform..."
    cd "$TERRAFORM_DIR"
    terraform init
    log_success "Terraform initialized"
}

# Validate Terraform configuration
validate_terraform() {
    log_info "Validating Terraform configuration..."
    cd "$TERRAFORM_DIR"
    terraform validate
    log_success "Terraform configuration is valid"
}

# Plan infrastructure changes
plan_infrastructure() {
    log_info "Planning infrastructure changes..."
    cd "$TERRAFORM_DIR"
    
    if [ -f "terraform.tfvars" ]; then
        terraform plan -var-file="terraform.tfvars"
    else
        log_warning "No terraform.tfvars file found. Using default values."
        terraform plan
    fi
}

# Apply infrastructure changes
apply_infrastructure() {
    log_info "Applying infrastructure changes..."
    cd "$TERRAFORM_DIR"
    
    if [ -f "terraform.tfvars" ]; then
        terraform apply -var-file="terraform.tfvars"
    else
        log_warning "No terraform.tfvars file found. Using default values."
        terraform apply
    fi
    
    log_success "Infrastructure applied successfully"
}

# Destroy infrastructure
destroy_infrastructure() {
    log_warning "This will destroy all UMS Identity infrastructure!"
    read -p "Are you sure you want to continue? (type 'yes' to confirm): " confirm
    
    if [ "$confirm" = "yes" ]; then
        log_info "Destroying infrastructure..."
        cd "$TERRAFORM_DIR"
        
        if [ -f "terraform.tfvars" ]; then
            terraform destroy -var-file="terraform.tfvars"
        else
            terraform destroy
        fi
        
        log_success "Infrastructure destroyed"
    else
        log_info "Operation cancelled"
    fi
}

# Show outputs
show_outputs() {
    log_info "Showing Terraform outputs..."
    cd "$TERRAFORM_DIR"
    terraform output
}

# Main script
main() {
    echo "🚀 UMS Identity Infrastructure Management"
    echo "========================================"
    
    case "${1:-help}" in
        "init")
            check_prerequisites
            init_terraform
            ;;
        "validate")
            validate_terraform
            ;;
        "plan")
            check_prerequisites
            init_terraform
            validate_terraform
            plan_infrastructure
            ;;
        "apply")
            check_prerequisites
            init_terraform
            validate_terraform
            plan_infrastructure
            apply_infrastructure
            show_outputs
            ;;
        "destroy")
            check_prerequisites
            destroy_infrastructure
            ;;
        "outputs")
            show_outputs
            ;;
        "help"|*)
            echo "Usage: $0 {init|validate|plan|apply|destroy|outputs|help}"
            echo ""
            echo "Commands:"
            echo "  init      - Initialize Terraform"
            echo "  validate  - Validate Terraform configuration"
            echo "  plan      - Plan infrastructure changes"
            echo "  apply     - Apply infrastructure changes"
            echo "  destroy   - Destroy infrastructure"
            echo "  outputs   - Show Terraform outputs"
            echo "  help      - Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0 plan     # See what will be created"
            echo "  $0 apply    # Create the infrastructure"
            echo "  $0 outputs  # Show connection details"
            echo ""
            echo "Configuration:"
            echo "  Copy terraform.tfvars.example to terraform.tfvars and customize"
            ;;
    esac
}

# Run main function with all arguments
main "$@"
