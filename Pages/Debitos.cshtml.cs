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

    public DebitosModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Aluno> Alunos { get; set; } = new();
    public decimal ReceitaMensalPrevista { get; set; }
    public int PendentesCount { get; set; }
    public int ConveniosCount { get; set; }

    [TempData]
    public string? Mensagem { get; set; }

    public async Task OnGetAsync()
    {
        Alunos = await _db.Alunos
            .OrderBy(a => a.Nome)
            .ToListAsync();

        var diretos = Alunos.Where(a => a.TipoPlano == "Direto" && a.Status == "Ativo").ToList();
        ReceitaMensalPrevista = diretos.Sum(a => a.ValorMensalidade);
        PendentesCount = diretos.Count(a => a.SituacaoPagamento != "Em Dia");
        ConveniosCount = Alunos.Count(a => a.TipoPlano == "Wellhub" || a.TipoPlano == "TotalPass");
    }

    public async Task<IActionResult> OnPostMarcarPagoAsync(int id)
    {
        var aluno = await _db.Alunos.FindAsync(id);
        if (aluno != null)
        {
            aluno.SituacaoPagamento = "Em Dia";
            aluno.ProximoVencimento = DateTime.Today.AddDays(30);
            await _db.SaveChangesAsync();
            Mensagem = $"Mensalidade de {aluno.Nome} confirmada! Novo vencimento: {aluno.ProximoVencimento:dd/MM/yyyy}.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarcarPendenteAsync(int id)
    {
        var aluno = await _db.Alunos.FindAsync(id);
        if (aluno != null)
        {
            aluno.SituacaoPagamento = "Pendente";
            await _db.SaveChangesAsync();
            Mensagem = $"Situação de {aluno.Nome} marcada como Pendente.";
        }
        return RedirectToPage();
    }
}