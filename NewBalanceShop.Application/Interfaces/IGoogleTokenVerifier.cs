namespace NewBalanceShop.Application.Interfaces;

/// <summary>Особа, підтверджена Google (через Firebase Authentication).</summary>
/// <param name="Uid">Ідентифікатор користувача Firebase.</param>
/// <param name="Email">Email облікового запису Google.</param>
/// <param name="Name">Імʼя з профілю, якщо є.</param>
public record GoogleIdentity(string Uid, string? Email, string? Name);

/// <summary>Перевірка токена входу через Google. Реалізація живе в Infrastructure, щоб Application не залежав від SDK.</summary>
public interface IGoogleTokenVerifier
{
    /// <summary>Перевіряє підпис і строк дії токена.</summary>
    /// <param name="idToken">Firebase ID-токен.</param>
    /// <exception cref="InvalidOperationException">Firebase не налаштовано або токен недійсний.</exception>
    Task<GoogleIdentity> VerifyAsync(string idToken);
}
