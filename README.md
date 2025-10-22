# <img width="50" height="50" alt="BBVFavicon" src="https://github.com/user-attachments/assets/8a0cc63b-5a0e-44f0-9e3c-52fa2bd7e2dd" /> Band Baaja Vivaah

## Project Overview

Band Baaja Vivaah is a wedding planning and management application built with:

  * **Backend**: ASP.NET Core Web API (.NET 8) + Entity Framework Core
  * **Realtime notifications**: gRPC
  * **Business logic**: `BandBaajaVivaah.Services` (shared)
  * **Desktop client**: WPF (.NET 8) with gRPC client for realtime notifications

This repository contains:

  * **`BandBaajaVivaah.WebAPI`** — ASP.NET Core Web API, exposes REST + gRPC services
  * **`BandBaajaVivaah.Services`** — business logic, domain services and server-side gRPC server integrations
  * **`BandBaaajaVivaah.Data`** — EF Core models and DbContext
  * **`BandBaajaVivaah.WPF`** — WPF desktop client (UI + gRPC client)

-----

## Features & Core Capabilities

Band Baaja Vivaah supports full wedding management with guests, tasks, and expenses, plus user management and real-time notifications:

  * **Create / Read / Update / Delete (CRUD) for:**
      * Weddings
      * Guests
      * Tasks
      * Expenses
  * **Authentication & Account Flows:**
      * Register
      * Login (JWT)
      * Change Password
      * Forgot / Reset Password
  * **Role Model:**
      * **Regular User:** Manages their own weddings and all related data (guests, tasks, expenses).
      * **Admin:** Manages all users, can view and modify any wedding/guests/tasks/expenses, and can create/delete data on behalf of users.
  * **Real-time Notifications:**
      * Server-to-client streaming via gRPC for immediate updates on Guests, Tasks, Expenses, and Wedding data.

-----

