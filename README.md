# CandidatAppWeb

A full-stack web application built with a **.NET 10 API** backend and a **Vue 3 + TypeScript** frontend.

---

## 🛠️ Tech Stack

| Layer    | Technology                                      |
|----------|-------------------------------------------------|
| Backend  | .NET 10, ASP.NET Core, Entity Framework Core, PostgreSQL, SignalR, JWT Auth |
| Frontend | Vue 3, TypeScript, Vite, Pinia, Tailwind CSS v4, shadcn-vue |
| Storage  | Cloudinary (media), Neon PostgreSQL (database)  |

---

## 📋 Prerequisites

Make sure the following are installed before proceeding:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (v18+) & npm
- [Git](https://git-scm.com/)
- Access to a PostgreSQL database (e.g. [Neon](https://neon.tech))

---

## 🚀 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/Atoui-Yassine/CandidatAppWeb.git
cd CandidatAppWeb
```

---

## 🔧 Backend Setup (`/api`)

### 2. Navigate to the API folder

```bash
cd api
```

### 3. Configure environment settings

The API uses `appsettings.json` for configuration. Update the following sections with your own values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=<your-db-host>;Port=5432;Database=<db-name>;Username=<user>;Password=<password>;SSL Mode=require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Key": "<your-256-bit-secret-key>",
    "Issuer": "MyAuthApi",
    "Audience": "MyAuthApiUsers"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "My App",
    "SenderEmail": "<your-email>",
    "Username": "<your-email>",
    "Password": "<your-app-password>",
    "EnableSsl": true
  },
  "CloudinarySettings": {
    "CloudName": "<your-cloud-name>",
    "ApiKey": "<your-api-key>",
    "ApiSecret": "<your-api-secret>"
  }
}
```

> **Tip:** Use `appsettings.Development.json` to override settings in development without touching production values.

### 4. Restore dependencies

```bash
dotnet restore
```

### 5. Apply database migrations

Migrations are applied automatically on startup. But you can also run them manually:

```bash
dotnet ef database update
```

### 6. Run the API

```bash
dotnet watch run
```

The API will start at `http://localhost:5259`.  
OpenAPI docs will be available at `http://localhost:5259/openapi/v1.json` (development only).

---

## 🎨 Frontend Setup (`/client`)

### 7. Navigate to the client folder

```bash
cd ../client
```

### 8. Configure environment variables

Create a `.env.development` file (already included) or update the existing one:

```env
VITE_API_URL=http://localhost:5259/api
VITE_APP_TITLE=Candidat App Dev
```

For production, update `.env.production`:

```env
VITE_API_URL=/api
```

### 9. Install dependencies

```bash
npm install
```

### 10. Run the development server

```bash
npm run dev
```

The frontend will be available at `http://localhost:5173`.

---

## 📦 Running Both Together (Full Stack)

Open two terminal windows:

**Terminal 1 — API:**
```bash
cd api
dotnet watch run
```

**Terminal 2 — Client:**
```bash
cd client
npm run dev
```

---

## 🏗️ Build for Production

### Build the frontend

```bash
cd client
npm run build
```

The compiled output goes to `client/dist/` and is automatically served by the .NET API via `wwwroot`.

### Run the API in production mode

```bash
cd api
dotnet run --environment Production
```

---

## 🗂️ Project Structure

```
CandidatAppWeb/
├── api/                        # .NET 10 Backend
│   ├── Controllers/            # API Controllers
│   ├── data/                   # DbContext, Entities, Migrations
│   ├── dtos/                   # Data Transfer Objects
│   ├── Extensions/             # Service registration extensions
│   ├── services/               # Business logic services
│   ├── SignalR/                # Real-time hubs
│   ├── templates/              # Email templates
│   ├── appsettings.json        # Configuration
│   └── Program.cs              # App entry point
└── client/                     # Vue 3 Frontend
    ├── src/
    │   ├── assets/             # Static assets
    │   ├── components/         # Reusable Vue components
    │   ├── composables/        # Vue composables
    │   ├── router/             # Vue Router config
    │   ├── stores/             # Pinia stores
    │   └── views/              # Page-level views
    ├── .env.development        # Dev environment variables
    ├── .env.production         # Prod environment variables
    └── vite.config.ts          # Vite configuration
```

---

## 🔑 Key Features

- 🔐 **JWT Authentication** with ASP.NET Identity
- 📡 **Real-time** notifications via SignalR
- 🖼️ **Media uploads** via Cloudinary
- 🗄️ **PostgreSQL** with Entity Framework Core (auto-migration on startup)
- 🎨 **Dark / Light / System** theme support
- 🧩 **shadcn-vue** component library with Tailwind CSS v4

---

## 📝 Notes

- The database is **auto-migrated** every time the API starts — no manual migrations needed in most cases.
- CORS is configured to allow `http://localhost:5173` (Vite dev server) by default.
- Make sure your PostgreSQL connection string uses **SSL** if connecting to a hosted provider like Neon.