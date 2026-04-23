# GitHub OAuth

OpenNet supports sign-in with GitHub using OAuth 2.0.

## 1. Create a GitHub OAuth App

1. Go to **GitHub → Settings → Developer settings → OAuth Apps → New OAuth App**.
2. Fill in the fields:
   - **Application name**: OpenNet (or your preferred name)
   - **Homepage URL**: `https://<your-domain>`
   - **Authorization callback URL**: `https://<your-domain>/signin-github`
3. Click **Register application**.
4. Copy the **Client ID** and generate a **Client Secret**.

## 2. Configure OpenNet

Add the credentials to your configuration (use user secrets or environment variables — never commit secrets to source control):

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

**User Secrets (recommended for development):**

```bash
dotnet user-secrets set "Authentication:GitHub:ClientId" "<your-client-id>"
dotnet user-secrets set "Authentication:GitHub:ClientSecret" "<your-client-secret>"
```

**Environment variables:**

```
Authentication__GitHub__ClientId=<your-client-id>
Authentication__GitHub__ClientSecret=<your-client-secret>
```

## Claims Populated

| Claim | Source |
|-------|--------|
| `NameIdentifier` | GitHub user numeric ID |
| `Name` | GitHub login (username) |
| `Email` | Primary email (if public) |
