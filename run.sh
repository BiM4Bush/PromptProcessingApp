#!/bin/bash

echo "======================================================="
echo "   Starting Prompt Processing"
echo "======================================================="
echo ""

open_url() {
    local URL=$1
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        xdg-open "$URL" >/dev/null 2>&1
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        open "$URL" >/dev/null 2>&1
    elif [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
        # Dla Git Bash na Windowsie
        start "$URL" >/dev/null 2>&1
    fi
}

cleanup() {
    echo ""
    echo "Stopping processes..."
    kill $FRONTEND_PID $BACKEND_PID 2>/dev/null
    exit
}
trap cleanup SIGINT

echo "[1/2] Running frontend (React + Vite)..."
cd prompt-processing-ui
npm run dev > vite.log 2>&1 &
FRONTEND_PID=$!
cd ..

open_url "http://localhost:5173"

echo "[2/2] Running backend (.NET Aspire)..."
PIPE="/tmp/aspire_pipe_$$"
mkfifo "$PIPE"

dotnet run --project PromptProcessing.AppHost/PromptProcessing.AppHost.csproj > "$PIPE" 2>&1 &
BACKEND_PID=$!

echo ""
echo "The Prompt Processing App started successfully!"
echo "-------------------------------------------------------"
echo "Frontend UI:        http://localhost:5173"
echo "Press [Ctrl + C], to safe kill the process."
echo "-------------------------------------------------------"
echo ""

(
    while IFS= read -r line; do
        echo "$line"
        
        if [[ "$line" == *"Login to the dashboard at"* ]]; then
            DASHBOARD_URL=$(echo "$line" | grep -o 'https://localhost:[0-9]*/login?t=[a-zA-Z0-9]*')
            
            if [ ! -z "$DASHBOARD_URL" ]; then
                echo -e "\n[INFO] Opening Aspire Dashboard in browser..."
                open_url "$DASHBOARD_URL"
            fi
        fi
    done < "$PIPE"
) &
READER_PID=$!

wait $BACKEND_PID $FRONTEND_PID