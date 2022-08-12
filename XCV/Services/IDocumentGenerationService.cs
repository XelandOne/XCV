using System.Threading.Tasks;
using XCV.Entities;

namespace XCV.Services
{
    /// <summary>
    /// Handles a document generation request.
    /// </summary>
    public interface IDocumentGenerationService
    {
        /// <summary>
        /// Generates and downloads a document with the document configurations details.
        /// </summary>
        /// <param name="documentConfiguration">The document configuration</param>
        /// <returns>Success of the document download</returns>
        Task<bool> GenerateDocument(DocumentConfiguration documentConfiguration);

    }
}