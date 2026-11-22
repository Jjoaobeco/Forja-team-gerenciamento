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
    public int TotalTurmas { get; set; }
    public int AlunosConvenio { get; set; }
    public List<Presenca> UltimasPresencas { get; set; } = new();

    public async Task OnGetAsync()
    {
        var hoje = DateTime.Today;

        TotalAlunosAtivos = await _db.Alunos.CountAsync(a => a.Status == "Ativo");
        CheckinsHoje = await _db.Presencas.CountAsync(p => p.DataHora.Date == hoje);
        TotalTurmas = await _db.Turmas.CountAsync();
        AlunosConvenio = await _db.Alunos.CountAsync(a => a.TipoPlano == "Wellhub" || a.TipoPlano == "TotalPass");

        UltimasPresencas = await _db.Presencas
            .Include(p => p.Aluno)
            .Include(p => p.Turma)
            .OrderByDescending(p => p.DataHora)
            .Take(10)
            .ToListAsync();
    }
}