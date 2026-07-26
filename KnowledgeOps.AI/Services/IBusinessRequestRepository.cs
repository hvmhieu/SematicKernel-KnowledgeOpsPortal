using KnowledgeOps.AI.Models;

namespace KnowledgeOps.AI;

public interface IBusinessRequestRepository
{
    Task<BusinessRequest?> GetByIdAsync(string requestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusinessRequest>> GetOpenRequestsAsync(CancellationToken cancellationToken = default);
}

public sealed class InMemoryBusinessRequestRepository : IBusinessRequestRepository
{
    private static readonly IReadOnlyList<BusinessRequest> Requests = [
        new BusinessRequest
        {
            Id = "REQ-1001",
            Title = "Approve Azure OpenAI access for support knowledge assistant",
            Department = "Customer Support",
            RequestedBy = "Alicia Morgan",
            Description = "The support team wants access to Azure OpenAI so they can build an internal assistant that helps agents answer product support questions faster.",
            BusinessJustification = "Support response times have increased because agents must search multiple systems manually. The assistant should reduce lookup time and improve answer consistency.",
            Status = BusinessRequestStatus.InReview,
            Impact = "High",
            Urgency = "Medium",
            SubmittedOnUtc = DateTime.UtcNow.AddDays(-5),
            RequiredByUtc = DateTime.UtcNow.AddDays(10),
            AssignedTo = "Operations Review Team"
        },
        new BusinessRequest
        {
            Id = "REQ-1002",
            Title = "Enable finance invoice categorization copilot",
            Department = "Finance",
            RequestedBy = "Kevin Tran",
            Description = "Finance requests an internal assistant to classify vendor invoices by cost center and suggest anomaly flags before monthly close.",
            BusinessJustification = "Manual invoice triage is slow and error-prone during close week. Automation can reduce rework and speed reconciliation.",
            Status = BusinessRequestStatus.New,
            Impact = "High",
            Urgency = "High",
            SubmittedOnUtc = DateTime.UtcNow.AddDays(-2),
            RequiredByUtc = DateTime.UtcNow.AddDays(7),
            AssignedTo = "Finance Operations"
        },
        new BusinessRequest
        {
            Id = "REQ-1003",
            Title = "Pilot legal contract clause summarization workflow",
            Department = "Legal",
            RequestedBy = "Mina Patel",
            Description = "Legal asks for a pilot that summarizes contract clauses and highlights non-standard terms for reviewer attention.",
            BusinessJustification = "Review queues are growing and first-pass reviews consume significant lawyer time. A summary assistant can improve throughput.",
            Status = BusinessRequestStatus.WaitingForInformation,
            Impact = "Medium",
            Urgency = "Medium",
            SubmittedOnUtc = DateTime.UtcNow.AddDays(-8),
            RequiredByUtc = DateTime.UtcNow.AddDays(14),
            AssignedTo = "Legal Ops AI Review"
        },
        new BusinessRequest
        {
            Id = "REQ-1004",
            Title = "Create HR onboarding Q&A assistant for policy lookup",
            Department = "Human Resources",
            RequestedBy = "Daniela Ruiz",
            Description = "HR proposes an assistant that answers onboarding policy questions and links to official internal policy pages.",
            BusinessJustification = "New hires submit repeated policy questions to HR coordinators. A guided assistant can reduce repetitive tickets and improve onboarding experience.",
            Status = BusinessRequestStatus.InReview,
            Impact = "Medium",
            Urgency = "Low",
            SubmittedOnUtc = DateTime.UtcNow.AddDays(-3),
            RequiredByUtc = DateTime.UtcNow.AddDays(20),
            AssignedTo = "HR Systems Team"
        }
    ];
    public Task<BusinessRequest?> GetByIdAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        
        BusinessRequest? request = Requests.FirstOrDefault(request => string.Equals(request.Id, requestId.Trim(), 
                                                            StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(request);
    }

    public Task<IReadOnlyList<BusinessRequest>> GetOpenRequestsAsync(CancellationToken cancellationToken = default)
    {
        BusinessRequestStatus[] closedStatus =
        [
            BusinessRequestStatus.Approved,
            BusinessRequestStatus.Rejected,
            BusinessRequestStatus.Completed
        ];
        
        IReadOnlyList<BusinessRequest> openRequests = Requests
            .Where(request => !closedStatus.Contains(request.Status))
            .OrderBy(request => request.RequiredByUtc)
            .ToList();
        
        return Task.FromResult(openRequests);
    }
}