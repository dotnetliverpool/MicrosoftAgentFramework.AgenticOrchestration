namespace MicrosoftAgentFramework.Models;

public class TravelIntentResponse
{
    public string? Destination { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public int? Travelers { get; set; }
    public List<string>? Interests { get; set; }
    public string? BudgetLevel { get; set; }
    public List<string>? Constraints { get; set; }
    public List<string>? MissingRequiredDetails { get; set; }
    public List<string>? FollowUpQuestions { get; set; }
    public string? IntentSummary { get; set; }
    public string? ConversationStage { get; set; }
    public bool Success { get; set; }
    public string? UserPromptMessage { get; set; }
}

public class TravelBudgetPlan
{
    public string? Currency { get; set; }
    public string? TotalBudgetEstimate { get; set; }
    public List<string>? AllocationSummary { get; set; }
    public List<string>? CostSavingTips { get; set; }
}

public class TravelDayPlan
{
    public int DayNumber { get; set; }
    public string? Title { get; set; }
    public List<string>? Activities { get; set; }
}

public class TravelItineraryPlan
{
    public string? Summary { get; set; }
    public List<TravelDayPlan>? DailyPlan { get; set; }
    public List<string>? Notes { get; set; }
}

public class TravelLodgingPlan
{
    public string? RecommendedArea { get; set; }
    public string? PropertyType { get; set; }
    public string? NightlyBudgetRange { get; set; }
    public List<string>? Reasons { get; set; }
    public List<string>? BookingTips { get; set; }
}

public class TravelTransportPlan
{
    public List<string>? ArrivalOptions { get; set; }
    public List<string>? LocalTransportOptions { get; set; }
    public List<string>? PassOrCardAdvice { get; set; }
}

public class TravelPlannerResponse
{
    public TravelIntentResponse? Intent { get; set; }
    public TravelBudgetPlan? Budget { get; set; }
    public TravelItineraryPlan? Itinerary { get; set; }
    public TravelLodgingPlan? Lodging { get; set; }
    public TravelTransportPlan? Transport { get; set; }
}

public class TravelPlannerFollowUpResponse
{
    public string Status { get; set; } = "needs_more_info";
    public TravelIntentResponse? Intent { get; set; }
    public string? Guidance { get; set; }
}
