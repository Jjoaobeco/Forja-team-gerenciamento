using ForjaTeamApp.Data;
using ForjaTeamApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Pages;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public int TotalAlunosAtivos { get; set; }
    public int CheckinsHoje { get; set; }
    public int AlunosConvenio { get; set; }
    public int TotalTurmas { get; set; }
    public int CheckinsPendentes { get; set; }
    public int MensalidadesPendentes { get; set; }
    public List<Presenca> UltimasPresencas { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalAlunosAtivos = await _db.Alunos.CountAsync(a => a.Status == "Ativo");
        CheckinsHoje = await _db.Presencas.CountAsync(p => p.Validada && p.DataHora.Date == DateTime.Today);
        AlunosConvenio = await _db.Alunos.CountAsync(a => a.TipoPlano == "Wellhub" || a.TipoPlano == "TotalPass");
        TotalTurmas = await _db.Turmas.CountAsync();

        CheckinsPendentes = await _db.Presencas.CountAsync(p => !p.Validada);
        MensalidadesPendentes = await _db.Alunos.CountAsync(a =>
            a.TipoPlano != "Wellhub" &&
            a.TipoPlano != "TotalPass" &&
            a.SituacaoPagamento != "Em Dia");

        UltimasPresencas = await _db.Presencas
            .Include(p => p.Aluno)
            .Include(p => p.Turma)
            .Where(p => p.Validada)
            .OrderByDescending(p => p.DataHora)
            .Take(10)
            .ToListAsync();
    }
}