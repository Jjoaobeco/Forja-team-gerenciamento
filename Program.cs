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

    if (app.Environment.IsDevelopment() && db.Alunos.Count() < 30)
    {
        var nomes = new[]
        {
            "Lucas Silva", "Gabriel Santos", "Mateus Oliveira", "Rafael Souza", "Pedro Lima",
            "Bruno Costa", "Thiago Pereira", "Felipe Alves", "Diego Ferreira", "Gustavo Rodrigues",
            "Mariana Almeida", "Camila Nascimento", "Juliana Araújo", "Fernanda Ribeiro", "Amanda Carvalho",
            "Larissa Gomes", "Beatriz Martins", "Carolina Rocha", "Letícia Barbosa", "Patrícia Dias",
            "Rodrigo Teixeira", "Leonardo Mendes", "Eduardo Freitas", "Vinícius Cardoso", "Henrique Correia",
            "Daniel Monteiro", "André Moreira", "Caio Pinto", "Renan Cavalcanti", "Igor Nogueira"
        };

        var planos = new[] { "Mensal", "Trimestral", "Wellhub", "TotalPass" };
        var random = new Random(42);
        var faltam = 30 - db.Alunos.Count();

        for (int i = 0; i < faltam; i++)
        {
            var indice = db.Alunos.Count() + i;
            var nome = nomes[indice % nomes.Length];
            var cpfNumero = (10000000100L + indice * 137L).ToString("D11");
            var plano = planos[random.Next(planos.Length)];
            var ativo = i % 8 != 0;

            db.Alunos.Add(new Aluno
            {
                Nome = nome,
                Cpf = cpfNumero,
                Status = ativo ? "Ativo" : "Inativo",
                TipoPlano = plano,
                SituacaoPagamento = !ativo ? "Pendente" : (i % 6 == 0 ? "Atrasado" : "Em Dia"),
                AulasNaGraduacaoAtual = random.Next(0, 20),
                MetaAulasGraduacao = 24
            });
        }

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
            motivo = "Presença já registrada ou aguardando aprovação para este aluno nessa turma nas últimas 2 horas.",
            aluno = aluno.Nome,
            turma = $"{turma.Modalidade} ({turma.Horario})",
            aulasNaGraduacaoAtual = aluno.AulasNaGraduacaoAtual
        });
    }

    var origem = string.IsNullOrWhiteSpace(req.Origem)
        ? (ehConvenio ? aluno.TipoPlano : "Api")
        : req.Origem.Trim();

    var pendenteAprovacao = string.Equals(origem, "Aluno", StringComparison.OrdinalIgnoreCase);

    var presenca = new Presenca
    {
        AlunoId = aluno.Id,
        TurmaId = turma.Id,
        DataHora = DateTime.Now,
        Origem = origem,
        Validada = !pendenteAprovacao
    };

    if (!pendenteAprovacao)
    {
        aluno.AulasNaGraduacaoAtual += 1;
    }

    db.Presencas.Add(presenca);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        liberado = !pendenteAprovacao,
        pendente = pendenteAprovacao,
        duplicada = false,
        presencaId = presenca.Id,
        aluno = aluno.Nome,
        turma = $"{turma.Modalidade} ({turma.Horario})",
        origem,
        motivo = pendenteAprovacao ? "Check-in solicitado. Aguardando autorização do administrador." : null,
        aulasNaGraduacaoAtual = aluno.AulasNaGraduacaoAtual,
        metaAulasGraduacao = aluno.MetaAulasGraduacao,
        percentualEvolucao = aluno.PercentualEvolucao
    });
});

// Endpoints exclusivos do Admin para listar, aprovar ou recusar check-ins pendentes
app.MapGet("/api/presencas/pendentes", async (AppDbContext db) =>
{
    var pendentes = await db.Presencas
        .Include(p => p.Aluno)
        .Include(p => p.Turma)
        .Where(p => !p.Validada)
        .OrderByDescending(p => p.DataHora)
        .Select(p => new
        {
            p.Id,
            p.DataHora,
            p.Origem,
            AlunoId = p.AlunoId,
            AlunoNome = p.Aluno != null ? p.Aluno.Nome : "",
            AlunoCpf = p.Aluno != null ? p.Aluno.Cpf : "",
            Turma = p.Turma != null ? $"{p.Turma.Modalidade} ({p.Turma.DiaSemana} - {p.Turma.Horario})" : ""
        })
        .ToListAsync();

    return Results.Ok(pendentes);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/api/presencas/{id:int}/aprovar", async (int id, AppDbContext db) =>
{
    var presenca = await db.Presencas
        .Include(p => p.Aluno)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (presenca == null)
    {
        return Results.NotFound(new { sucesso = false, motivo = "Check-in não encontrado." });
    }

    if (!presenca.Validada)
    {
        presenca.Validada = true;
        if (presenca.Aluno != null)
        {
            presenca.Aluno.AulasNaGraduacaoAtual += 1;
        }
        await db.SaveChangesAsync();
    }

    return Results.Ok(new
    {
        sucesso = true,
        presencaId = presenca.Id,
        aluno = presenca.Aluno?.Nome,
        aulasNaGraduacaoAtual = presenca.Aluno?.AulasNaGraduacaoAtual
    });
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/api/presencas/{id:int}/recusar", async (int id, AppDbContext db) =>
{
    var presenca = await db.Presencas.FirstOrDefaultAsync(p => p.Id == id);
    if (presenca == null)
    {
        return Results.NotFound(new { sucesso = false, motivo = "Check-in não encontrado." });
    }

    db.Presencas.Remove(presenca);
    await db.SaveChangesAsync();

    return Results.Ok(new { sucesso = true, presencaId = id });
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.Run();

record PresencaRequest(string? Cpf, int? AlunoId, int? TurmaId, string? Origem);