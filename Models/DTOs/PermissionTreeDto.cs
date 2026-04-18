namespace sms.api.Models.DTOs
{
    /// <summary>Permission node for tree API (parent with optional children).</summary>
    public class PermissionTreeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid? ParentPermissionId { get; set; }
        public List<PermissionTreeDto> Children { get; set; } = new();
    }
}
