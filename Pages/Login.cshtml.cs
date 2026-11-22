using System.Security.Claims;
using ForjaTeamApp.Data;
using ForjaTeamApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Pages;

public class LoginModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public LoginModel(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [BindProperty]
    public string Identificador { get; set; } = string.Empty;

    [BindProperty]
    public string Senha { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public bool Cadastrado { get; set; }

    public string? Erro { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return User.IsInRole("Admin")
                ? RedirectToPage("/Index")
                : RedirectToPage("/AreaAluno");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Identificador))
        {
            Erro = "Informe o CPF (ou usuário) e a senha.";
            return Page();
        }

        var loginLimpo = Identificador.Trim();
        var cpfLimpo = new string(loginLimpo.Where(char.IsDigit).ToArray());

        // 1. Verifica perfil Admin configurado (ex.: appsettings.json -> "AdminPerfil": { "Usuario": "admin", "Senha": "..." })
        var adminUsuario = _config["AdminPerfil:Usuario"] ?? "admin";
        var adminSenha = _config["AdminPerfil:Senha"] ?? "forja2026";
        var adminNome = _config["AdminPerfil:Nome"] ?? "Administração Forja Team";

        if (string.Equals(loginLimpo, adminUsuario, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(Senha) || Senha != adminSenha)
            {
                Erro = "CPF/usuário ou senha inválidos.";
                return Page();
            }

            var claimsAdmin = new List<Claim>
            {
                new(ClaimTypes.Name, adminNome),
                new(ClaimTypes.Role, "Admin")
            };

            var identityAdmin = new ClaimsIdentity(claimsAdmin, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identityAdmin));

            return RedirectToPage("/Index");
        }

        // 2. Verifica cadastro de Aluno por CPF
        if (cpfLimpo.Length == 11)
        {
            var alunos = await _db.Alunos.ToListAsync();
            var aluno = alunos.FirstOrDefault(a =>
                new string((a.Cpf ?? "").Where(char.IsDigit).ToArray()) == cpfLimpo);

            if (aluno != null)
            {
                if (aluno.Status != "Ativo")
                {
                    Erro = $"Cadastro encontrado, mas o status atual é '{aluno.Status}'. Fale com a administração.";
                    return Page();
                }

                // Aluno cadastrado pelo Admin, mas ainda não definiu sua senha
                if (string.IsNullOrWhiteSpace(aluno.SenhaHash))
                {
                    return RedirectToPage("/Cadastrar", new { cpf = cpfLimpo });
                }

                if (!string.IsNullOrWhiteSpace(Senha) && SenhaHelper.Verificar(cpfLimpo, aluno.SenhaHash, Senha))
                {
                    var claimsAluno = new List<Claim>
                    {
                        new(ClaimTypes.Name, aluno.Nome),
                        new(ClaimTypes.Role, "Aluno"),
                        new("AlunoId", aluno.Id.ToString())
                    };

                    var identityAluno = new ClaimsIdentity(claimsAluno, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identityAluno));

                    return RedirectToPage("/AreaAluno");
                }
            }
        }

        Erro = "CPF/usuário ou senha inválidos.";
        return Page();
    }

    public async Task<IActionResult> OnPostSairAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Login");
    }
}