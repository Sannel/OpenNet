# Authentication Overview

OpenNet supports multiple authentication methods, configured via `appsettings.json` (or environment variables / secrets). OpenID Connect providers are fully configuration-driven. GitHub OAuth uses fixed built-in endpoint URLs because GitHub does not provide OIDC discovery.

## GitHub OAuth

GitHub authentication uses raw OAuth 2.0 and is configured under `Authentication:GitHub`.

```json
{
  "Authentication": {
    "GitHub": {
      "ClientId": "<your-client-id>",
      "ClientSecret": "<your-client-secret>"
    }
  }
}
```

GitHub authentication is active when `ClientId` is non-empty. See [GitHub OAuth documentation](github.md).

## OpenID Connect Providers

Any number of OpenID Connect providers can be added under `Authentication:OpenIdConnect`. Each key becomes the scheme name used internally by ASP.NET Core.

```json
{
  "Authentication": {
    "OpenIdConnect": {
      "Google": {
        "ClientId": "<your-client-id>",
        "ClientSecret": "<your-client-secret>",
        "Authority": "https://accounts.google.com"
      },
      "EntraId": {
        "ClientId": "<your-client-id>",
        "ClientSecret": "<your-client-secret>",
        "Authority": "https://login.microsoftonline.com/<tenant-id>/v2.0"
      }
    }
  }
}
```

A provider is activated when its `ClientId` is non-empty. Multiple providers can be active simultaneously.

### Supported Properties

| Property | Required | Description |
|----------|----------|-------------|
| `ClientId` | Yes | OAuth client ID from your identity provider |
| `ClientSecret` | Yes | OAuth client secret from your identity provider |
| `Authority` | Yes | OIDC discovery base URL for the identity provider |

For provider-specific setup guides, see:

- [Google](google.md)
- [Microsoft Entra ID (Azure AD)](entra-id.md)

## Secrets Management

Never commit real client IDs or secrets to source control. Use one of:

- **User Secrets** (development): `dotnet user-secrets set "Authentication:GitHub:ClientId" "..."`
- **Environment variables**: `Authentication__GitHub__ClientId=...`
- **Azure Key Vault / other secret stores** in production
