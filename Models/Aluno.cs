using System.ComponentModel.DataAnnotations;

namespace ForjaTeamApp.Models;

public class Aluno
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório")]
    public string Cpf { get; set; } = string.Empty;

    public string SenhaHash { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string ModalidadePrincipal { get; set; } = "Muay Thai";

    public string TipoPlano { get; set; } = "Direto";

    public string Status { get; set; } = "Ativo";

    public string GraduacaoAtual { get; set; } = "Faixa Branca / Iniciante";

    public int AulasNaGraduacaoAtual { get; set; } = 0;

    public int MetaAulasGraduacao { get; set; } = 40;

    public decimal ValorMensalidade { get; set; } = 120.00m;

    public string SituacaoPagamento { get; set; } = "Em Dia";

    public DateTime? ProximoVencimento { get; set; } = DateTime.Today.AddDays(30);

    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public List<Presenca> Presencas { get; set; } = new();

    public int AulasFaltantes => Math.Max(0, MetaAulasGraduacao - AulasNaGraduacaoAtual);

    public int PercentualEvolucao => MetaAulasGraduacao > 0
        ? Math.Min(100, (int)Math.Round((double)AulasNaGraduacaoAtual / MetaAulasGraduacao * 100))
        : 0;
}