#!/bin/bash

# NotiBlock Docker Quick Start Script
# This script sets up and starts the entire NotiBlock stack with one command

set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMMAND="${1:-up}"
BUILD=false
DETACH=false

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Helper functions
write_header() {
    echo -e "\n${CYAN}=== $1 ===${NC}"
}

write_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

write_error() {
    echo -e "${RED}✗ $1${NC}"
}

write_info() {
    echo -e "${YELLOW}ℹ $1${NC}"
}

# Check if Docker is installed and running
test_docker() {
    if ! command -v docker &> /dev/null; then
        return 1
    fi
    
    if ! docker ps &> /dev/null; then
        return 1
    fi
    
    return 0
}

# Check if .env file exists
test_env_file() {
    [ -f "$PROJECT_ROOT/.env" ]
}

# Create .env from template
initialize_env() {
    if ! test_env_file; then
        write_header "Initializing Environment"
        
        if [ -f "$PROJECT_ROOT/.env.example" ]; then
            cp "$PROJECT_ROOT/.env.example" "$PROJECT_ROOT/.env"
            write_success ".env file created from template"
            write_info "Edit .env file with your configuration values"
            write_info "Required: DB_PASSWORD, JWT_KEY"
            
            # Try to open in default editor
            if command -v nano &> /dev/null; then
                read -p "Press Enter to edit .env file (or Ctrl+C to skip): "
                nano "$PROJECT_ROOT/.env"
            fi
        else
            write_error ".env.example not found!"
            exit 1
        fi
    else
        write_success ".env file exists"
    fi
}

start_stack() {
    write_header "Starting NotiBlock Stack"
    
    local args=("up" "-d")
    if [ "$BUILD" = true ]; then
        args+=("--build")
        write_info "Rebuilding images..."
    fi
    
    if docker-compose -f "$PROJECT_ROOT/docker-compose.yml" "${args[@]}"; then
        write_success "Stack started successfully"
        show_status
    else
        write_error "Failed to start stack"
        exit 1
    fi
}

stop_stack() {
    write_header "Stopping NotiBlock Stack"
    
    if docker-compose -f "$PROJECT_ROOT/docker-compose.yml" down; then
        write_success "Stack stopped"
    else
        write_error "Failed to stop stack"
    fi
}

show_status() {
    write_header "Service Status"
    docker-compose -f "$PROJECT_ROOT/docker-compose.yml" ps
    
    write_header "Access Points"
    echo -e "${GREEN}Frontend:      http://localhost${NC}"
    echo -e "${GREEN}Backend API:   http://localhost:5271${NC}"
    echo -e "${GREEN}Health Check:  http://localhost:5271/api/auth/health${NC}"
    echo -e "${GREEN}Database:      localhost:5432${NC}"
}

show_logs() {
    write_header "Streaming Logs (Ctrl+C to exit)"
    docker-compose -f "$PROJECT_ROOT/docker-compose.yml" logs -f --tail=50
}

restart_stack() {
    write_header "Restarting NotiBlock Stack"
    
    if docker-compose -f "$PROJECT_ROOT/docker-compose.yml" restart; then
        write_success "Stack restarted"
        sleep 2
        show_status
    else
        write_error "Failed to restart stack"
    fi
}

clean_stack() {
    write_header "Cleaning NotiBlock Stack"
    write_info "This will remove all containers and data."
    read -p "Type 'yes' to confirm: " confirm
    
    if [ "$confirm" = "yes" ]; then
        docker-compose -f "$PROJECT_ROOT/docker-compose.yml" down -v
        write_success "Stack cleaned"
    else
        write_info "Cancelled"
    fi
}

show_help() {
    cat << EOF
Usage: ./start-docker.sh [COMMAND] [OPTIONS]

Commands:
    up       Start the stack (default)
    down     Stop the stack
    logs     Show live logs
    status   Show service status
    restart  Restart all services
    clean    Remove containers and data

Options:
    --build  Rebuild images before starting
    --help   Show this help message

Examples:
    ./start-docker.sh up
    ./start-docker.sh up --build
    ./start-docker.sh logs
    ./start-docker.sh down
EOF
}

# Parse command-line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        up)
            COMMAND="up"
            shift
            ;;
        down)
            COMMAND="down"
            shift
            ;;
        logs)
            COMMAND="logs"
            shift
            ;;
        status)
            COMMAND="status"
            shift
            ;;
        restart)
            COMMAND="restart"
            shift
            ;;
        clean)
            COMMAND="clean"
            shift
            ;;
        --build)
            BUILD=true
            shift
            ;;
        --help)
            show_help
            exit 0
            ;;
        *)
            write_error "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Main logic
if ! test_docker; then
    write_error "Docker is not installed or not running!"
    write_info "Install Docker: https://docs.docker.com/engine/install/"
    exit 1
fi

write_header "NotiBlock Docker Manager"

initialize_env

case "$COMMAND" in
    up)
        start_stack
        ;;
    down)
        stop_stack
        ;;
    logs)
        show_logs
        ;;
    status)
        show_status
        ;;
    restart)
        restart_stack
        ;;
    clean)
        clean_stack
        ;;
    *)
        write_error "Unknown command: $COMMAND"
        show_help
        exit 1
        ;;
esac

echo ""
