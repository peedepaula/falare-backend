using NpgsqlTypes;

namespace FalareBackend.Models.Enums;

public enum Nivel
{
    [PgName("ingles_1")] Ingles1,
    [PgName("ingles_2")] Ingles2,
    [PgName("ingles_3")] Ingles3,
    [PgName("espanhol_1")] Espanhol1,
    [PgName("espanhol_2")] Espanhol2,
    [PgName("espanhol_3")] Espanhol3
}