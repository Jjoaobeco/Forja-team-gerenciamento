using ForjaTeamApp.Data;
using ForjaTeamApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Pages;

[Authorize(Roles = "Admin")]
public class DebitosModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public DebitosModel(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [BindProperty(SupportsGet = true)]
    public string Filtro { get; set; } = "todos";

    [BindProperty(SupportsGet = true)]
    public string? Busca { get; set; }

    public List<Aluno> Alunos { get; set; } = new();
    public Dictionary<int, int> CheckinsMesPorAluno { get; set; } = new();

    public decimal ReceitaMensalPrevista { get; set; }
    public decimal ReceitaConveniosMes { get; set; }
    public decimal ValorCheckinWellhub { get; set; }
    public decimal ValorCheckinTotalPass { get; set; }

    public int TotalCount { get; set; }
    public int PagosCount { get; set; }
    public int PendentesCount { get; set; }
    public int ConveniosCount { get; set; }

    [TempData]
    public string? Mensagem { get; set; }

    public async Task OnGetAsync()
    {
        ValorCheckinWellhub = _config.GetValue<decimal?>("RepasseConvenios:WellhubSilver") ?? 20.00m;
        ValorCheckinTotalPass = _config.GetValue<decimal?>("RepasseConvenios:TotalPass") ?? 18.00m;

        var todosAlunos = await _db.Alunos
            .OrderBy(a => a.Nome)
            .ToListAsync();

        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var proximoMes = inicioMes.AddMonths(1);

        CheckinsMesPorAluno = await _db.Presencas
            .Where(p => p.Validada && p.DataHora >= inicioMes && p.DataHora < proximoMes)
            .GroupBy(p => p.AlunoId)
            .Select(g => new { AlunoId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.AlunoId, x => x.Total);

        var diretos = todosAlunos
            .Where(a => a.TipoPlano != "Wellhub" && a.TipoPlano != "TotalPass")
            .ToList();

        var convenios = todosAlunos
            .Where(a => a.TipoPlano == "Wellhub" || a.TipoPlano == "TotalPass")
            .ToList();

        TotalCount = todosAlunos.Count;
        PagosCount = diretos.Count(a => a.SituacaoPagamento == "Em Dia");
        PendentesCount = diretos.Count(a => a.SituacaoPagamento != "Em Dia");
        ConveniosCount = convenios.Count;

        ReceitaMensalPrevista = diretos
            .Where(a => a.Status == "Ativo")
            .Sum(a => a.ValorMensalidade);

        ReceitaConveniosMes = convenios.Sum(a =>
        {
            var qtd = CheckinsMesPorAluno.GetValueOrDefault(a.Id, 0);
            var tarifa = a.TipoPlano == "Wellhub" ? ValorCheckinWellhub : ValorCheckinTotalPass;
            return qtd * tarifa;
        });

        IEnumerable<Aluno> filtrados = todosAlunos;

        filtrados = (Filtro ?? "todos").ToLowerInvariant() switch
        {
            "pagos" => diretos.Where(a => a.SituacaoPagamento == "Em Dia"),
            "pendentes" => diretos.Where(a => a.SituacaoPagamento != "Em Dia"),
            "convenios" => convenios,
            _ => todosAlunos
        };

        if (!string.IsNullOrWhiteSpace(Busca))
        {
            var termo = Busca.Trim();
            filtrados = filtrados.Where(a =>
                a.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (a.Cpf != null && a.Cpf.Contains(termo, StringComparison.OrdinalIgnoreCase)));
        }

        Alunos = filtrados.ToList();
    }

    public async Task<IActionResult> OnPostMarcarPagoAsync(int id, string? filtro, string? busca)
    {
        var aluno = await _db.Alunos.FindAsync(id);
        if (aluno != null)
        {
            aluno.SituacaoPagamento = "Em Dia";
            aluno.ProximoVencimento = DateTime.Today.AddDays(30);
            await _db.SaveChangesAsync();
            Mensagem = $"Mensalidade de {aluno.Nome} confirmada! Novo vencimento: {aluno.ProximoVencimento:dd/MM/yyyy}.";
        }
        return RedirectToPage(new { Filtro = filtro ?? "todos", Busca = busca });
    }

    public async Task<IActionResult> OnPostMarcarPendenteAsync(int id, string? filtro, string? busca)
    {
        var aluno = await _db.Alunos.FindAsync(id);
        if (aluno != null)
        {
            aluno.SituacaoPagamento = "Pendente";
            await _db.SaveChangesAsync();
            Mensagem = $"Situação de {aluno.Nome} marcada como Pendente.";
        }
        return RedirectToPage(new { Filtro = filtro ?? "todos", Busca = busca });
    }
}