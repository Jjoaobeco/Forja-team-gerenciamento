namespace ForjaTeamApp.Models;

public class Presenca
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int TurmaId { get; set; }
    public Turma? Turma { get; set; }

    public DateTime DataHora { get; set; } = DateTime.Now;

    public string Origem { get; set; } = "App";

    public bool Validada { get; set; } = true;
}