using ForjaTeamApp.Data;
using ForjaTeamApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Pages;

[Authorize(Roles = "Admin")]
public class AlunosModel : PageModel
{
    private readonly AppDbContext _db;

    public AlunosModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public Aluno NovoAluno { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Busca { get; set; }

    public List<Aluno> ListaAlunos { get; set; } = new();

    [TempData]
    public string? Mensagem { get; set; }

    public async Task OnGetAsync()
    {
        var query = _db.Alunos
            .Include(a => a.Presencas)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Busca))
        {
            var termo = Busca.Trim().ToLower();
            query = query.Where(a => a.Nome.ToLower().Contains(termo) || a.Cpf.Contains(termo));
        }

        ListaAlunos = await query
            .OrderBy(a => a.Nome)
            .ToListAsync();
    }
    public async Task<IActionResult> OnPostCriarAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        NovoAluno.DataCadastro = DateTime.Now;
        _db.Alunos.Add(NovoAluno);
        await _db.SaveChangesAsync();

        Mensagem = $"Aluno {NovoAluno.Nome} cadastrado com sucesso!";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGraduarAsync(int id, string novaGraduacao)
    {
        var aluno = await _db.Alunos.FindAsync(id);
        if (aluno != null && !string.IsNullOrWhiteSpace(novaGraduacao))
        {
            aluno.GraduacaoAtual = novaGraduacao.Trim();
            aluno.AulasNaGraduacaoAtual = 0;
            await _db.SaveChangesAsync();
            Mensagem = $"{aluno.Nome} graduado para {aluno.GraduacaoAtual}! Contador reiniciado.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAlternarStatusAsync(int id)
    {
        var aluno = await _db.Alunos.FindAsync(id);
        if (aluno != null)
        {
            aluno.Status = aluno.Status == "Ativo" ? "Inativo" : "Ativo";
            await _db.SaveChangesAsync();
            Mensagem = $"Status de {aluno.Nome} alterado para {aluno.Status}.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExcluirAsync(int id)
    {
        var aluno = await _db.Alunos.FindAsync(id);
        if (aluno != null)
        {
            _db.Alunos.Remove(aluno);
            await _db.SaveChangesAsync();
            Mensagem = $"Aluno {aluno.Nome} removido.";
        }
        return RedirectToPage();
    }
}