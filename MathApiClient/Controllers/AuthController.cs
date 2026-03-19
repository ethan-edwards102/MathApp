using Firebase.Auth;
using MathApiClient.Models;
using MathApiClient.Util;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MathApiClient.Controllers;

public class AuthController : Controller
{
    private FirebaseAuthProvider auth;

    public AuthController()
    {
        auth = new FirebaseAuthProvider
        (
            new FirebaseConfig
            (
                Environment.GetEnvironmentVariable("FirebaseMathApp")
            )
        );
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(LoginModel model)
    {
        try
        {
            await auth.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password);
            
            var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);
            string currentUserId = fbAuthLink.User.LocalId;

            if (currentUserId != null)
            {
                HttpContext.Session.SetString("currentUser", currentUserId);
                return RedirectToAction("Calculate", "Math");
            }
        }
        catch (FirebaseAuthException e)
        {
            var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(e.ResponseData);
            ModelState.AddModelError(string.Empty, firebaseEx.error.message);
            return View(model);
        }
        
        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model)
    {
        try
        {
            var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);
            string currentUserId = fbAuthLink.User.LocalId;

            if (currentUserId != null)
            {
                HttpContext.Session.SetString("currentUser", currentUserId);
                return RedirectToAction("Calculate", "Math");
            }
        }
        catch (FirebaseAuthException e)
        {
            var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(e.ResponseData);
            ModelState.AddModelError(string.Empty, firebaseEx.error.message);
            
            AuthLogger.Instance.LogError
            (
                firebaseEx.error.message
                + " - User: "
                + model.Email + " - IP: "
                + HttpContext.Connection.RemoteIpAddress
                + " - Browser: " + Request.Headers.UserAgent
            );
            
            return View(model);
        }
        
        return View();
    }
    
    [HttpGet]
    public IActionResult LogOut()
    {
        HttpContext.Session.Remove("currentUser");
        return RedirectToAction("Login");
    }
}