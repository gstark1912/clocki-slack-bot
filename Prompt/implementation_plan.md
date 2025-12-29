# Implementation Plan - Clockify Gamification Bot

## Goal Description
Refactor the existing console application into a modular architecture and implement the "Weekly Challenge" gamification logic involving team quotas and a narrative ("Kitten Police").

## User Review Required
> [!IMPORTANT]
> **Persistence**: The bot needs to store the "Team Streak" and current weekly state. Since this runs on GitHub Actions (ephemeral), we need a strategy.
> **Proposal**: I will implement a `GameStateService` that reads/writes to `data/gamestate.json`.
> **Action Required**: The GitHub Action workflow will need to be updated to **commit and push** this file back to the repository after every run to persist changes.

## Proposed Changes

### Database & Persistence
#### [NEW] [GameState.json](file:///c:/Users/Germa/source/repos/clocki-slack-bot/data/gamestate.json)
- **Purpose**: Persist the game state between GitHub Action runs.
- **Structure**:
```json
{
  "teamStreak": 3,
  "lastUpdate": "2025-12-29T10:00:00Z",
  "currentStoryId": 1
}
```

### Configuration & Content
#### [NEW] [stories.json](file:///c:/Users/Germa/source/repos/clocki-slack-bot/data/stories.json)
- **Purpose**: Store the narrative templates.
- **Structure**:
```json
{
  "stories": [
    {
      "id": 1,
      "title": "Salvar al Gatito Ramirez",
      "intro": "Esta semana el Agente Meow nos informa que el Gatito Ramirez está atrapado...",
      "dailyFail": "¡Alerta! @usuario no completó sus horas. Ramirez corre peligro.",
      "riskHigh": "El equipo está al 60% de la meta y es Jueves. ¡Apurate!",
      "win": "¡Misión cumplida! Ramirez ha sido rescatado.",
      "loss": "Lamentablemente, no llegamos a tiempo. Ramirez ha caído."
    }
  ]
}
```

#### [NEW] [vacations.json](file:///c:/Users/Germa/source/repos/clocki-slack-bot/data/vacations.json) (Optional MVP)
- List of dates/emails to exclude from calculations.

### Core Refactor
#### [NEW] [ClockifyService.cs](file:///c:/Users/Germa/source/repos/clocki-slack-bot/ClockifyService.cs)
- Implement `GetTeamSummaryAsync` minimizing API calls (fetch summary group by User for the whole week once).

#### [NEW] [SlackService.cs](file:///c:/Users/Germa/source/repos/clocki-slack-bot/SlackService.cs)
- Support new methods SendMessageAsync, SendDailyMessageAsync, SendWeeklyMessageAsync.
- Support multiple Webhook URLs (defined in Env Vars or Config):
    - `SLACK_GENERAL_WEBHOOK` (Public channel)
    - `SLACK_ADMIN_WEBHOOK` (Admin channel)
    - `SLACK_DM_BOT_TOKEN` (needed for DMs, or use webhook if allowed, but DMs usually require Bot Token).

### Gamification & Logic
#### [NEW] [GameService.cs](file:///c:/Users/Germa/source/repos/clocki-slack-bot/GameService.cs)
- **Day Logic**: The service will read `GameState` and the current date to decide the action:
    - **NotStarted** (first day of the week): `StartNewWeek()` → select story → send intro → set `GameState` to **InProgress**.
    - **InProgress** (daily): `CheckDaily()` → analyze today's hours → send DMs as needed.
    - **Anteultimo** (penultimate day) **and** weekly progress < 60 %: `SendWarning()` → public channel warning.
    - **UltimoDia** (last day of the week): `EndWeek()` → calculate result → send outcome → update streak → reset `GameState` to **NotStarted**.
- **Vacation Logic**: Check `vacations.json` or Clockify "Time Off" project to exclude users from daily targets.

#### [NEW] [Models.cs](file:///c:/Users/Germa/source/repos/clocki-slack-bot/Models.cs)
```csharp
public record GameState(
    int TeamStreak,
    DateTime LastUpdate,
    int CurrentStoryId
);
```

### Infrastructure
#### [MODIFY] [daily_check.yml](file:///c:/Users/Germa/source/repos/clocki-slack-bot/.github/workflows/daily_check.yml)
- Ensure `data/gamestate.json` uses `git commit` to persist.
- Add `SLACK_ADMIN_WEBHOOK` and `SLACK_GENERAL_WEBHOOK` secrets.

## Verification Plan

### Automated Tests
#### [NEW] [ClockifyBot.Tests](file:///c:/Users/Germa/source/repos/clocki-slack-bot/ClockifyBot.Tests/ClockifyBot.Tests.csproj)
- **Unit Tests**:
    - `GameServiceTests`: Test streak logic, win/loss conditions, and date/story progression.
    - `ClockifyServiceTests`: Mock HTTP responses to verify parsing.
- **Run**: `dotnet test` in CI/CD.

### Manual Verification
1. **Mock Data Run**:
   - Create `gamestate.json` with a specific state.
   - Run `dotnet run`.
   - Verify Slack messages and JSON updates.
