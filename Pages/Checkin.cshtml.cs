using ForjaTeamApp.Data;
using ForjaTeamApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Pages;

public class CheckinModel : PageModel
{
    private readonly AppDbContext _db;

    public CheckinModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public int AlunoId { get; set; }

    [BindProperty]
    public int TurmaId { get; set; }

    [BindProperty]
    public string Origem { get; set; } = "App";

    public List<Aluno> Alunos { get; set; } = new();
    public List<Turma> Turmas { get; set; } = new();
    public List<Presenca> PresencasHoje { get; set; } = new();

    [TempData]
    public string? MensagemSucesso { get; set; }

    [TempData]
    public string? MensagemErro { get; set; }

    public async Task OnGetAsync()
    {
        await CarregarDadosAsync();
    }

   public async Task<IActionResult> OnPostAsync()
    {
        var aluno = await _db.Alunos.FindAsync(AlunoId);
        var turma = await _db.Turmas.FindAsync(TurmaId);

        if (aluno == null || turma == null)
        {
            MensagemErro = "Selecione um aluno e uma turma válidos.";
            return RedirectToPage();
        }

        if (aluno.Status != "Ativo")
        {
            MensagemErro = $"Check-in bloqueado: {aluno.Nome} está com status '{aluno.Status}'.";
            return RedirectToPage();
        }

        var presenca = new Presenca
        {
            AlunoId = aluno.Id,
            TurmaId = turma.Id,
            DataHora = DateTime.Now,
            Origem = Origem,
            Validada = true
        };

        aluno.AulasNaGraduacaoAtual += 1;

        _db.Presencas.Add(presenca);
        await _db.SaveChangesAsync();

        MensagemSucesso = $"Check-in confirmado para {aluno.Nome} ({turma.Modalidade} {turma.Horario}) via {Origem}! Evolução: {aluno.AulasNaGraduacaoAtual}/{aluno.MetaAulasGraduacao} aulas ({aluno.PercentualEvolucao}%).";

        return RedirectToPage();
    }

    private async Task CarregarDadosAsync()
    {
        Alunos = await _db.Alunos
            .OrderBy(a => a.Nome)
            .ToListAsync();

        Turmas = await _db.Turmas
            .OrderBy(t => t.Id)
            .ToListAsync();

        PresencasHoje = await _db.Presencas
            .Include(p => p.Aluno)
            .Include(p => p.Turma)
            .Where(p => p.DataHora.Date == DateTime.Today)
            .OrderByDescending(p => p.DataHora)
            .ToListAsync();
    }
}