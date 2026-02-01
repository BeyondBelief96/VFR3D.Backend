using VFR3D.Infrastructure.Dtos.AircraftDocuments;

namespace VFR3D.Infrastructure.Interfaces;

public interface IAircraftDocumentService
{
    Task<AircraftDocumentDto> UploadDocumentAsync(
        string userId,
        string aircraftId,
        Stream fileStream,
        string fileName,
        string contentType,
        CreateAircraftDocumentRequest request);

    Task<List<AircraftDocumentListDto>> GetDocumentsForAircraftAsync(string userId, string aircraftId);

    Task<AircraftDocumentDto?> GetDocumentAsync(string userId, string documentId);

    Task<AircraftDocumentUrlDto> GetDocumentUrlAsync(string userId, string documentId);

    Task<AircraftDocumentDto> UpdateDocumentMetadataAsync(
        string userId,
        string documentId,
        UpdateAircraftDocumentRequest request);

    Task<AircraftDocumentDto> ReplaceDocumentAsync(
        string userId,
        string documentId,
        Stream fileStream,
        string fileName,
        string contentType);

    Task DeleteDocumentAsync(string userId, string documentId);
}
