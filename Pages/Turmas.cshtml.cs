using ForjaTeamApp.Data;
using ForjaTeamApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Pages;

[Authorize(Roles = "Admin")]
public class TurmasModel : PageModel
{
    private readonly AppDbContext _db;

    public TurmasModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public Turma NovaTurma { get; set; } = new();

    public List<Turma> ListaTurmas { get; set; } = new();

    [TempData]
    public string? Mensagem { get; set; }

    public async Task OnGetAsync()
    {
        ListaTurmas = await _db.Turmas
            .Include(t => t.Presencas)
            .OrderBy(t => t.Id)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        _db.Turmas.Add(NovaTurma);
        await _db.SaveChangesAsync();
        Mensagem = $"Turma {NovaTurma.Modalidade} ({NovaTurma.DiaSemana} {NovaTurma.Horario}) adicionada!";
        return RedirectToPage();
    }
}