## How to Use (WPF App — User Flows)

  * **Login / Register:**
      * Use the login form to authenticate. On success, the client receives a JWT which is stored in `ApiClientService` and used for all subsequent REST calls.
      * Register creates a new account via the `AuthController`.
      * Forgot Password sends a reset token; Reset Password applies the new password.
      * Login Screen
      <table>
        <thead>
          <tr>
            <th align="center">Before adding details:</th>
            <th align="center">After adding details (hidden):</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>
              <img width="400" height="400" alt="image" src="https://github.com/user-attachments/assets/dd15ff62-a93b-41f9-9231-162a2d18a1da" />
            </td>
            <td>
              <img width="400" height="400" alt="image" src="https://github.com/user-attachments/assets/7a38deac-de45-4896-81fe-3121c8c6fd22" />
            </td>
          </tr>
        </tbody>
      </table>
      * Register & Forgot Password Screen:-
            <table>
        <thead>
          <tr>
            <th align="center">Register:</th>
            <th align="center">Forgot Password:</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>
              <img  width="400" height="400" alt="image" src="https://github.com/user-attachments/assets/22aab3b5-6a5c-4d19-85cf-fb802055924b" />
            </td>
            <td>
             <img width="400" height="400" alt="image" src="https://github.com/user-attachments/assets/0c85496d-0237-4f83-bcca-44920a6fd38b" />
            </td>
          </tr>
        </tbody>
        </table>

     * Appropriate messages of success/failures are displayed as well.
   
     * Login Screens Admin and User:-
            <table>
        <thead>
          <tr>
            <th align="center">Admin Login:</th>
            <th align="center">User Login:</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>
              <img width="997" height="562" alt="image" src="https://github.com/user-attachments/assets/8ba62d9d-d3c4-442d-9ebd-6eeac8b46627" />
            </td>
            <td>
             <img width="995" height="560" alt="image" src="https://github.com/user-attachments/assets/f58e9f09-4a45-4733-b2c3-0292e6053311" />
            </td>
          </tr>
        </tbody>
        </table>


  * **Weddings List:**
      * Displays all weddings associated with the authenticated user (or a target user (from admin portal) if in admin mode).
      * **Create:** Click "Add" → fill out the form → POST to API.
      * **Edit:** Double-click a wedding row → opens the Add/Edit view → PUT to API.
      * **Delete:** Select a wedding → confirm → DELETE to API.
      * Proper data validations are in place with red marks for validation errors.
      * The 3 buttons under actions depict Manage Guests, Manage Tasks and Manage Expenses.
      * <img width="150" height="150" alt="image" src="https://github.com/user-attachments/assets/626f5ee9-acf5-4cc4-9f1d-917fbbf661b8" />
      * Weddings list screenshots:-
     <table>
        <thead>
          <tr>
            <th align="center">Weddings List Page</th>
            <th align="center">Adding a wedding</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>
              <img width="997" height="562" alt="image" src="https://github.com/user-attachments/assets/515cba1c-5d96-4600-a122-c0492a99c60e" />
            </td>
            <td>
              <img width="997" height="562" alt="image" src="https://github.com/user-attachments/assets/d588357b-160d-4131-89ce-f8290271e859" />
            </td>
          </tr>
        </tbody>
      </table>
      
    <table>
        <thead>
          <tr>
            <th align="center">Validation error while saving</th>
            <th align="center">Pressing delete after adding wedding and selecting it</th>
          </tr>
        </thead>
        <tbody>
          <tr>
             <td>
              <img width="997" height="562" alt="image" src="https://github.com/user-attachments/assets/9508f02a-fffc-48a7-b1ad-190f120d65fa" />
            </td>
            <td>
             <img width="997" height="562" alt="image" src="https://github.com/user-attachments/assets/8e630e18-46ad-41a5-a6ee-43f905aeeea6" />
        </td>
          </tr>
        </tbody>
      </table>

    <table>
        <thead>
          <tr>
            <th align="center">gRPC notification on delete</th>
            <th align="center">Final state (additionally - night mode toggled on) </th>
          </tr>
        </thead>
        <tbody>
          <tr>
             <td>
             <img width="997" height="561" alt="image" src="https://github.com/user-attachments/assets/22f984a8-7a22-4831-a34e-d5b76ca9bb2a" />
            </td>
            <td>
             <img width="997" height="561" alt="image" src="https://github.com/user-attachments/assets/7522f70e-aa6b-4f43-8c34-d60851585553" />
        </td>
          </tr>
        </tbody>
      </table>
  * **Guests / Tasks / Expenses (Per Wedding):**
      * Open a wedding → navigate to the Guests, Tasks, or Expenses pages.
      * **Add:** Open the form → POST to the corresponding endpoint.
      * **Edit:** Select an item → open the form → PUT.
      * **Delete:** Select an item → confirm → DELETE.
      * Proper data validations are in place with red marks for validation errors.
      * The UI automatically refreshes the list after operations and also responds to real-time server notifications.
      * Guests, tasks and expenses screenshots added.
          <table>
        <thead>
          <tr>
            <th align="center">Guests List Page</th>
            <th align="center">Add Guests Page</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>
              <img width="997" height="561" alt="image" src="https://github.com/user-attachments/assets/298299e9-e70d-4942-bd2a-705e1d80372a" />
            </td>
            <td>
              <img width="997" height="560" alt="image" src="https://github.com/user-attachments/assets/11635cde-c8db-4d8c-8bd0-b87f53cdb113" />
            </td>
          </tr>
        </tbody>
      </table>

       <table>
        <thead>
          <tr>
            <th align="center">Tasks List Page</th>
            <th align="center">Add Tasks Page</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>
              <img width="997" height="560" alt="image" src="https://github.com/user-attachments/assets/bcd8f659-4014-401e-8466-1c64cb80da48" />
            </td>
            <td>
              <img width="997" height="560" alt="image" src="https://github.com/user-attachments/assets/611fccd0-04f1-46ce-a9c1-c81ec04b85a9" />
            </td>
          </tr>
        </tbody>
      </table>

    <table>
        <thead>
          <tr>
            <th align="center">Expenses List Page</th>
            <th align="center">Add Expenses Page</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>
             <img width="997" height="560" alt="image" src="https://github.com/user-attachments/assets/f9dda2dd-03a1-4ede-b693-f75f177599dd" />
            </td>
            <td>
              <img width="997" height="560" alt="image" src="https://github.com/user-attachments/assets/0c069c42-e99f-4da7-9100-5d4a2182381b" />
            </td>
          </tr>
        </tbody>
      </table>

