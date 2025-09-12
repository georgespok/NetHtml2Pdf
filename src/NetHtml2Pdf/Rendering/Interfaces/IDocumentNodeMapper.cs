using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;

namespace NetHtml2Pdf.Rendering.Interfaces
{
    /// <summary>
    /// Interface for mapping document nodes to QuestPDF elements
    /// </summary>
    /// <typeparam name="T">The type of document node this mapper handles</typeparam>
    public interface IDocumentNodeMapper<in T> where T : DocumentNode
    {
        /// <summary>
        /// Maps a document node to QuestPDF elements
        /// </summary>
        /// <param name="node">The document node to map</param>
        /// <param name="container">The QuestPDF container to add elements to</param>
        void Map(T node, IContainer container);
    }

    /// <summary>
    /// Non-generic interface for document node mappers
    /// </summary>
    public interface IDocumentNodeMapper
    {
        /// <summary>
        /// Maps a document node to QuestPDF elements
        /// </summary>
        /// <param name="node">The document node to map</param>
        /// <param name="container">The QuestPDF container to add elements to</param>
        void Map(DocumentNode node, IContainer container);

        /// <summary>
        /// Determines if this mapper can handle the specified node type
        /// </summary>
        /// <param name="node">The document node to check</param>
        /// <returns>True if this mapper can handle the node</returns>
        bool CanMap(DocumentNode node);
    }
}
