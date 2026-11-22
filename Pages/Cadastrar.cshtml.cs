using ForjaTeamApp.Data;
using ForjaTeamApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Pages;

public class CadastrarModel : PageModel
{
    private readonly AppDbContext _db;

    public CadastrarModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string Cpf { get; set; } = string.Empty;

    [BindProperty]
    public string Senha { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmarSenha { get; set; } = string.Empty;

    public string? Erro { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var cpfLimpo = new string((Cpf ?? "").Where(char.IsDigit).ToArray());

        if (cpfLimpo.Length != 11 || string.IsNullOrWhiteSpace(Senha))
        {
            Erro = "Informe um CPF válido (11 dígitos) e crie uma senha.";
            return Page();
        }

        if (Senha.Length < 4)
        {
            Erro = "A senha deve ter pelo menos 4 caracteres.";
            return Page();
        }

        if (Senha != ConfirmarSenha)
        {
            Erro = "As senhas não conferem.";
            return Page();
        }

        var alunos = await _db.Alunos.ToListAsync();
        var aluno = alunos.FirstOrDefault(a =>
            new string((a.Cpf ?? "").Where(char.IsDigit).ToArray()) == cpfLimpo);

        if (aluno == null)
        {
            Erro = "CPF não cadastrado na academia. Solicite seu cadastro à administração.";
            return Page();
        }

        if (aluno.Status != "Ativo")
        {
            Erro = $"Seu cadastro está '{aluno.Status}'. Fale com a administração.";
            return Page();
        }

        if (!string.IsNullOrEmpty(aluno.SenhaHash))
        {
            Erro = "Este CPF já possui senha cadastrada. Vá para a tela de login.";
            return Page();
        }

        aluno.SenhaHash = SenhaHelper.GerarHash(cpfLimpo, Senha);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Login", new { cadastrado = true });
    }
}