using KnowledgeOps.Models.Documents;
using KnowledgeOps.Services;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeOps.Controllers;

public class DocumentController (IDocumentRepository documentRepository) : Controller
{
    // GET
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var documents = await documentRepository.GetAllDocumentsAsync(cancellationToken);
        var model = documents
            .OrderBy(document => document.Title)
            .Select(document => new DocumentListItemViewModel
            {
                Id = document.Id,
                Title = document.Title,
                Category =  document.Category,
                Department =  document.Department,
                Status = document.Status,
                LastReviewedOn =  document.LastReviewedOn,
                Summary =  document.Summary,
            });
        return View(model);
    }

    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var document = await documentRepository.GetDocumentByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        var model = new DocumentDetailsViewModel
        {
            Id = document.Id,
            Title = document.Title,
            Category = document.Category,
            Department = document.Department,
            Status = document.Status,
            LastReviewedOn = document.LastReviewedOn,
            Summary = document.Summary,
            Tags = document.Tags,
            Owner = document.Owner,
        };
        return View(model);
    }
}