using ForjaTeamApp.Data;
using ForjaTeamApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AreaAluno";
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=forjateam.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Turmas.Any())
    {
        db.Turmas.AddRange(
            new Turma { Modalidade = "Muay Thai", Categoria = "Adulto e Kids", DiaSemana = "Segunda", Horario = "07:30", Professor = "Professor Rômulo" },
            new Turma { Modalidade = "Jiu-Jitsu", Categoria = "Infantil", DiaSemana = "Segunda", Horario = "18:00", Professor = "Professora Fabiana" },
            new Turma { Modalidade = "Muay Thai", Categoria = "Feminino", DiaSemana = "Segunda", Horario = "19:00", Professor = "Professora Fabiana" },
            new Turma { Modalidade = "Muay Thai / Boxe", Categoria = "Adulto", DiaSemana = "Segunda", Horario = "20:00", Professor = "Professor Rômulo" },

            new Turma { Modalidade = "Muay Thai / Boxe", Categoria = "Adulto", DiaSemana = "Terça", Horario = "19:00", Professor = "Professor Rômulo" },
            new Turma { Modalidade = "Jiu-Jitsu", Categoria = "Adulto", DiaSemana = "Terça", Horario = "20:00", Professor = "Professor Rômulo" },

            new Turma { Modalidade = "Muay Thai", Categoria = "Adulto e Kids", DiaSemana = "Quarta", Horario = "07:30", Professor = "Professor Rômulo" },
            new Turma { Modalidade = "Jiu-Jitsu", Categoria = "Baby", DiaSemana = "Quarta", Horario = "16:00", Professor = "Professora Fabiana" },
            new Turma { Modalidade = "Muay Thai / Boxe", Categoria = "Adulto", DiaSemana = "Quarta", Horario = "17:00", Professor = "Professor Rômulo" },
            new Turma { Modalidade = "Muay Thai", Categoria = "Kids", DiaSemana = "Quarta", Horario = "18:00", Professor = "Professora Fabiana" },
            new Turma { Modalidade = "Muay Thai", Categoria = "Feminino", DiaSemana = "Quarta", Horario = "19:00", Professor = "Professora Fabiana" },
            new Turma { Modalidade = "Muay Thai / Boxe", Categoria = "Adulto", DiaSemana = "Quarta", Horario = "20:00", Professor = "Professor Rômulo" },

            new Turma { Modalidade = "Jiu-Jitsu", Categoria = "Infantil", DiaSemana = "Quinta", Horario = "18:00", Professor = "Professora Fabiana" },
            new Turma { Modalidade = "Muay Thai / Boxe", Categoria = "Adulto", DiaSemana = "Quinta", Horario = "19:00", Professor = "Professor Rômulo" },
            new Turma { Modalidade = "Jiu-Jitsu", Categoria = "Adulto", DiaSemana = "Quinta", Horario = "20:00", Professor = "Professor Rômulo" },

            new Turma { Modalidade = "Muay Thai / Boxe", Categoria = "Adulto", DiaSemana = "Sexta", Horario = "07:30", Professor = "Professor Rômulo" },
            new Turma { Modalidade = "Jiu-Jitsu", Categoria = "Baby", DiaSemana = "Sexta", Horario = "16:00", Professor = "Professora Fabiana" },
            new Turma { Modalidade = "Muay Thai / Boxe", Categoria = "Adulto", DiaSemana = "Sexta", Horario = "17:00", Professor = "Professor Rômulo" },
            new Turma { Modalidade = "Muay Thai", Categoria = "Kids", DiaSemana = "Sexta", Horario = "18:00", Professor = "Professora Fabiana" },
            new Turma { Modalidade = "Muay Thai / Boxe", Categoria = "Adulto", DiaSemana = "Sexta", Horario = "19:00", Professor = "Professor Rômulo" },
            new Turma { Modalidade = "Jiu-Jitsu", Categoria = "Adulto", DiaSemana = "Sexta", Horario = "20:00", Professor = "Professor Rômulo" }
        );
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages()
   .WithStaticAssets();

app.MapPost("/api/presencas/registrar", async (
    HttpRequest httpRequest,
    IConfiguration config,
    PresencaRequest req,
    AppDbContext db) =>
{
    var chaveEsperada = config["IntegracaoPresenca:ApiKey"] ?? "forja-team-chave-local-2026";
    var chaveRecebida = httpRequest.Headers["X-Api-Key"].FirstOrDefault();

    if (string.IsNullOrWhiteSpace(chaveRecebida) || chaveRecebida != chaveEsperada)
    {
        return Results.Json(new { liberado = false, motivo = "Chave de API inválida ou ausente." }, statusCode: 401);
    }

    if (string.IsNullOrWhiteSpace(req.Cpf) && !req.AlunoId.HasValue)
    {
        return Results.BadRequest(new { liberado = false, motivo = "Informe Cpf ou AlunoId." });
    }

    var cpfLimpo = new string((req.Cpf ?? "").Where(char.IsDigit).ToArray());

    var aluno = req.AlunoId.HasValue
        ? await db.Alunos.FirstOrDefaultAsync(a => a.Id == req.AlunoId.Value)
        : (await db.Alunos.ToListAsync()).FirstOrDefault(a =>
            new string((a.Cpf ?? "").Where(char.IsDigit).ToArray()) == cpfLimpo);

    if (aluno == null)
    {
        return Results.NotFound(new { liberado = false, motivo = "Aluno não encontrado." });
    }

    if (aluno.Status != "Ativo")
    {
        return Results.Json(new { liberado = false, motivo = $"Aluno está '{aluno.Status}'." }, statusCode: 403);
    }

    var ehConvenio = aluno.TipoPlano == "Wellhub" || aluno.TipoPlano == "TotalPass";
    if (!ehConvenio && aluno.SituacaoPagamento != "Em Dia")
    {
        return Results.Json(new { liberado = false, motivo = $"Mensalidade '{aluno.SituacaoPagamento}'." }, statusCode: 403);
    }

    var turma = req.TurmaId.HasValue
        ? await db.Turmas.FindAsync(req.TurmaId.Value)
        : await db.Turmas.OrderBy(t => t.Id).FirstOrDefaultAsync();

    if (turma == null)
    {
        return Results.BadRequest(new { liberado = false, motivo = "Nenhuma turma encontrada." });
    }

    var limiteDuplicidade = DateTime.Now.AddHours(-2);
    var jaRegistrada = await db.Presencas.AnyAsync(p =>
        p.AlunoId == aluno.Id &&
        p.TurmaId == turma.Id &&
        p.DataHora >= limiteDuplicidade);

    if (jaRegistrada)
    {
        return Results.Conflict(new
        {
            liberado = false,
            duplicada = true,
            motivo = "Presença já registrada para este aluno nessa turma nas últimas 2 horas.",
            aluno = aluno.Nome,
            turma = $"{turma.Modalidade} ({turma.Horario})",
            aulasNaGraduacaoAtual = aluno.AulasNaGraduacaoAtual
        });
    }

    var origem = string.IsNullOrWhiteSpace(req.Origem)
        ? (ehConvenio ? aluno.TipoPlano : "Api")
        : req.Origem.Trim();

    var presenca = new Presenca
    {
        AlunoId = aluno.Id,
        TurmaId = turma.Id,
        DataHora = DateTime.Now,
        Origem = origem,
        Validada = true
    };

    aluno.AulasNaGraduacaoAtual += 1;

    db.Presencas.Add(presenca);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        liberado = true,
        duplicada = false,
        aluno = aluno.Nome,
        turma = $"{turma.Modalidade} ({turma.Horario})",
        origem,
        aulasNaGraduacaoAtual = aluno.AulasNaGraduacaoAtual,
        metaAulasGraduacao = aluno.MetaAulasGraduacao,
        percentualEvolucao = aluno.PercentualEvolucao
    });
});

app.Run();

record PresencaRequest(string? Cpf, int? AlunoId, int? TurmaId, string? Origem);