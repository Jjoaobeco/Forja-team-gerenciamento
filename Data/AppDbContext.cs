using ForjaTeamApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ForjaTeamApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Turma> Turmas => Set<Turma>();
    public DbSet<Presenca> Presencas => Set<Presenca>();
}