namespace Solucao.Service.API.Core.Registers;

using Microsoft.Data.SqlClient;

/// <summary>
/// Classe responsável por criar os bancos de dados se não existirem.
/// </summary>
public static class CriadorBancoDados
{
    /// <summary>
    /// Cria os bancos de dados se não existirem.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="nomesBancosDados"></param>
    public static void CriarBancosDadosSeNaoExistirem(string connectionString, List<string> nomesBancosDados)
    {
        using var conexao = new SqlConnection(connectionString);
        conexao.Open();

        foreach (var nomeBancoDados in nomesBancosDados)
        {
            // Verifica se o banco de dados já existe
            var consultaVerificaBancoDados = $"SELECT db_id('{nomeBancoDados}')";
            using (var comando = new SqlCommand(consultaVerificaBancoDados, conexao))
            {
                var resultado = comando.ExecuteScalar();
                if (resultado != null && resultado != DBNull.Value)
                {
                    continue;
                }
            }

            // Cria o banco de dados se não existir
            var consultaCriarBancoDados = $"CREATE DATABASE [{nomeBancoDados}]";
            using (var comando = new SqlCommand(consultaCriarBancoDados, conexao))
            {
                comando.ExecuteNonQuery();
            }

        }
    }
}
