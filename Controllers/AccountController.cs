using Leadgen.Model.Entities;
using leadgen.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace leadgen.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return View(await CreateLoginModelAsync(returnUrl));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ExternalLogins = await GetExternalLoginsAsync();
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        model.ExternalLogins = await GetExternalLoginsAsync();
        return View(model);
    }

    [AllowAnonymous]
    [HttpGet("register")]
    public async Task<IActionResult> Register(string? returnUrl = null)
    {
        return View(new RegisterViewModel
        {
            ReturnUrl = returnUrl,
            ExternalLogins = await GetExternalLoginsAsync()
        });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ExternalLogins = await GetExternalLoginsAsync();
            return View(model);
        }

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.DisplayName.Trim(),
            OIB = model.OIB.Trim(),
            JMBG = model.JMBG.Trim()
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(model.ReturnUrl);
        }

        AddIdentityErrors(result);
        model.ExternalLogins = await GetExternalLoginsAsync();
        return View(model);
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [AllowAnonymous]
    [HttpPost("external-login")]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [AllowAnonymous]
    [HttpGet("external-login-callback")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError is not null)
        {
            ModelState.AddModelError(string.Empty, $"External provider error: {remoteError}");
            return View("Login", await CreateLoginModelAsync(returnUrl));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            ModelState.AddModelError(string.Empty, "External login information was not available.");
            return View("Login", await CreateLoginModelAsync(returnUrl));
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            return RedirectToLocal(returnUrl);
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel
        {
            Provider = info.ProviderDisplayName ?? info.LoginProvider,
            Email = email,
            DisplayName = name,
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost("external-login-confirmation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            ModelState.AddModelError(string.Empty, "External login information expired. Start the sign-in again.");
            return View("Login", await CreateLoginModelAsync(model.ReturnUrl));
        }

        if (!ModelState.IsValid)
        {
            model.Provider = info.ProviderDisplayName ?? info.LoginProvider;
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                DisplayName = model.DisplayName.Trim(),
                OIB = model.OIB.Trim(),
                JMBG = model.JMBG.Trim()
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                AddIdentityErrors(createResult);
                model.Provider = info.ProviderDisplayName ?? info.LoginProvider;
                return View(model);
            }
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded && !addLoginResult.Errors.Any(error => error.Code == "LoginAlreadyAssociated"))
        {
            AddIdentityErrors(addLoginResult);
            model.Provider = info.ProviderDisplayName ?? info.LoginProvider;
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToLocal(model.ReturnUrl);
    }

    [AllowAnonymous]
    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task<LoginViewModel> CreateLoginModelAsync(string? returnUrl)
    {
        return new LoginViewModel
        {
            ReturnUrl = returnUrl,
            ExternalLogins = await GetExternalLoginsAsync()
        };
    }

    private async Task<IReadOnlyList<AuthenticationScheme>> GetExternalLoginsAsync()
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        return schemes.ToList();
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}
