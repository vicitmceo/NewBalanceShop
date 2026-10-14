using FirebaseAdmin.Auth;
using NewBalanceShop.Application.Interfaces;

namespace NewBalanceShop.Infrastructure.Services;

/// <summary>Перевірка Firebase ID-токена: підпис звіряється з публічними ключами Google.</summary>
public class FirebaseGoogleTokenVerifier : IGoogleTokenVerifier
{
    public async Task<GoogleIdentity> VerifyAsync(string idToken)
    {
        if (FirebaseAuth.DefaultInstance is null)
            throw new InvalidOperationException("Firebase не налаштовано на сервері (FIREBASE_SERVICE_ACCOUNT_JSON).");

        FirebaseToken decoded;
        try
        {
            decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Недійсний токен Google.");
        }

        var email = decoded.Claims.TryGetValue("email", out var emailClaim) ? emailClaim.ToString() : null;
        var name = decoded.Claims.TryGetValue("name", out var nameClaim) ? nameClaim.ToString() : null;
        return new GoogleIdentity(decoded.Uid, email, name);
    }
}
