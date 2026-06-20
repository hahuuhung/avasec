# SysAnti Deployment Guide

This guide describes how to deploy the SysAnti system (Server + UI) on a test machine.

## Prerequisites

- **Docker Desktop**: Required to run the Server and Database.
- **Windows 10/11**: Required to run the Desktop UI.

## 1. Start the Backend (Server & Database)

1. Open PowerShell/Terminal.
2. Navigate to the `deployment` folder (or project root).
3. Run the following command to start the services:

    ```bash
    docker-compose up -d --build
    ```

4. Wait for the containers to start locally.
    - **MySQL** will initialize automatically with the schema in `deploy/mysql-init/`.
    - **Server** will start on port `3001`.
5. Verify the server is running by opening:
    - [http://localhost:3001/api/health](http://localhost:3001/api/health) (Should return `{"status":"UP"...}`)

## 2. Deploy the Desktop Application

1. Run the `publish_release.bat` script in the root directory.
    - This will build a standalone executable in `deploy/client/SysAnti_App`.
2. Copy the entire `SysAnti_App` folder to the target machine.
3. Run `SysAnti.UI.exe` (or `SysAnti_App.exe`).

## 3. Configuration

The client connects to `localhost:3001` by default. To connect to a different server (e.g., if running UI on a different machine than Docker):

1. Open `appsettings.json` in the `SysAnti_App` folder.
2. Update `ApiBaseUrl` and `WebSocketUrl` with the IP of the server machine:

    ```json
    {
      "ApiBaseUrl": "http://192.168.1.100:3001",
      "WebSocketUrl": "http://192.168.1.100:3001"
    }
    ```

## 4. Troubleshooting

- **Server Connection Failed**:
  - Ensure Docker containers are running (`docker ps`).
  - Check if firewall is blocking port 3001.
  - Check `appsettings.json` matches the server IP.
- **Database Error**:
  - Check docker logs: `docker logs avasec-server`.
  - Ensure `deploy/mysql-init` scripts ran successfully (only on first volume creation). If schema is missing, delete the `mysql_data` volume and restart:

        ```bash
        docker-compose down -v
        docker-compose up -d
        ```
