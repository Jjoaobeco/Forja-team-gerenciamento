using ForjaTeamApp.Data;
using ForjaTeamApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Pages;

[Authorize]
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
    public List<Turma> TurmasDisponiveis { get; set; } = new();
    public List<Presenca> HistoricoTreinos { get; set; } = new();
    public List<Aluno> Ranking { get; set; } = new();
    public int PosicaoRanking { get; set; }

    [TempData]
    public string? MensagemSucesso { get; set; }

    [TempData]
    public string? MensagemErro { get; set; }

    public async Task OnGetAsync()
    {
        await CarregarDadosAsync();
    }

    public async Task<IActionResult> OnPostSolicitarCheckInAsync(int turmaId, int? alunoIdForm)
    {
        var ehAdmin = User.IsInRole("Admin");
        int? idAlvo = null;

        if (!ehAdmin)
        {
            var claimAlunoId = User.FindFirst("AlunoId")?.Value;
            if (int.TryParse(claimAlunoId, out var parsedId))
            {
                idAlvo = parsedId;
            }
        }
        else
        {
            idAlvo = alunoIdForm ?? AlunoId;
        }

        if (!idAlvo.HasValue)
        {
            MensagemErro = "Não foi possível identificar o aluno.";
            return RedirectToPage();
        }

        var aluno = await _db.Alunos.FirstOrDefaultAsync(a => a.Id == idAlvo.Value);
        if (aluno == null)
        {
            MensagemErro = "Aluno não encontrado.";
            return RedirectToPage();
        }

        if (aluno.Status != "Ativo")
        {
            MensagemErro = $"Check-in bloqueado: aluno está '{aluno.Status}'.";
            return RedirectToPage(ehAdmin ? new { AlunoId = aluno.Id } : null);
        }

        var ehConvenio = aluno.TipoPlano == "Wellhub" || aluno.TipoPlano == "TotalPass";
        if (!ehConvenio && aluno.SituacaoPagamento != "Em Dia")
        {
            MensagemErro = $"Check-in bloqueado: mensalidade '{aluno.SituacaoPagamento}'.";
            return RedirectToPage(ehAdmin ? new { AlunoId = aluno.Id } : null);
        }

        var turma = await _db.Turmas.FindAsync(turmaId);
        if (turma == null)
        {
            MensagemErro = "Selecione uma turma válida para o check-in.";
            return RedirectToPage(ehAdmin ? new { AlunoId = aluno.Id } : null);
        }

        var limiteDuplicidade = DateTime.Now.AddHours(-2);
        var jaExiste = await _db.Presencas.AnyAsync(p =>
            p.AlunoId == aluno.Id &&
            p.TurmaId == turma.Id &&
            p.DataHora >= limiteDuplicidade);

        if (jaExiste)
        {
            MensagemErro = "Você já solicitou ou registrou check-in nessa turma nas últimas 2 horas.";
            return RedirectToPage(ehAdmin ? new { AlunoId = aluno.Id } : null);
        }

        var presenca = new Presenca
        {
            AlunoId = aluno.Id,
            TurmaId = turma.Id,
            DataHora = DateTime.Now,
            Origem = ehAdmin ? "Admin" : "Aluno",
            Validada = ehAdmin // Se o próprio Admin registrar, já entra validado; se for o aluno, aguarda aprovação
        };

        if (ehAdmin)
        {
            aluno.AulasNaGraduacaoAtual += 1;
        }

        _db.Presencas.Add(presenca);
        await _db.SaveChangesAsync();

        MensagemSucesso = ehAdmin
            ? $"Check-in confirmado para {aluno.Nome} ({turma.Modalidade} - {turma.Horario})."
            : $"Solicitação de check-in enviada para {turma.Modalidade} ({turma.Horario})! Aguarde a aprovação do professor/admin.";

        return RedirectToPage(ehAdmin ? new { AlunoId = aluno.Id } : null);
    }

    private async Task CarregarDadosAsync()
    {
        PodeTrocarAluno = User.IsInRole("Admin");

        TodosAlunos = await _db.Alunos
            .Include(a => a.Presencas)
            .OrderBy(a => a.Nome)
            .ToListAsync();

        TurmasDisponiveis = await _db.Turmas
            .OrderBy(t => t.DiaSemana)
            .ThenBy(t => t.Horario)
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

        // Ranking considera apenas presenças já validadas pelo Admin
        Ranking = TodosAlunos
            .Where(a => a.Status == "Ativo")
            .OrderByDescending(a => a.Presencas.Count(p => p.Validada))
            .ThenByDescending(a => a.AulasNaGraduacaoAtual)
            .ToList();

        PosicaoRanking = Ranking.FindIndex(a => a.Id == AlunoSelecionado.Id) + 1;
    }
}