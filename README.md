# SecureVault

A web-based file encryption service built with ASP.NET Core Blazor that provides secure file storage with support for multiple encryption algorithms.

## Features

- **Multiple encryption algorithms** — AES-256-GCM, RSA-2048 (hybrid), DES, Triple DES, and Elliptic Curve Cryptography
- **Secure key management** — Per-file encryption keys wrapped with a master key using AES-256-GCM
- **Cloud file storage** — Encrypted files stored in AWS S3 with server-side AES-256 encryption
- **User authentication** — ASP.NET Identity with strong password policies and account lockout
- **Audit logging** — Full trail of encrypt, decrypt, and delete operations
- **Interactive UI** — Blazor server-side rendering with file upload, algorithm selection, and progress feedback

## Tech Stack

- .NET 10.0 / ASP.NET Core / Blazor
- PostgreSQL with Entity Framework Core
- BouncyCastle for asymmetric cryptography
- AWS S3 for file storage
- Docker / Heroku for deployment
- xUnit / Moq for testing

## Project Structure

```
src/SecureVault.Web/        # Main web application
  Components/Pages/         # Blazor pages (Encrypt, Decrypt, Dashboard, AuditLog, Account)
  Components/Shared/        # Reusable UI components
  Data/Models/              # EF Core entities (EncryptedFile, EncryptionKeyRecord, AuditLogEntry)
  Services/Encryption/      # Encryption service implementations & factory
  Services/                 # Key management, audit logging, S3 storage
tests/SecureVault.Tests/    # Unit tests for encryption services & key management
myencrypter2/               # Legacy Windows Forms desktop client
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL

### Environment Variables

| Variable | Description |
|---|---|
| `ENCRYPTION_MASTER_KEY` | Base64-encoded 32-byte master key |
| `DATABASE_URL` | PostgreSQL connection string |
| `AWS_ACCESS_KEY_ID` | AWS credentials (optional if using IAM roles) |
| `AWS_SECRET_ACCESS_KEY` | AWS credentials (optional if using IAM roles) |
| `AWS_REGION` | AWS region (default: `us-east-1`) |
| `AWS_S3_BUCKET` | S3 bucket name (default: `securevault-files`) |

### Run Locally

```bash
dotnet restore
dotnet run --project src/SecureVault.Web/SecureVault.Web.csproj
```

The app will be available at `https://localhost:7095`.

### Run with Docker

```bash
docker build -t securevault .
docker run -p 8080:8080 \
  -e ENCRYPTION_MASTER_KEY="your-base64-key" \
  -e DATABASE_URL="your-postgres-url" \
  securevault
```

### Deploy to Heroku

The repo includes a `heroku.yml` for container-based deployment:

```bash
heroku create
heroku stack:set container
git push heroku main
```

## Running Tests

```bash
dotnet test
```

Tests cover encryption round-trips, wrong-key failures, key management, and the encryption service factory.

## License

[MIT](LICENSE.txt)
