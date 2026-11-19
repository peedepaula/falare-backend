using System.Security.Claims;

namespace FalareBackend.Extensions
{
    public static class ClaimsExtensions
    {
        public static Guid UsuarioId(this ClaimsPrincipal user) =>
            Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public static bool EhProfessor(this ClaimsPrincipal user) =>
            user.IsInRole("Professor");
    }
}