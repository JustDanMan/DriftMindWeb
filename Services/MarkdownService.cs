using Markdig;

namespace DriftMindWeb.Services
{
    public interface IMarkdownService
    {
        string ToHtml(string markdown);
    }

    public class MarkdownService : IMarkdownService
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownService()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseSoftlineBreakAsHardlineBreak()
                .UseEmojiAndSmiley()
                .UsePipeTables()
                .UseGridTables()
                .UseListExtras()
                .UseTaskLists()
                .UseAutoLinks()
                .UseGenericAttributes()
                .UseCitations()
                .UseCustomContainers()
                .UseMathematics()
                .UseMediaLinks()
                .UseFigures()
                .UseFooters()
                .UseFootnotes()
                .UseAbbreviations()
                .UseDefinitionLists()
                .Build();
        }

        public string ToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            return Markdown.ToHtml(markdown, _pipeline);
        }
    }
}
