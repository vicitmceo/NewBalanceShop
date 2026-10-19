using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NewBalanceShop.Presentation.Middleware;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Controllers;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenNextThrows_Returns500WithGenericJsonAndNoStackTrace()
    {
        var sut = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("secret detail"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await sut.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("Внутрішня помилка сервера", body);
        Assert.DoesNotContain("secret detail", body);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextSucceeds_LeavesResponseUntouched()
    {
        var sut = new ExceptionHandlingMiddleware(ctx => { ctx.Response.StatusCode = 204; return Task.CompletedTask; },
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();

        await sut.InvokeAsync(context);

        Assert.Equal(204, context.Response.StatusCode);
    }
}
