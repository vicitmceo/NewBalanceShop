using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;

namespace NewBalanceShop.Presentation.Security;

/// <summary>Читання поточного покупця з сесійної куки (ту саму куку ставить AuthController).</summary>
public static class SessionAuth
{
    /// <summary>Ключ сесії з ключем залогіненого покупця.</summary>
    public const string CustomerIdSessionKey = "customerId";

    /// <summary>Поточний покупець або <c>null</c>, якщо вхід не виконано або обліковий запис заблоковано/видалено.</summary>
    public static async Task<CustomerDto?> GetCurrentAsync(HttpContext http, ICustomerService customers)
    {
        var id = http.Session.GetInt32(CustomerIdSessionKey);
        if (id is null) return null;

        var customer = await customers.GetByIdAsync(id.Value);
        return customer is { IsBlocked: false } ? customer : null;
    }
}

/// <summary>Пропускає лише адміністратора: 401 без входу, 403 для звичайного покупця.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireAdminAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var me = await SessionAuth.GetCurrentAsync(
            context.HttpContext, context.HttpContext.RequestServices.GetRequiredService<ICustomerService>());
        if (me is null)
            context.Result = new UnauthorizedResult();
        else if (!me.IsAdmin)
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
    }
}