-----

## Admin Capabilities

  * Admin users (those with the `Admin` role) can:
      * View all users (via `AdminController`) and manage their roles.
      * View and modify any user’s weddings and their related Guests, Tasks, and Expenses.
      * Create, update, or delete resources on behalf of other users.
  * **Admin Mode:** The WPF app supports an "admin view" where an admin can choose a target user and perform operations as that user.
  * Logged in as an admin. Clicking on any icon under Actions will take the admin to weddings list page for that corresponding user.
      <table>
        <thead>
          <tr>
            <th align="center">User management page (on clicking admin portal):</th>
            <th align="center">Role Change by admin (double clicking on a row):</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>
              <img  width="997" height="562" alt="Screenshot 2025-10-21 211507" src="https://github.com/user-attachments/assets/99053719-1001-4bd1-922b-3ac8eae0ac81" />
            </td>
            <td>
              <img width="997" height="562" alt="Screenshot 2025-10-21 211830_roleedit" src="https://github.com/user-attachments/assets/99debfcf-e705-41c7-aae4-0e7d0cb887c0" />
            </td>
          </tr>
        </tbody>
      </table>
-----

## API & Server Architecture (Important Files)

  * **Authentication:** `AuthController` — Handles register, login (JWT), forgot/reset password.
  * **REST Controllers:**
      * `GuestController`, `TaskController`, `ExpenseController`, `WeddingController`
  * **Business Logic:** `BandBaajaVivaah.Services` (contains services like `GuestService`, `TaskService`, `ExpenseService`, `WeddingService`)
  * **gRPC Services (Server):** `GuestUpdateGrpcService`, `TaskUpdateGrpcService`, `ExpenseUpdateGrpcService`, `WeddingUpdateGrpcService`
  * **gRPC Clients (WPF):** `GuestUpdateService`, `TaskUpdateService`, `ExpenseUpdateService`, `WeddingUpdateService`
  * **Client API Layer:** `BandBaajaVivaah.WPF\Services\ApiClientService.cs`

-----

## Quick notes

  * **Local DB**: SQL Server (LocalDB / Developer SQL Server)
  * **Production DB**: Azure SQL
  * **Production hosting**: Azure App Service (WebAPI); App Service supports gRPC when configured for HTTP/2 + Kestrel configured to accept HTTP/1.1+HTTP/2
  * gRPC notifications require HTTP/2 end-to-end; server and client must be configured accordingly

-----

## Prerequisites

  * .NET 8 SDK
  * Visual Studio 2022 (or VS Code + appropriate extensions)
  * SQL Server LocalDB / SQL Server / Azure SQL
  * Azure subscription (for deployment)
  * Optional: `dotnet-ef` tools (for running migrations locally)

-----

## Local setup (developer machine)

1.  **Clone repository**

    ```bash
    git clone https://github.com/yash-saini/BandBaajaVivaah.git
    ```

2.  **Open solution in Visual Studio 2022.**

3.  **Configure local DB connection:**

      * Edit `BandBaajaVivaah.WebAPI\appsettings.json`
      * Confirm `ConnectionStrings:DefaultConnection` points to your local SQL Server / LocalDB instance.
        Example:
        ```json
        "ConnectionStrings": {
          "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BandBaajaVivaahDb;Trusted_Connection=True;MultipleActiveResultSets=true"
        }
        ```

