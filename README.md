# Clockify Slack Bot

A C# console application that checks Clockify hours for specific users and notifies them via Slack if they haven't reached their daily target.

## Features

- **Clockify Integration**: Fetches daily summary reports and user details.
- **Slack Integration**: Sends notifications via Webhook Trigger URL.
- **Targeted Checking**: Checks specific emails defined in configuration.
- **GitHub Actions**: Automated daily checks (Mon-Fri).

## Configuration

The application requires the following environment variables:

| Variable | Description |
|----------|-------------|
| `CLOCKIFY_API_KEY` | Your Clockify API Key. |
| `CLOCKIFY_WORKSPACE_ID` | The ID of the Clockify Workspace. |
| `SLACK_BOT_TRIGGER_URL` | The Slack Webhook Trigger URL for sending messages. |
| `TARGET_EMAILS` | Semicolon-separated list of emails to check (e.g., `user1@example.com;user2@example.com`). |
| `TARGET_HOURS` | (Optional) Daily target hours (default: 8.0). |

## Project Structure

- **[Program.cs](file:///c:/Users/Germa/source/repos/clocki-slack-bot/Program.cs)**: Main entry point and orchestration logic.
- **[ClockifyService.cs](file:///c:/Users/Germa/source/repos/clocki-slack-bot/ClockifyService.cs)**: Handles Clockify API interactions.
- **[SlackService.cs](file:///c:/Users/Germa/source/repos/clocki-slack-bot/SlackService.cs)**: Handles Slack API interactions via Trigger URL.
- **[Models.cs](file:///c:/Users/Germa/source/repos/clocki-slack-bot/Models.cs)**: Data Transfer Objects.

## GitHub Actions

The workflow is defined in **[.github/workflows/daily_check.yml](file:///c:/Users/Germa/source/repos/clocki-slack-bot/.github/workflows/daily_check.yml)**.

### Setup Secrets
To run this in GitHub Actions, configure the following Repository Secrets:
- `CLOCKIFY_API_KEY`
- `CLOCKIFY_WORKSPACE_ID`
- `SLACK_BOT_TRIGGER_URL`
- `TARGET_EMAILS`

## Local Development

Update `Properties/launchSettings.json` with your keys to run locally.

```bash
dotnet run
```
