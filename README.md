# Tixcash Federation API

C# REST API service providing federation and data access layer for the Tixcash network.

## Overview

The Federation API acts as a bridge between external clients and the Tixcash blockchain, providing:

- Balance and transaction queries
- Block data retrieval
- Account management endpoints
- Integration with payment processing services

## Tech Stack

- .NET / ASP.NET Core
- SQL Server (via ADO.NET)
- JSON REST API

## Configuration

Set the following in `appsettings.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<your-db-connection-string>"
  }
}
```

## License

See [LICENSE](LICENSE) for details.
