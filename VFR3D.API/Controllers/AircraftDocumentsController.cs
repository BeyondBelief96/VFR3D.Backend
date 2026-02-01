using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos.AircraftDocuments;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/aircraft/{userId}/{aircraftId}/documents")]
[ConditionalAuth]
public class AircraftDocumentsController(
    IAircraftDocumentService documentService,
    ILogger<AircraftDocumentsController> logger)
    : ControllerBase
{
    private const int MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AircraftDocumentDto>> UploadDocument(
        string userId,
        string aircraftId,
        IFormFile file,
        [FromForm] string displayName,
        [FromForm] string category,
        [FromForm] string? description = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest($"File size exceeds the maximum allowed size of {MaxFileSizeBytes / 1024 / 1024} MB");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return BadRequest("Display name is required");
        }

        if (!Enum.TryParse<Domain.Enums.DocumentCategory>(category, true, out var documentCategory))
        {
            return BadRequest($"Invalid category. Valid values: {string.Join(", ", Enum.GetNames<Domain.Enums.DocumentCategory>())}");
        }

        try
        {
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading document for aircraft {AircraftId} by user {UserId}", aircraftId, userId);
            return StatusCode(500, "An error occurred while uploading the document");
        }
    }

    /// <summary>
    /// Gets all documents for an aircraft
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <returns>List of documents</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<AircraftDocumentListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AircraftDocumentListDto>>> GetDocuments(
        string userId,
        string aircraftId)
    {
        try
        {
            var documents = await documentService.GetDocumentsForAircraftAsync(userId, aircraftId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting documents for aircraft {AircraftId} by user {UserId}", aircraftId, userId);
            return StatusCode(500, "An error occurred while retrieving documents");
        }
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AircraftDocumentDto>> GetDocument(
        string userId,
        string aircraftId,
        string id)
    {
        try
        {
            var document = await documentService.GetDocumentAsync(userId, id);
            if (document == null)
            {
                return NotFound($"Document not found with ID {id}");
            }

            // Verify document belongs to the specified aircraft
            if (document.AircraftId != aircraftId)
            {
                return NotFound($"Document not found with ID {id}");
            }

            return Ok(document);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting document {DocumentId} for user {UserId}", id, userId);
            return StatusCode(500, "An error occurred while retrieving the document");
        }
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AircraftDocumentUrlDto>> GetDocumentUrl(
        string userId,
        string aircraftId,
        string id)
    {
        try
        {
            // First verify document exists and belongs to the user/aircraft
            var document = await documentService.GetDocumentAsync(userId, id);
            if (document == null || document.AircraftId != aircraftId)
            {
                return NotFound($"Document not found with ID {id}");
            }

            var urlDto = await documentService.GetDocumentUrlAsync(userId, id);
            return Ok(urlDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting URL for document {DocumentId} by user {UserId}", id, userId);
            return StatusCode(500, "An error occurred while generating the document URL");
        }
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AircraftDocumentDto>> UpdateDocumentMetadata(
        string userId,
        string aircraftId,
        string id,
        [FromBody] UpdateAircraftDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest("Display name is required");
        }

        try
        {
            // First verify document exists and belongs to the aircraft
            var existingDocument = await documentService.GetDocumentAsync(userId, id);
            if (existingDocument == null || existingDocument.AircraftId != aircraftId)
            {
                return NotFound($"Document not found with ID {id}");
            }

            var document = await documentService.UpdateDocumentMetadataAsync(userId, id, request);
            return Ok(document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating document {DocumentId} for user {UserId}", id, userId);
            return StatusCode(500, "An error occurred while updating the document");
        }
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AircraftDocumentDto>> ReplaceDocument(
        string userId,
        string aircraftId,
        string id,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest($"File size exceeds the maximum allowed size of {MaxFileSizeBytes / 1024 / 1024} MB");
        }

        try
        {
            // First verify document exists and belongs to the aircraft
            var existingDocument = await documentService.GetDocumentAsync(userId, id);
            if (existingDocument == null || existingDocument.AircraftId != aircraftId)
            {
                return NotFound($"Document not found with ID {id}");
            }

            using var stream = file.OpenReadStream();
            var document = await documentService.ReplaceDocumentAsync(
                userId,
                id,
                stream,
                file.FileName,
                file.ContentType);

            return Ok(document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error replacing document {DocumentId} for user {UserId}", id, userId);
            return StatusCode(500, "An error occurred while replacing the document");
        }
    }

    /// <summary>
    /// Deletes a document
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <param name="id">The ID of the document to delete</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteDocument(
        string userId,
        string aircraftId,
        string id)
    {
        try
        {
            // First verify document exists and belongs to the aircraft
            var existingDocument = await documentService.GetDocumentAsync(userId, id);
            if (existingDocument == null || existingDocument.AircraftId != aircraftId)
            {
                return NotFound($"Document not found with ID {id}");
            }

            await documentService.DeleteDocumentAsync(userId, id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting document {DocumentId} for user {UserId}", id, userId);
            return StatusCode(500, "An error occurred while deleting the document");
        }
    }
}
