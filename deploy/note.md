I have completed the Deployment preparation.

Deployment Guide: Detailed instructions in DEPLOYMENT_GUIDE.md.
Docker Config: Running docker-compose up -d --build will now fully initialize the database with deploy/mysql-init/ scripts.
UI Build Script: Created publish_release.bat to build a standalone executable folder in deploy/client/SysAnti_App.
You can now:

Run .\publish_release.bat to build the client.
Start the backend with docker-compose up.
Copy the SysAnti_App folder to other machines for testing.
