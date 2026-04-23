# Google Sign-In (OpenID Connect)

OpenNet supports sign-in with Google via OpenID Connect.

## 1. Create a Google OAuth Client

1. Open the [Google Cloud Console](https://console.cloud.google.com/).
2. Select or create a project.
3. Navigate to **APIs & Services → Credentials → Create Credentials → OAuth client ID**.
4. Choose **Web application**.
5. Add an **Authorized redirect URI**: `https://<your-domain>/signin-oidc` (or your configured callback path).
6. Copy the **Client ID** and **Client Secret**.

## 2. Configure OpenNet

Add the provider under `Authentication:OpenIdConnect`. The key (`Google` below) becomes the ASP.NET Core scheme name — you may choose any name.

```json
{
  "Authentication": {
    "OpenIdConnect": {
      "Google": {
        "ClientId": "<your-client-id>",
        "ClientSecret": "<your-client-secret>",
        "Authority": "https://accounts.google.com"
      }
    }
  }
}
```

**User Secrets (recommended for development):**

```bash
dotnet user-secrets set "Authentication:OpenIdConnect:Google:ClientId" "<your-client-id>"
dotnet user-secrets set "Authentication:OpenIdConnect:Google:ClientSecret" "<your-client-secret>"
dotnet user-secrets set "Authentication:OpenIdConnect:Google:Authority" "https://accounts.google.com"
```

**Environment variables:**

```
Authentication__OpenIdConnect__Google__ClientId=<your-client-id>
Authentication__OpenIdConnect__Google__ClientSecret=<your-client-secret>
Authentication__OpenIdConnect__Google__Authority=https://accounts.google.com
```

## Notes

- The `Authority` for Google is always `https://accounts.google.com`.
- ASP.NET Core automatically discovers the OIDC metadata from `{Authority}/.well-known/openid-configuration`.
