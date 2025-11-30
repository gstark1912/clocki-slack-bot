using ClockiSlackBot;

// Configuration
string? clockifyApiKey = Environment.GetEnvironmentVariable("CLOCKIFY_API_KEY");
string? clockifyWorkspaceId = Environment.GetEnvironmentVariable("CLOCKIFY_WORKSPACE_ID");
string? slackBotTriggerUrl = Environment.GetEnvironmentVariable("SLACK_BOT_TRIGGER_URL");
string? targetEmailsEnv = Environment.GetEnvironmentVariable("TARGET_EMAILS");
double targetHours = double.TryParse(Environment.GetEnvironmentVariable("TARGET_HOURS"), out double t) ? t : 8.0;

if (string.IsNullOrEmpty(clockifyApiKey) || string.IsNullOrEmpty(clockifyWorkspaceId) || string.IsNullOrEmpty(slackBotTriggerUrl) || string.IsNullOrEmpty(targetEmailsEnv))
{
    Console.WriteLine("Error: Missing required environment variables (CLOCKIFY_API_KEY, CLOCKIFY_WORKSPACE_ID, SLACK_BOT_TRIGGER_URL, TARGET_EMAILS).");
    Environment.Exit(1);
}

var targetEmails = targetEmailsEnv.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

using HttpClient httpClient = new HttpClient();
var clockifyService = new ClockifyService(httpClient, clockifyApiKey);
var slackService = new SlackService(httpClient, slackBotTriggerUrl);

Console.WriteLine("Fetching Clockify users and report...");
var today = DateTime.UtcNow;

var usersTask = clockifyService.GetUsersAsync(clockifyWorkspaceId);
var reportTask = clockifyService.GetDailySummaryAsync(clockifyWorkspaceId, today);

await Task.WhenAll(usersTask, reportTask);

var allUsers = usersTask.Result;
var clockifyData = reportTask.Result;

if (clockifyData?.GroupOne == null)
{
    Console.WriteLine("No report data found for today. Checking if users have 0 hours...");
    // If no report data, it means NO ONE worked, or at least no one in the filter.
    // We should treat everyone as having 0 hours.
}

foreach (var email in targetEmails)
{
    var user = allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    if (user == null)
    {
        Console.WriteLine($"Warning: User with email {email} not found in Clockify workspace.");
        continue;
    }

    // Find user's entry in the report
    // The report groups by USER, and the 'Name' field is the user's name.
    // We match by Name because that's what the summary report gives us.
    // Ideally we would match by ID if available in the group, but let's try Name first.
    
    var userEntry = clockifyData?.GroupOne?.FirstOrDefault(g => g.Name.Equals(user.Name, StringComparison.OrdinalIgnoreCase));
    
    // If we added ID to the model and it's populated, we could use that:
    // var userEntry = clockifyData?.GroupOne?.FirstOrDefault(g => g.Id == user.Id); 

    double totalHours = 0;
    if (userEntry != null)
    {
        totalHours = userEntry.Duration / 3600.0;
    }

    if (totalHours < targetHours)
    {
        Console.WriteLine($"User {user.Name} ({email}) has {totalHours:F2} hours. Target is {targetHours}. Notifying...");
        await slackService.SendMessageAsync(email, $"Hola, hoy tienes cargadas {totalHours:F2} horas. Por favor completa tus horas del día.");
    }
    else
    {
        Console.WriteLine($"User {user.Name} ({email}) has reached the target ({totalHours:F2} >= {targetHours}).");
    }
}

Console.WriteLine("Done.");
