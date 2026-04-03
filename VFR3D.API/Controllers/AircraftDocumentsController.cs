using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos.AircraftDocuments;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/aircraft/{userId}/{aircraftId}/documents")]
[ConditionalAuth]
public class AircraftDocumentsController(IAircraftDocumentService documentService)
    : ControllerBase
{
    private const int MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB

    /// <summary>
    /// Uploads a document for an aircraft
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <param name="file">The document file to upload</param>
    /// <param name="displayName">User-friendly name for the document</param>
    /// <param name="category">Document category</param>
    /// <param name="description">Optional description</param>
    /// <returns>The created document metadata</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AircraftDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AircraftDocumentDto>> UploadDocument(
        string userId,
        string aircraftId,
        IFormFile file,
        [FromForm] string displayName,
        [FromForm] string category,
        [FromForm] string? description = null)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException("File", "No file was uploaded");

        if (file.Length > MaxFileSizeBytes)
            throw new ValidationException("File", $"File size exceeds the maximum allowed size of {MaxFileSizeBytes / 1024 / 1024} MB");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ValidationException("DisplayName", "Display name is required");

        if (!Enum.TryParse<Domain.Enums.DocumentCategory>(category, true, out var documentCategory))
            throw new ValidationException("Category", $"Invalid category. Valid values: {string.Join(", ", Enum.GetNames<Domain.Enums.DocumentCategory>())}");

        using var stream = file.OpenReadStream();
        var request = new CreateAircraftDocumentRequest
        {
            DisplayName = displayName,
            Description = description,
            Category = documentCategory
        };

        var document = await documentService.UploadDocumentAsync(
            userId,
            aircraftId,
            stream,
            file.FileName,
            file.ContentType,
            request);

        return Ok(document);
    }

    /// <summary>
    /// Gets all documents for an aircraft
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <returns>List of documents</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<AircraftDocumentListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AircraftDocumentListDto>>> GetDocuments(
        string userId,
        string aircraftId)
    {
        var documents = await documentService.GetDocumentsForAircraftAsync(userId, aircraftId);
        return Ok(documents);
    }

    /// <summary>
    /// Gets a single document's metadata
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <param name="id">The ID of the document</param>
    /// <returns>The document metadata</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AircraftDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AircraftDocumentDto>> GetDocument(
        string userId,
        string aircraftId,
        string id)
    {
        var document = await documentService.GetDocumentAsync(userId, id);
        if (document == null)
            throw new DocumentNotFoundException(id);

        // Verify document belongs to the specified aircraft
        if (document.AircraftId != aircraftId)
            throw new DocumentNotFoundException(aircraftId, id);

        return Ok(document);
    }

    /// <summary>
    /// Gets a presigned URL for accessing a document
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <param name="id">The ID of the document</param>
    /// <returns>A presigned URL for the document</returns>
    [HttpGet("{id}/url")]
    [ProducesResponseType(typeof(AircraftDocumentUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AircraftDocumentUrlDto>> GetDocumentUrl(
        string userId,
        string aircraftId,
        string id)
    {
        // First verify document exists and belongs to the user/aircraft
        var document = await documentService.GetDocumentAsync(userId, id);
        if (document == null || document.AircraftId != aircraftId)
            throw new DocumentNotFoundException(aircraftId, id);

        var urlDto = await documentService.GetDocumentUrlAsync(userId, id);
        return Ok(urlDto);
    }

    /// <summary>
    /// Updates a document's metadata
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <param name="id">The ID of the document</param>
    /// <param name="request">The updated metadata</param>
    /// <returns>The updated document metadata</returns>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(AircraftDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AircraftDocumentDto>> UpdateDocumentMetadata(
        string userId,
        string aircraftId,
        string id,
        [FromBody] UpdateAircraftDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new ValidationException("DisplayName", "Display name is required");

        // First verify document exists and belongs to the aircraft
        var existingDocument = await documentService.GetDocumentAsync(userId, id);
        if (existingDocument == null || existingDocument.AircraftId != aircraftId)
            throw new DocumentNotFoundException(aircraftId, id);

        var document = await documentService.UpdateDocumentMetadataAsync(userId, id, request);
        return Ok(document);
    }

    /// <summary>
    /// Replaces a document's file content
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <param name="id">The ID of the document</param>
    /// <param name="file">The new document file</param>
    /// <returns>The updated document metadata</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AircraftDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AircraftDocumentDto>> ReplaceDocument(
        string userId,
        string aircraftId,
        string id,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException("File", "No file was uploaded");

        if (file.Length > MaxFileSizeBytes)
            throw new ValidationException("File", $"File size exceeds the maximum allowed size of {MaxFileSizeBytes / 1024 / 1024} MB");

        // First verify document exists and belongs to the aircraft
        var existingDocument = await documentService.GetDocumentAsync(userId, id);
        if (existingDocument == null || existingDocument.AircraftId != aircraftId)
            throw new DocumentNotFoundException(aircraftId, id);

        using var stream = file.OpenReadStream();
        var document = await documentService.ReplaceDocumentAsync(
            userId,
            id,
            stream,
            file.FileName,
            file.ContentType);

        return Ok(document);
    }

    /// <summary>
    /// Deletes a document
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <param name="id">The ID of the document to delete</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(
        string userId,
        string aircraftId,
        string id)
    {
        // First verify document exists and belongs to the aircraft
        var existingDocument = await documentService.GetDocumentAsync(userId, id);
        if (existingDocument == null || existingDocument.AircraftId != aircraftId)
            throw new DocumentNotFoundException(aircraftId, id);

        await documentService.DeleteDocumentAsync(userId, id);
        return Ok();
    }
}