4.  **Apply EF Core migrations (Package Manager Console or CLI)**

      * In Visual Studio open **Package Manager Console** OR use CLI.
      * Using PMC:
        ```powershell
        PM> Add-Migration InitialCreate -Project BandBaaajaVivaah.Data -StartupProject BandBaajaVivaah.WebAPI
        PM> Update-Database -Project BandBaaajaVivaah.Data -StartupProject BandBaajaVivaah.WebAPI
        ```
      * Or using `dotnet-ef` CLI:
        ```bash
        dotnet ef database update --project BandBaaajaVivaah.Data --startup-project BandBaajaVivaah.WebAPI
        ```

5.  **Run API locally:**

      * Set `BandBaajaVivaah.WebAPI` as startup project and run (F5). API provides REST endpoints and gRPC services.

6.  **Run WPF client:**

      * Set `BandBaajaVivaah.WPF` as startup project and run (F5). Default local API address expected: `https://localhost:7159` (update in `App.xaml.cs` if different).

-----

## Production deployment (Azure)

High-level steps to host Web API and DB on Azure and enable gRPC notifications:

### A. Database (Azure SQL)

1.  Create Azure SQL Database and logical server.
2.  Add database firewall rule: allow your client IP and enable "Allow Azure services and resources to access this server" if App Service needs DB access.
3.  Obtain the connection string. Example:
    ```
    Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=BandBaajaVivaahDb;Persist Security Info=False;User ID=youruser;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
    ```
4.  Migrate schema to Azure:
      * **Option A (recommended)**: run migrations from CI/CD or run `dotnet ef database update` using the production connection string.
      * **Option B**: add automatic migration code in startup, but prefer migrations applied in controlled deployment.

### B. App Service (Web API)

1.  Create an App Service (Linux or Windows). Use 64-bit platform.
2.  In App Service -\> **Configuration** -\> **General settings**:
      * Set **HTTP Version** to 2.0 (enables HTTP/2)
      * **Platform**: 64-bit
      * **Always On**: On (recommended for persistent gRPC connections)
3.  In App Service -\> **Configuration** -\> **Application settings** add the following keys (use double underscore `__` for nested config keys in Azure UI):
      * `Jwt__Key` = \<your strong secret\>
      * `Jwt__Issuer` = https://\<your-app-service-hostname\>
      * `Jwt__Audience` = https://\<your-app-service-hostname\>
      * `ASPNETCORE_ENVIRONMENT` = Production
      * (Either create a Connection String named "DefaultConnection" in Connection Strings section, or add) `ConnectionStrings__DefaultConnection` = \<your azure sql connection string\>
      * `SmtpSettings__Host`, `SmtpSettings__Port`, `SmtpSettings__Username`, `SmtpSettings__Password` (if required)

### C. Kestrel / HTTP/2 configuration

  * The WebAPI project is configured to allow both HTTP/1.1 and HTTP/2 in `Program.cs`:
    ```csharp
    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    ```
  * This allows REST endpoints (HTTP/1.1) and gRPC (HTTP/2) to work side-by-side.

### D. Deployment:

  * Publish `BandBaajaVivaah.WebAPI` to the App Service via Visual Studio publish profile, GitHub Actions, or Azure CLI.
  * Restart App Service after configuration changes.

-----

## Real-time Notifications (gRPC)

  * **Architecture:** The server exposes streaming gRPC endpoints (e.g., `SubscribeToGuestUpdates`, `SubscribeToTaskUpdates`).
  * **Client Flow:** The WPF clients open long-lived gRPC streams after login and call these subscription methods (e.g., `GuestUpdateService.SubscribeToWeddingUpdates(weddingId)`).
  * **Data Flow:** When server-side services call a notification method (e.g., `NotifyGuestChange(...)`), the server writes an `*UpdateEvent` to all connected subscribers for that wedding. The clients receive these events and update the UI.
  * **Client-side:** Channels must be kept alive while the app is in use. Call `Unsubscribe()` or dispose of the channels on logout/exit.
  * **Important Production Notes:**
      * App Service and Kestrel must support HTTP/2. The server is configured for `HttpProtocols.Http1AndHttp2` to allow REST and gRPC side-by-side.
      * Enable **Always On** in the Azure App Service to maintain the long-lived gRPC streams required for notifications.

