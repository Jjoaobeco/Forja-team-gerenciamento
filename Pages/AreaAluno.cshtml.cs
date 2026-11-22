using ForjaTeamApp.Data;
using ForjaTeamApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Pages;

public class AreaAlunoModel : PageModel
{
    private readonly AppDbContext _db;

    public AreaAlunoModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int? AlunoId { get; set; }

    public bool PodeTrocarAluno { get; set; }
    public List<Aluno> TodosAlunos { get; set; } = new();
    public Aluno? AlunoSelecionado { get; set; }
    public List<Presenca> HistoricoTreinos { get; set; } = new();
    public List<Aluno> Ranking { get; set; } = new();
    public int PosicaoRanking { get; set; }

    public async Task OnGetAsync()
    {
        PodeTrocarAluno = User.IsInRole("Admin");

        TodosAlunos = await _db.Alunos
            .Include(a => a.Presencas)
            .OrderBy(a => a.Nome)
            .ToListAsync();

        if (!TodosAlunos.Any()) return;

        int? idDaSessao = null;
        var claimAlunoId = User.FindFirst("AlunoId")?.Value;
        if (!PodeTrocarAluno && int.TryParse(claimAlunoId, out var parsedId))
        {
            idDaSessao = parsedId;
        }

        var idAlvo = idDaSessao ?? AlunoId;

        AlunoSelecionado = idAlvo.HasValue
            ? TodosAlunos.FirstOrDefault(a => a.Id == idAlvo.Value) ?? TodosAlunos.First()
            : TodosAlunos.First();

        HistoricoTreinos = await _db.Presencas
            .Include(p => p.Turma)
            .Where(p => p.AlunoId == AlunoSelecionado.Id)
            .OrderByDescending(p => p.DataHora)
            .Take(15)
            .ToListAsync();

        Ranking = TodosAlunos
            .Where(a => a.Status == "Ativo")
            .OrderByDescending(a => a.Presencas.Count)
            .ThenByDescending(a => a.AulasNaGraduacaoAtual)
            .ToList();

        PosicaoRanking = Ranking.FindIndex(a => a.Id == AlunoSelecionado.Id) + 1;
    }
}