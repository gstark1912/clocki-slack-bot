# Clockify Slack Bot Walkthrough

I have successfully implemented the Clockify Slack Bot console application and the GitHub Actions workflow.

## Changes Implemented

### 1. Project Files
- **[TimeChecker.csproj](file:///c:/Users/Germa/source/repos/clocki-slack-bot/TimeChecker.csproj)**: .NET 8 Console App project file.
- **[Program.cs](file:///c:/Users/Germa/source/repos/clocki-slack-bot/Program.cs)**: Main logic.
    - Reads config from Environment Variables.
    - Fetches daily summary from Clockify.
    - Calculates hours per user.
    - Looks up Slack ID by email.
    - Sends notification if under target.

### 2. GitHub Actions
- **[.github/workflows/daily_check.yml](file:///c:/Users/Germa/source/repos/clocki-slack-bot/.github/workflows/daily_check.yml)**:
    - Runs Mon-Fri at 18:00 UTC.
    - Injects secrets: `CLOCKIFY_API_KEY`, `CLOCKIFY_WORKSPACE_ID`, `SLACK_BOT_TOKEN`.

## Verification Results

### Build Verification
I ran `dotnet build` and it succeeded.

```
Build succeeded in 2.8s
```

### Next Steps for User
1.  **Commit and Push** the code to GitHub.
2.  **Configure Secrets** in your GitHub Repository settings:
    - `CLOCKIFY_API_KEY`
    - `CLOCKIFY_WORKSPACE_ID`
    - `SLACK_BOT_TOKEN`
3.  **Test Manually**: Go to the "Actions" tab in GitHub and run the "Daily Time Check" workflow manually to verify it works with real keys.
