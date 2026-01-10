namespace UserManagement.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int MunicipalityId { get; set; }
        // Note: Country and Department are navigational concepts, 
        // but the entity persists the Municipality link.
    }
}
