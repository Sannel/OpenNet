# Microsoft Entra ID (Azure AD)

OpenNet supports sign-in with Microsoft Entra ID (formerly Azure Active Directory) via OpenID Connect.

## 1. Register an Application

1. Open the [Azure portal](https://portal.azure.com/) and navigate to **Microsoft Entra ID → App registrations → New registration**.
2. Fill in:
   - **Name**: OpenNet (or your preferred name)
   - **Supported account types**: choose based on your requirements (single tenant, multi-tenant, etc.)
   - **Redirect URI**: `https://<your-domain>/signin-oidc` (platform: **Web**)
3. Click **Register**.
4. Copy the **Application (client) ID** and **Directory (tenant) ID**.
5. Under **Certificates & secrets**, create a new **Client secret** and copy its value.

## 2. Configure OpenNet

Add the provider under `Authentication:OpenIdConnect`. The key (`EntraId` below) becomes the ASP.NET Core scheme name — you may choose any name.

Replace `<tenant-id>` with your Directory (tenant) ID from step 4.

```json
{
  "Authentication": {
    "OpenIdConnect": {
      "EntraId": {
        "ClientId": "<your-client-id>",
        "ClientSecret": "<your-client-secret>",
        "Authority": "https://login.microsoftonline.com/<tenant-id>/v2.0"
      }
    }
  }
}
```

**User Secrets (recommended for development):**

```bash
dotnet user-secrets set "Authentication:OpenIdConnect:EntraId:ClientId" "<your-client-id>"
dotnet user-secrets set "Authentication:OpenIdConnect:EntraId:ClientSecret" "<your-client-secret>"
dotnet user-secrets set "Authentication:OpenIdConnect:EntraId:Authority" "https://login.microsoftonline.com/<tenant-id>/v2.0"
```

**Environment variables:**

```
Authentication__OpenIdConnect__EntraId__ClientId=<your-client-id>
Authentication__OpenIdConnect__EntraId__ClientSecret=<your-client-secret>
Authentication__OpenIdConnect__EntraId__Authority=https://login.microsoftonline.com/<tenant-id>/v2.0
```

## Common Authority URLs

| Scenario | Authority |
|----------|-----------|
| Single tenant | `https://login.microsoftonline.com/<tenant-id>/v2.0` |
| Multi-tenant (work/school accounts) | `https://login.microsoftonline.com/organizations/v2.0` |
| Multi-tenant + personal accounts | `https://login.microsoftonline.com/common/v2.0` |
| Personal Microsoft accounts only | `https://login.microsoftonline.com/consumers/v2.0` |

## Notes

- ASP.NET Core automatically discovers the OIDC metadata from `{Authority}/.well-known/openid-configuration`.
- For multi-tenant apps, set `TokenValidationParameters.ValidateIssuer = false` or configure `ValidIssuers` explicitly.
