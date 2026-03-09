namespace Application.Features.Common.PageContents.DTOs
{
    public class PageContentDto
    {
        public Guid Id { get; set; }
        public string PageSlug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
        public string? LastModifiedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
