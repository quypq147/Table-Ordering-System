// AdminWeb user DTOs kept local to AdminWeb
using System.ComponentModel.DataAnnotations;

namespace AdminWeb.Dtos;

public record UserVm(Guid Id, string UserName, string Email, string FullName, bool IsActive, string[] Roles);
public record UserDetailVm(Guid Id, string UserName, string Email, string FullName, bool IsActive, List<string> Roles);
public record CreateUserVm(
 [Required, StringLength(64, MinimumLength =3)] string UserName,
 [Required, EmailAddress] string Email,
 [Required, StringLength(128, MinimumLength =1)] string FullName,
 [Required, StringLength(64, MinimumLength =6)] string Password
);
public record UpdateUserVm(
 [Required, StringLength(64, MinimumLength =3)] string UserName,
 [Required, EmailAddress] string Email,
 [Required, StringLength(128, MinimumLength =1)] string FullName,
 bool IsActive
);
