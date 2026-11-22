using ForjaTeamApp.Data;
using ForjaTeamApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Pages;

[Authorize]
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

    public bool EhAdmin { get; set; }
    public Aluno? AlunoLogado { get; set; }
    public List<Aluno> Alunos { get; set; } = new();
    public List<Turma> Turmas { get; set; } = new();
    public List<Presenca> CheckinsPendentes { get; set; } = new();
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
        var ehAdmin = User.IsInRole("Admin");

        int alunoAlvoId = AlunoId;
        if (!ehAdmin)
        {
            var claimAlunoId = User.FindFirst("AlunoId")?.Value;
            if (!int.TryParse(claimAlunoId, out alunoAlvoId))
            {
                MensagemErro = "Não foi possível identificar o aluno na sessão atual.";
                return RedirectToPage();
            }
        }

        var aluno = await _db.Alunos.FindAsync(alunoAlvoId);
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

        var ehConvenio = aluno.TipoPlano == "Wellhub" || aluno.TipoPlano == "TotalPass";
        if (!ehConvenio && aluno.SituacaoPagamento != "Em Dia")
        {
            MensagemErro = $"Check-in bloqueado: mensalidade '{aluno.SituacaoPagamento}'.";
            return RedirectToPage();
        }

        var limiteDuplicidade = DateTime.Now.AddHours(-2);
        var jaExiste = await _db.Presencas.AnyAsync(p =>
            p.AlunoId == aluno.Id &&
            p.TurmaId == turma.Id &&
            p.DataHora >= limiteDuplicidade);

        if (jaExiste)
        {
            MensagemErro = ehAdmin
                ? $"Já existe check-in registrado ou pendente para {aluno.Nome} nessa turma nas últimas 2 horas."
                : "Você já enviou ou registrou check-in nessa turma nas últimas 2 horas.";
            return RedirectToPage();
        }

        var origemFinal = ehAdmin
            ? (string.IsNullOrWhiteSpace(Origem) ? "App" : Origem)
            : "Aluno";

        // Se for o próprio aluno solicitando, entra como pendente (Validada = false) e não soma aula ainda
        var validada = ehAdmin;

        var presenca = new Presenca
        {
            AlunoId = aluno.Id,
            TurmaId = turma.Id,
            DataHora = DateTime.Now,
            Origem = origemFinal,
            Validada = validada
        };

        if (validada)
        {
            aluno.AulasNaGraduacaoAtual += 1;
        }

        _db.Presencas.Add(presenca);
        await _db.SaveChangesAsync();

        MensagemSucesso = validada
            ? $"Check-in confirmado para {aluno.Nome} ({turma.Modalidade} {turma.Horario}) via {origemFinal}! Evolução: {aluno.AulasNaGraduacaoAtual}/{aluno.MetaAulasGraduacao} aulas ({aluno.PercentualEvolucao}%)."
            : $"Solicitação de check-in enviada para {turma.Modalidade} ({turma.DiaSemana} {turma.Horario})! Aguarde a autorização do administrador.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAprovarAsync(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var presenca = await _db.Presencas
            .Include(p => p.Aluno)
            .Include(p => p.Turma)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (presenca == null)
        {
            MensagemErro = "Solicitação de check-in não encontrada.";
            return RedirectToPage();
        }

        if (!presenca.Validada)
        {
            presenca.Validada = true;
            if (presenca.Aluno != null)
            {
                presenca.Aluno.AulasNaGraduacaoAtual += 1;
            }
            await _db.SaveChangesAsync();
            MensagemSucesso = $"Check-in de {presenca.Aluno?.Nome} autorizado com sucesso!";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRecusarAsync(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var presenca = await _db.Presencas
            .Include(p => p.Aluno)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (presenca == null)
        {
            MensagemErro = "Solicitação de check-in não encontrada.";
            return RedirectToPage();
        }

        var nomeAluno = presenca.Aluno?.Nome ?? "Aluno";
        _db.Presencas.Remove(presenca);
        await _db.SaveChangesAsync();

        MensagemSucesso = $"Check-in de {nomeAluno} recusado e removido.";
        return RedirectToPage();
    }

    private async Task CarregarDadosAsync()
    {
        EhAdmin = User.IsInRole("Admin");

        Alunos = await _db.Alunos
            .OrderBy(a => a.Nome)
            .ToListAsync();

        if (!EhAdmin)
        {
            var claimAlunoId = User.FindFirst("AlunoId")?.Value;
            if (int.TryParse(claimAlunoId, out var idSessao))
            {
                AlunoLogado = Alunos.FirstOrDefault(a => a.Id == idSessao);
                if (AlunoLogado != null)
                {
                    AlunoId = AlunoLogado.Id;
                }
            }
        }

        Turmas = await _db.Turmas
            .OrderBy(t => t.Id)
            .ToListAsync();

        if (EhAdmin)
        {
            CheckinsPendentes = await _db.Presencas
                .Include(p => p.Aluno)
                .Include(p => p.Turma)
                .Where(p => !p.Validada)
                .OrderByDescending(p => p.DataHora)
                .ToListAsync();
        }

        PresencasHoje = await _db.Presencas
            .Include(p => p.Aluno)
            .Include(p => p.Turma)
            .Where(p => p.DataHora.Date == DateTime.Today)
            .OrderByDescending(p => p.DataHora)
            .ToListAsync();
    }
}