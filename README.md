# WebsiteSellLaptop API

Backend API for website of sell laptop, built with ASP.NET Core and SQL Server.

## Features
- CRUD Product, Brand, Category
- Order & OrderItems
- Dashboard statistics


## Tech Stack
- ASP.NET Core 8
- SQL Server
- Dapper
- Swagger
## Installation
1. Clone repo:
git clone https://github.com/yourname/WebsiteSellLaptop.git

2. Open appsettings.json and change:
- DefaultConnection:
Server=your_server\SQLEXPRESS;Database=your_database;Trusted_Connection=True;TrustServerCertificate=True;

- Add frontend IP to AllowedOrigins

3. Run MainProgram.cs or run command:
dotnet run

4. Open swagger in browser:
http://{server-ip}:{port}/swagger
``