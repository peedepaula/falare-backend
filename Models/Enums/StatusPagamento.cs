using NpgsqlTypes;

namespace FalareBackend.Models.Enums;

public enum StatusPagamento
{
    [PgName("pendente")] Pendente,
    [PgName("paga")] Paga
}