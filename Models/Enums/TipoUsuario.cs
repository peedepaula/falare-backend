using NpgsqlTypes;

namespace FalareBackend.Models.Enums;

public enum TipoUsuario
{
    [PgName("professor")] Professor,
    [PgName("secretaria")] Secretaria
}