using NpgsqlTypes;

namespace FalareBackend.Models.Enums;

public enum StatusMatricula
{
    [PgName("ativa")] Ativa,
    [PgName("concluida")] Concluida,
    [PgName("certificada")] Certificada,
    [PgName("reprovada_falta")] ReprovadaFalta,
    [PgName("reprovada_nota")] ReprovadaNota,
    [PgName("trancada")] Trancada
}