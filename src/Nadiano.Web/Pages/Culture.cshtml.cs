using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Nadiano.Web.Infrastructure.Localization;

namespace Nadiano.Web.Pages;

public class CultureModel : PageModel
{
    [BindProperty(SupportsGet = false)]
    public string Culture { get; set; } = SupportedCultures.Default;

    [BindProperty(SupportsGet = false)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public IActionResult OnPost()
    {
        if (SupportedCultures.IsSupported(Culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(Culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
        }

        return LocalRedirect(string.IsNullOrEmpty(ReturnUrl) ? "/" : ReturnUrl);
    }
}