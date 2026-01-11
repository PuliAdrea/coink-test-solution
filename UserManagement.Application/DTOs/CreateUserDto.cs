namespace UserManagement.Application.DTOs
{
    public class CreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public int CountryId { get; set; }
        public int DepartmentId { get; set; }
        public int MunicipalityId { get; set; }
    }
}
