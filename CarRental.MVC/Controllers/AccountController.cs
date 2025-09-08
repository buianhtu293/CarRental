﻿using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using CarRental.Domain.Entities;
using CarRental.Application.Services.Interfaces;
using App.Areas.Identity.Models.AccountViewModels;
using CarRental.Application.Attributes;
using CarRental.Application.ExtendMethod;
using CarRental.Application.Interfaces;
using CarRental.Domain.Enums;

namespace CarRental.Presentation.Controllers
{
	//[Authorize]
	//[Area("Identity")]
	[Route("/Account/[action]")]
	public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole<Guid>> _roleManager;
		private readonly SignInManager<User> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly ILogger<AccountController> _logger;
		private readonly IWalletService _walletService;

		public AccountController(UserManager<User> userManager, 
			RoleManager<IdentityRole<Guid>> roleManager, 
			SignInManager<User> signInManager, 
			IEmailSender emailSender, 
			ILogger<AccountController> logger,
			IWalletService walletService)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_logger = logger;
			_walletService = walletService;
		}

		// GET: /Account/Login
		[HttpGet("/login/")]
		[AllowAnonymous]
		public IActionResult Login(string returnUrl = null)
		{
			returnUrl ??= Url.Content("~/");
			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}

		//
		// POST: /Account/Login
		[HttpPost("/login/")]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
		{
			returnUrl ??= Url.Content("~/");
			ViewData["ReturnUrl"] = returnUrl;

			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(model.UserNameOrEmail, model.Password, model.RememberMe, lockoutOnFailure: true);

				// If login fails, check if input was an email and try finding the username
				User user = null;
				if (!result.Succeeded && AppUtilities.IsValidEmail(model.UserNameOrEmail))
				{
					user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
					if (user != null)
					{
						result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
					}
				}
				else
				{
					user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
				}

				if (result.Succeeded && user != null)
				{
					// Add role claims to the authentication cookie
					await AddRoleClaimsToCookie(user, model.RememberMe);

					_logger.LogInformation(1, "User logged in.");
					return LocalRedirect(returnUrl);
				}

				if (result.RequiresTwoFactor)
				{
					return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
				}

                if (result.IsLockedOut)
                {
                    _logger.LogWarning(2, "Account is locked!");
                    return View("Lockout");
                }

                ModelState.AddModelError(string.Empty, "Can't login.");
            }

			return View(model);
		}

		private async Task AddRoleClaimsToCookie(User user, bool isPersistent)
		{
			// Get current user claims
			var existingClaims = await _userManager.GetClaimsAsync(user);

			// Remove old role claims
			foreach (var claim in existingClaims.Where(c => c.Type == ClaimTypes.Role))
			{
				await _userManager.RemoveClaimAsync(user, claim);
			}

			// Get user roles
			var roles = await _userManager.GetRolesAsync(user);

			// Create a new identity with role claims
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName!)
			};

			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			// Create a new ClaimsIdentity and Principal
			var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
			var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

			// Sign out and sign in again with updated claims
			await _signInManager.SignOutAsync();
			await _signInManager.SignInWithClaimsAsync(user, isPersistent, claims);
		}


        // POST: /Account/LogOff
        [HttpPost("/logout/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logout");
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        //
        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = "",
                    Address = "",
                    Province = "",
                    District = "",
					Ward = ""
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("New user is created.");

					// Assign role based on selected RoleType
					if (!string.IsNullOrEmpty(model.RoleType))
					{
						// Ensure the role exists before adding
						if (model.RoleType.Equals(RoleName.CarOwner))
						{
							await _userManager.AddToRoleAsync(user, RoleName.CarOwner);
						}
						else
						{
							await _userManager.AddToRoleAsync(user, RoleName.Customer);
						}
					}

					// Phát sinh token để xác nhận email
					var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

					// https://localhost:5001/confirm-email?userId=fdsfds&code=xyz&returnUrl=
					var callbackUrl = Url.ActionLink(
						action: nameof(ConfirmEmail),
						values:
							new
							{
								area = "Identity",
								userId = user.Id,
								code = code
							},
						protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email,
    "Email Address Confirmation",
    @$"
    <div style='font-family: Arial, sans-serif; text-align: center; padding: 20px; background-color: #f8f9fa;'>
        <div style='max-width: 500px; margin: auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
            <h2 style='color: #007bff;'>Confirm Your Email</h2>
            <p style='font-size: 16px; color: #333;'>
                Thank you for registering on <strong>CarRental</strong>.  
                Please confirm your email by clicking the button below.
            </p>
            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
               style='display: inline-block; padding: 10px 20px; margin: 20px 0; color: white; background-color: #007bff; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                Verify Email
            </a>
            <p style='color: #666; font-size: 14px;'>
                If you did not request this, please ignore this email.
            </p>
        </div>
    </div>");


					if (_userManager.Options.SignIn.RequireConfirmedAccount)
					{
						return LocalRedirect(Url.Action(nameof(RegisterConfirmation)));
					}
					else
					{
						await _signInManager.SignInAsync(user, isPersistent: false);
						return LocalRedirect(returnUrl);
					}

				}

				ModelState.AddModelError(result);
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		// GET: /Account/ConfirmEmail
		[HttpGet]
		[AllowAnonymous]
		public IActionResult RegisterConfirmation()
		{
			return View();
		}

		// GET: /Account/ConfirmEmail
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ConfirmEmail(string userId, string code)
		{
			if (userId == null || code == null)
			{
				return View("ErrorConfirmEmail");
			}
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return View("ErrorConfirmEmail");
			}
			code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
			var result = await _userManager.ConfirmEmailAsync(user, code);
			
			if (result.Succeeded)
			{
				// Only create wallet after confirmed email
				await ProvisionWalletIfNeededAsync(user.Id);
			}
			
			return View(result.Succeeded ? "ConfirmEmail" : "ErrorConfirmEmail");
		}

		/// <summary>
		/// Ensures that a wallet is provisioned for the specified user. Logs errors if the operation fails.
		/// </summary>
		/// <param name="userId">The unique identifier of the user for whom the wallet should be provisioned.</param>
		/// <remarks>
		///	Idempotent and safe to call multiple times.
		/// </remarks>
		/// <returns>A task representing the asynchronous operation.</returns>
		private async Task ProvisionWalletIfNeededAsync(Guid userId)
		{
			try
			{
				await _walletService.EnsureWalletForUserAsync(userId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to provision wallet for user {UserId}", userId);
			}
		}

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error when using external service: {remoteError}");
                return View(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

			// Sign in the user with this external login provider if the user already has a login.
			var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
			if (result.Succeeded)
			{
				// Cập nhật lại token
				await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

				_logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
				return LocalRedirect(returnUrl);
			}
			if (result.RequiresTwoFactor)
			{
				return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
			}
			if (result.IsLockedOut)
			{
				return View("Lockout");
			}
			else
			{
				// If the user does not have an account, then ask the user to create an account.
				ViewData["ReturnUrl"] = returnUrl;
				ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
				var email = info.Principal.FindFirstValue(ClaimTypes.Email);
				return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
			}
		}

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ;
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

				// Input.Email
				var registeredUser = await _userManager.FindByEmailAsync(model.Email);
				string externalEmail = null;
				User externalEmailUser = null;

				// Claim ~ Dac tinh mo ta mot doi tuong 
				if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
				{
					externalEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
				}

				if (externalEmail != null)
				{
					externalEmailUser = await _userManager.FindByEmailAsync(externalEmail);
				}

				if ((registeredUser != null) && (externalEmailUser != null))
				{
					// externalEmail  == Input.Email
					if (registeredUser.Id == externalEmailUser.Id)
					{
						// Lien ket tai khoan, dang nhap
						var resultLink = await _userManager.AddLoginAsync(registeredUser, info);
						if (resultLink.Succeeded)
						{
							// Add role claims to the authentication cookie
							await AddRoleClaimsToCookie(registeredUser, false);

							await _signInManager.SignInAsync(registeredUser, isPersistent: false);
							return LocalRedirect(returnUrl);
						}
					}
					else
					{
						// registeredUser = externalEmailUser (externalEmail != Input.Email)
						/*
                            info => user1 (mail1@abc.com)
                                 => user2 (mail2@abc.com)
                        */
                        ModelState.AddModelError(string.Empty, "Can't link to this account, Let's use another account");
                        return View();
                    }
                }


                if ((externalEmailUser != null) && (registeredUser == null))
                {
                    ModelState.AddModelError(string.Empty, "Don't support create new account - your email don't match with email of external service");
                    return View();
                }

				if ((externalEmailUser == null) && (externalEmail == model.Email))
				{
					// Chua co Account -> Tao Account, lien ket, dang nhap
					var newUser = new User()
					{
						UserName = externalEmail,
						Email = externalEmail,
                        FullName = "",
                        Address = "",
                        Province = "",
                        District = "",
                        Ward = ""
                    };

					var resultNewUser = await _userManager.CreateAsync(newUser);
					if (resultNewUser.Succeeded)
					{
                        // Assign role based on selected RoleType
                        if (!string.IsNullOrEmpty(model.RoleType))
                        {
                            // Ensure the role exists before adding
                            if (model.RoleType.Equals(RoleName.CarOwner))
                            {
                                await _userManager.AddToRoleAsync(newUser, RoleName.CarOwner);
                            }
                            else
                            {
                                await _userManager.AddToRoleAsync(newUser, RoleName.Customer);
                            }
                        }

                        await _userManager.AddLoginAsync(newUser, info);
						var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
						await _userManager.ConfirmEmailAsync(newUser, code);

						// Wallet creation after confirmation
						await ProvisionWalletIfNeededAsync(newUser.Id);
						
						await _signInManager.SignInAsync(newUser, isPersistent: false);

						return LocalRedirect(returnUrl);

                    }
                    else
                    {
                        ModelState.AddModelError("Can't create new account");
                        return View();
                    }
                }


				var user = new User
				{
					UserName = model.Email,
					Email = model.Email,
                    FullName = "",
                    Address = "",
                    Province = "",
                    District = "",
                    Ward = ""
                };
				var result = await _userManager.CreateAsync(user);
				if (result.Succeeded)
				{
					result = await _userManager.AddLoginAsync(user, info);
					if (result.Succeeded)
					{
						await _signInManager.SignInAsync(user, isPersistent: false);
						_logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);

						// Update any authentication tokens as well
						await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

						// Wallet creation after confirmation
						await ProvisionWalletIfNeededAsync(user.Id);
						
						return LocalRedirect(returnUrl);
					}
				}
				ModelState.AddModelError(result);
			}

			ViewData["ReturnUrl"] = returnUrl;
			return View(model);
		}

		//
		// GET: /Account/ForgotPassword
		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		//
		// POST: /Account/ForgotPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
				{
					// Don't reveal that the user does not exist or is not confirmed
					return View("ForgotPasswordConfirmation");
				}
				var code = await _userManager.GeneratePasswordResetTokenAsync(user);
				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
				var callbackUrl = Url.ActionLink(
					action: nameof(ResetPassword),
					values: new { area = "Identity", code },
					protocol: Request.Scheme);


                await _emailSender.SendEmailAsync(
    model.Email,
    "Reset Your Password",
    @$"
    <div style='font-family: Arial, sans-serif; text-align: center; padding: 20px; background-color: #f8f9fa;'>
        <div style='max-width: 500px; margin: auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
            <h2 style='color: #dc3545;'>Password Reset Request</h2>
            <p style='font-size: 16px; color: #333;'>
                We received a request to reset your password.  
                Click the button below to proceed.
            </p>
            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
               style='display: inline-block; padding: 10px 20px; margin: 20px 0; color: white; background-color: #dc3545; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                Reset Password
            </a>
            <p style='color: #666; font-size: 14px;'>
                If you did not request this, please ignore this email.
            </p>
        </div>
    </div>");


				return RedirectToAction(nameof(ForgotPasswordConfirmation));



			}
			return View(model);
		}

		//
		// GET: /Account/ForgotPasswordConfirmation
		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgotPasswordConfirmation()
		{
			return View();
		}

		//
		// GET: /Account/ResetPassword
		[HttpGet]
		[AllowAnonymous]
		public IActionResult ResetPassword(string code = null)
		{
			return code == null ? View("Error") : View();
		}

		//
		// POST: /Account/ResetPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
			}
			var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));

			var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
			if (result.Succeeded)
			{
				return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
			}
			ModelState.AddModelError(result);
			return View();
		}

		//
		// GET: /Account/ResetPasswordConfirmation
		[HttpGet]
		[AllowAnonymous]
		public IActionResult ResetPasswordConfirmation()
		{
			return View();
		}

		//
		// GET: /Account/SendCode
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
		{
			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				return View("Error");
			}
			var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
			var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
			return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
		}
		//
		// POST: /Account/SendCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SendCode(SendCodeViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				return View("Error");
			}
			// Dùng mã Authenticator
			if (model.SelectedProvider == "Authenticator")
			{
				return RedirectToAction(nameof(VerifyAuthenticatorCode), new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
			}

			// Generate the token and send it
			var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
			if (string.IsNullOrWhiteSpace(code))
			{
				return View("Error");
			}

            var message = @$"
    <div style='font-family: Arial, sans-serif; text-align: center; padding: 20px; background-color: #f8f9fa;'>
        <div style='max-width: 500px; margin: auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
            <h2 style='color: #007bff;'>Security Code</h2>
            <p style='font-size: 16px; color: #333;'>
                Your one-time security code is:
            </p>
            <div style='display: inline-block; padding: 10px 20px; font-size: 20px; font-weight: bold; color: #fff; background-color: #007bff; border-radius: 5px;'>
                {code}
            </div>
            <p style='color: #666; font-size: 14px; margin-top: 10px;'>
                Please use this code to complete your verification.  
                If you did not request this, please ignore this email.
            </p>
        </div>
    </div>";

            if (model.SelectedProvider == "Email")
            {
                await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
            }
            else if (model.SelectedProvider == "Phone")
            {
                await _emailSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
            }

			return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
		}
		//
		// GET: /Account/VerifyCode
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
		{
			// Require that the user has already logged in via username/password or external login
			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				return View("Error");
			}
			return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
		}

		//
		// POST: /Account/VerifyCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
		{
			model.ReturnUrl ??= Url.Content("~/");
			//if (!ModelState.IsValid)
			//{
			//	return View(model);
			//}

			// The following code protects for brute force attacks against the two factor codes.
			// If a user enters incorrect codes for a specified amount of time then the user account
			// will be locked out for a specified amount of time.
			var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
			if (result.Succeeded)
			{
				return LocalRedirect(model.ReturnUrl);
			}
			if (result.IsLockedOut)
			{
				_logger.LogWarning(7, "User account locked out.");
				return View("Lockout");
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Invalid code.");
				return View(model);
			}
		}

		//
		// GET: /Account/VerifyAuthenticatorCode
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> VerifyAuthenticatorCode(bool rememberMe, string returnUrl = null)
		{
			// Require that the user has already logged in via username/password or external login
			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				return View("Error");
			}
			return View(new VerifyAuthenticatorCodeViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe });
		}

		//
		// POST: /Account/VerifyAuthenticatorCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorCodeViewModel model)
		{
			model.ReturnUrl ??= Url.Content("~/");
			//if (!ModelState.IsValid)
			//{
			//	return View(model);
			//}

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return LocalRedirect(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Wrong code.");
                return View(model);
            }
        }
        //
        // GET: /Account/UseRecoveryCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> UseRecoveryCode(string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            return View(new UseRecoveryCodeViewModel { ReturnUrl = returnUrl });
        }

        //
        // POST: /Account/UseRecoveryCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseRecoveryCode(UseRecoveryCodeViewModel model)
        {
            model.ReturnUrl ??= Url.Content("~/");
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.Code);
            if (result.Succeeded)
            {
                return LocalRedirect(model.ReturnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Wrong recovery code.");
                return View(model);
            }
        }

        [Route("/accessdenied.html")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
