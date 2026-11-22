using System.ComponentModel.DataAnnotations;

namespace ForjaTeamApp.Models;

public class Turma
{
    public int Id { get; set; }

    [Required]
    public string Modalidade { get; set; } = string.Empty;

    [Required]
    public string Categoria { get; set; } = string.Empty;

    [Required]
    public string DiaSemana { get; set; } = string.Empty;

    [Required]
    public string Horario { get; set; } = string.Empty;

    public string Professor { get; set; } = string.Empty;

    public List<Presenca> Presencas { get; set; } = new();
}