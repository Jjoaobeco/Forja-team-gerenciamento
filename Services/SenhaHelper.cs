using Microsoft.AspNetCore.Identity;

namespace ForjaTeamApp.Services;

public static class SenhaHelper
{
    private static readonly PasswordHasher<string> _hasher = new();

    public static string GerarHash(string cpfLimpo, string senha)
    {
        return _hasher.HashPassword(cpfLimpo, senha);
    }

    public static bool Verificar(string cpfLimpo, string senhaHash, string senhaDigitada)
    {
        if (string.IsNullOrWhiteSpace(senhaHash) || string.IsNullOrWhiteSpace(senhaDigitada))
            return false;

        var resultado = _hasher.VerifyHashedPassword(cpfLimpo, senhaHash, senhaDigitada);
        return resultado == PasswordVerificationResult.Success ||
               resultado == PasswordVerificationResult.SuccessRehashNeeded;
    }
}