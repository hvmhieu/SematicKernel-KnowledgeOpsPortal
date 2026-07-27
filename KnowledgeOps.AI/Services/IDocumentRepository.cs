using KnowledgeOps.Models.Documents;

namespace KnowledgeOps.Services;

public interface IDocumentRepository
{
    Task<IReadOnlyList<KnowledgeDocument>> GetAllDocumentsAsync(CancellationToken cancellationToken);
    Task<KnowledgeDocument?> GetDocumentByIdAsync(string id, CancellationToken cancellationToken);
}

public class InMemoryDocumentRepository : IDocumentRepository
{
    private static readonly IReadOnlyList<KnowledgeDocument> Documents =
    [
        new KnowledgeDocument
        {
            Id = "DOC-1001",
            Title = "IT Security Policy & Guidelines",
            Category = "Information Security",
            Department = "IT Operations",
            Owner = "Security Team",
            Status = "Approved",
            LastReviewedOn = new DateOnly(2026, 3, 15),
            Summary =
                "Comprehensive security guidelines covering password rules, multi-factor authentication, remote work safety, and incident reporting procedures.",
            Tags = ["security", "it-policy", "compliance"]
        },
        new KnowledgeDocument
        {
            Id = "DOC-1002",
            Title = "Quarterly Expense Reimbursement Flow",
            Category = "Finance",
            Department = "Finance & Accounting",
            Owner = "Accounts Payable",
            Status = "Under Review",
            LastReviewedOn = new DateOnly(2026, 5, 10),
            Summary =
                "Step-by-step instructions on submitting expense receipts, approval thresholds for managers, and expected payout timelines.",
            Tags = ["finance", "reimbursement", "expenses"]
        },
        new KnowledgeDocument
        {
            Id = "DOC-1003",
            Title = "API Architecture & Design Standards",
            Category = "Engineering",
            Department = "Software Development",
            Owner = "Core Tech Leads",
            Status = "Draft",
            LastReviewedOn = new DateOnly(2026, 6, 1),
            Summary =
                "Best practices and mandatory patterns for RESTful API design, versioning, error handling, and Semantic Kernel integration.",
            Tags = ["engineering", "architecture", "api-design"]
        }
    ];
        
    public Task<IReadOnlyList<KnowledgeDocument>> GetAllDocumentsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Documents);
    }

    public Task<KnowledgeDocument?> GetDocumentByIdAsync(string id, CancellationToken cancellationToken)
    {
        var document = Documents.FirstOrDefault(document => string.Equals(document.Id, id, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(document);
    }
}