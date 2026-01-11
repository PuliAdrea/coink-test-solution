namespace UserManagement.Application.DTOs
{
    public record UserResponseDto(
     int Id,
     string Name,
     string Phone,
     string Address,
     string MunicipalityName,
     string DepartmentName,
     string CountryName
 );
}
