using NpgsqlTypes;

namespace FalareBackend.Models.Enums;

public enum Idioma
{
    [PgName("ingles")] Ingles,
    [PgName("espanhol")] Espanhol
}