-----

## Security & Configuration

  * **JWT (Authentication):**
      * The token is stored and used by `ApiClientService` and sent with each API request as `Authorization: Bearer <token>`.
      * In production, you **must** configure `Jwt__Key`, `Jwt__Issuer`, and `Jwt__Audience` in the Azure App Service application settings.
  * **Database:**
      * **Local:** Uses SQL Server / LocalDB via the `appsettings.json` connection string.
      * **Production:** Uses Azure SQL. Set the connection string using the `ConnectionStrings__DefaultConnection` key in App Service settings.
  * **Secrets:**
      * **Do not commit secrets** (`appsettings.json` keys, passwords) to source control.
      * Use Azure App Service settings or Azure Key Vault for all production secrets.
  * **Recommended App Service Settings:**
      * **HTTP Version:** 2.0
      * **Platform:** 64 Bit
      * **Always On:** On (Critical for gRPC)

-----

## Troubleshooting

1.  **gRPC notifications not received (Real-time updates fail):**

      * Verify the WPF client called the `SubscribeTo...` method after login and the subscription is active.
      * Check server logs for "Added subscriber" messages in the respective `*UpdateGrpcService`.
      * Confirm the Azure App Service has **Always On** enabled and **HTTP Version** is set to 2.0.
      * Confirm network/firewall allows outbound HTTP/2 from the client to your App Service.

2.  **Authentication failures (Unauthorized / 401):**

      * Confirm `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience` and the `ConnectionStrings__DefaultConnection` are correctly set in the App Service application settings.
      * Check the Web API logs (via App Service **Log stream**) and any health check endpoints to validate configuration and DB connectivity.

3.  **"An HTTP/1.x request was sent to an HTTP/2 only endpoint"**

      * **Fix:** Ensure Kestrel is configured for `HttpProtocols.Http1AndHttp2` (it is in `Program.cs`) and that the App Service **HTTP Version** is set to 2.0.

4.  **Database empty in production:**

      * Ensure you have run the EF Core migrations against the production database.
      * Verify database firewall rules and credentials.

-----

## CI / Deployment suggestions

  * Use GitHub Actions or Azure Pipelines to:
      * Run tests and build
      * Apply EF migrations to production DB (careful, prefer controlled deployment)
      * Publish API to App Service
  * Do not put production secrets in pipeline YAML; use Azure Key Vault or App Service settings.

-----

## Notes for Developers

  * **Client-side Subscription Pattern:**
      * When adding new UI pages/view models that need real-time data:
        1.  Inject the matching `*UpdateService` (e.g., `GuestUpdateService`).
        2.  On page load, call `await guestUpdateService.SubscribeToWeddingUpdates(weddingId);` (preferably in the background).
        3.  Listen to the service's C\# event (e.g., `guestUpdateService.OnGuestUpdate`).
        4.  When the event fires, update your local `ObservableCollection` or UI properties. Remember to use `Dispatcher.Invoke` to move back to the UI thread.
        5.  Call `Unsubscribe()` and dispose of the service/channel when the page unloads or on user logout.
  * **Server-side Notification Pattern:**
      * Notification helper methods (e.g., `GuestUpdateGrpcService.NotifyGuestChange(...)`) live in each gRPC service.
      * The main business services (e.g., `GuestService`) call these gRPC helper methods *after* the database changes have been successfully committed.

-----

## Additional references

  * [gRPC in ASP.NET Core](https://docs.microsoft.com/aspnet/core/grpc)
  * [gRPC client in .NET](https://docs.microsoft.com/dotnet/architecture/grpc-for-wcf-developers)
  * [Deploy ASP.NET Core to Azure App Service](https://docs.microsoft.com/azure/app-service/quickstart-dotnetcore)

-----

## Contact / Maintainer

Project maintainer: Yash Saini
Repository: [https://github.com/yash-saini/BandBaajaVivaah](https://github.com/yash-saini/BandBaajaVivaah)

-----

## License

MIT — See LICENSE file in repository.
