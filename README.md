    -----Name-----                 ----ID-----
    1.Bantalem Mitku    -  -  -  - - 1501020
    2.Mihret Abebe      -  -  -  - - 1501336
    3.Firehiwot Tesfaye -  -  -  - - 1501207
    4.Zewdu Werede      -  -  -  - - 1501601
    5.Elefachew Fetene  -  -  -  - - 1501148       



    ## Our Project Window Form Do not work properly  We Try again and again just update A little Bit .Sorry !!! 


Library Management System
This project implements a basic Library Management System with a Web API backend and a Windows Forms (WinForms) desktop client. It allows for managing books, borrowers, and loans, with user authentication and authorization.

✨ Features
User Management: Register and Login users with secure authentication.

Book Management: Add, view, edit, and delete books.

Borrower Management: Add, view, edit, and delete borrower records.

Loan Management: Issue new loans and return books, track current and overdue loans.

API-driven: All client-server communication is handled via a RESTful API.

Swagger/OpenAPI: API documentation and testing interface.

🚀 Technologies Used
Backend (LibraryWebAPI):

C# / .NET 8.0

ASP.NET Core Web API

Entity Framework Core (EF Core) for data access

SQL Server LocalDB for database

ASP.NET Core Identity for user authentication and authorization

JWT (JSON Web Tokens) for API authentication

Swagger/Swashbuckle for API documentation

AutoMapper for DTO to Model mapping

Custom Middleware for Exception Handling and Logging (if implemented)

Client (LibraryClient):

C# / .NET 8.0 (Windows Forms)

HttpClient for interacting with the API

System.Text.Json for JSON serialization/deserialization

Shared Library (LibraryShared):

C# / .NET 8.0

Contains Data Transfer Objects (DTOs) used by both the API and the client for consistent data exchange.

🛠️ Prerequisites
Before you begin, ensure you have the following installed:

Visual Studio 2022 (Community, Professional, or Enterprise editions) with the following workloads:

.NET desktop development

ASP.NET and web development

Data storage and processing

.NET 8.0 SDK

SQL Server LocalDB (usually installed with Visual Studio's "Data storage and processing" workload)

📦 Getting Started
Follow these steps to get the project up and running on your local machine.

1. Clone or Download the Repository
If using Git:

git clone <repository_url>
cd LibrarySystem

If downloading, extract the ZIP file to your desired location (e.g., C:\Users\YourUsername\source\repos\).

2. Open the Solution in Visual Studio
Navigate to the cloned/extracted folder (LibrarySystem).

Double-click the LibrarySystem.sln file to open the solution in Visual Studio.

3. Verify Project Target Frameworks
Ensure all projects (LibraryWebAPI, LibraryClient, LibraryShared) are targeting .NET 8.0.

In Visual Studio's Solution Explorer, right-click on each project.

Select "Properties".

Under "Application" (or "Target framework"), confirm it's set to:

LibraryWebAPI: .NET 8.0

LibraryClient: .NET 8.0 (Windows)

LibraryShared: .NET 8.0

Save any changes.

4. Update NuGet Packages (Critical for consistency)
In Visual Studio, go to Tools > NuGet Package Manager > Manage NuGet Packages for Solution....

Go to the "Updates" tab and update all available packages to their latest compatible versions (ideally 8.0.x).

Go to the "Consolidate" tab to ensure all projects use the same package versions for common packages (especially Microsoft.EntityFrameworkCore.* and System.Text.Json).

5. Clean and Rebuild the Solution
In Visual Studio, go to Build > Clean Solution.

Then, go to Build > Rebuild Solution.

Troubleshooting: If you encounter file locked errors during rebuild, ensure all previous instances of the application are closed. Use Task Manager (Ctrl+Shift+Esc), go to the "Details" tab, find LibraryClient.exe (and any other project .exe), right-click, and select "End task". Then Clean and Rebuild again.

6. Database Setup & Migrations (Crucial for Identity Tables)
This step ensures your SQL Server LocalDB database has the correct schema for ASP.NET Core Identity (including the FullName column) and your application models.

Close all instances of Visual Studio and any SQL Server Management Studio (SSMS) windows that might be connected to (localdb)\MSSQLLocalDB.

Delete the existing database (if any):

Open Visual Studio.

Go to View > SQL Server Object Explorer.

Expand (localdb)\MSSQLLocalDB > Databases.

Find LibraryDB. Right-click on it and select "Delete". Check "Close existing connections" and click "OK". This ensures a clean database creation.

Delete existing migration files:

In Solution Explorer, navigate to the LibraryWebAPI project > Migrations folder.

Delete ALL .cs files inside the Migrations folder. Keep the Migrations folder itself.

Open Package Manager Console: Go to Tools > NuGet Package Manager > Package Manager Console.

Set LibraryWebAPI as the Default Project in the console's dropdown.

Create a new initial migration:

Add-Migration InitialIdentitySetup

A new .cs file will be created in the Migrations folder. Inspect it to confirm it adds AspNetUsers (with FullName), AspNetRoles, etc., and your other application tables.

Apply the migration to the database:

Update-Database

This will create your LibraryDB database with all the necessary tables. Monitor the console for success messages.

7. Run the API Backend
In Visual Studio's Solution Explorer, right-click on the LibraryWebAPI project.

Select "Debug" > "Start New Instance" (or press Ctrl+F5).

A console window will open for the API, and your default browser should launch with the Swagger UI (at https://localhost:7053/swagger/).

Confirm the API console shows "Application started." and "Now listening on: https://localhost:7053". Keep this window open and the API running.

8. Run the Windows Forms Client
In Visual Studio's Solution Explorer, right-click on the LibraryClient project.

Select "Debug" > "Start New Instance" (or press F5).

The "Library Management System" Windows Forms application will open.

9. Register and Log In
In the Client Application, you will be on the "Login" tab.

Click the "Register here" link (typically below the login button).

In the "Register New User" dialog:

Enter a Username, Full Name, and Email.

Enter a Password that meets the API's requirements (from Program.cs): at least 6 characters, one digit, one non-alphanumeric, one uppercase, one lowercase. Example: MyP@ssw0rd1

Confirm the password.

Click "Register".

You should see a "Registration successful" message box.

Close the registration dialog.

Back on the "Login" tab:

Enter the Username and Password you just registered.

Click "Login".

You should see a "Login successful!" message box and the application should switch to the "Books" tab, attempting to load data.

10. Test API Endpoints (via Swagger UI)
While the API is running, you can also test endpoints directly using Swagger:

Go to https://localhost:7053/swagger/ in your browser.

To test protected endpoints (like /books):

Expand Auth > POST /api/Auth/login.

Click "Try it out", enter a valid username/password, and "Execute".

Copy the Token value from the Response body.

Click the green "Authorize" button at the top of the Swagger UI.

In the dialog, paste Bearer <YOUR_TOKEN_HERE> (replace <YOUR_TOKEN_HERE> with your copied token).

Click "Authorize" and then "Close".

Now, try the GET /api/Books endpoint. It should return data (or an empty array) instead of a 401 Unauthorized error.

📂 Project Structure
LibrarySystem.sln: The main Visual Studio solution file.

LibraryWebAPI: The ASP.NET Core Web API project. Contains controllers, services, data context, models, and API startup configuration.

LibraryClient: The Windows Forms desktop client project. Contains the UI logic for interacting with the API.

LibraryShared: A .NET class library project that holds shared models, DTOs (Data Transfer Objects), and potentially interfaces used by both the API and the client.


