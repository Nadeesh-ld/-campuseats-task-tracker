# Managed PostgreSQL deployment

Keep production credentials in the backend host's environment settings. Do not add a real connection string or JWT key to `appsettings.json`, a `.env` file, or source control. The API requires these settings at startup:

| Environment variable | Value |
| --- | --- |
| `ConnectionStrings__Default` | PostgreSQL connection string from Neon (use SSL; Neon URLs include `sslmode=require`) |
| `Jwt__Key` | Random secret with at least 32 bytes |
| `Jwt__Issuer` | `CampusEats` |
| `Jwt__Audience` | `CampusEatsUsers` |
| `Cors__FrontendOrigin` | Exact deployed frontend origin, such as `https://campuseats-web.vercel.app` |

In Render, create a Docker Web Service with `backend` as the root directory. Add the variables above under **Environment**, save, and deploy. Keep the connection string and JWT key private. For local development, copy `.env.example` to `.env`, fill in your local values, and run `./run-local.ps1` from PowerShell. The script loads `.env` into the API process; ASP.NET Core does not read `.env` files by itself. The real `.env` file is ignored by Git.

## Apply the database migration

Install or use the .NET 8 `dotnet-ef` tool. After filling in the local `.env`, run this from the `backend` directory. It imports the connection string for the command and applies the migration:

```powershell
.\run-local.ps1 -Migrate
```

Run this against the new managed database before relying on the deployed API. The application no longer calls `EnsureCreated`; it expects the EF migration to have created its tables. If the `dotnet ef` command is unavailable, install the .NET 8 EF CLI tool first.

## Frontend

In the Vercel project settings, set `VITE_API_URL` to the deployed API origin (for example, `https://<your-render-service>.onrender.com`) and redeploy. The API's `Cors__FrontendOrigin` must match the frontend origin exactly. Vite embeds `VITE_*` values at build time; never put database credentials or JWT signing keys in frontend variables.

The current frontend is still the Vite starter screen and does not make API requests yet, so setting `VITE_API_URL` alone will not display menu data. Add API calls using this variable as the frontend is implemented.
