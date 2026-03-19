using System.Text;
using MathApiClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace MathAppSelf.Controllers;

public class MathController : Controller
{
    private static HttpClient http = new()
    {
        BaseAddress = new Uri("http://localhost:5265")
    };
    
    public IActionResult Calculate()
    {
        if (!ValidateLogin())
        {
            return RedirectToAction("Login", "Auth");
        }
        
        List<SelectListItem> operations = new List<SelectListItem>
        {
            new() { Value = "1", Text = "+" },
            new() { Value = "2", Text = "-" },
            new() { Value = "3", Text = "*" },
            new() { Value = "4", Text = "/" },
        };

        ViewBag.Operations = operations;

        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate
    (
        decimal? FirstNumber,
        decimal? SecondNumber,
        int Operation
    )
    {
        var token = HttpContext.Session.GetString("currentUser");

        if (token == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        decimal? result = 0;
        MathCalculation mathCalculation;

        try
        {
            mathCalculation = MathCalculation.Create(FirstNumber, SecondNumber, Operation, result, token);
        }
        catch (Exception e)
        {
            ViewBag.Error = e.Message;
            return View();
        }

        StringContent jsonContent = new
        (
            JsonConvert.SerializeObject(mathCalculation),
            Encoding.UTF8,
            "application/json"
        );
        
        HttpResponseMessage response = await http.PostAsync("api/Math/PostCalculate", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse =  await response.Content.ReadAsStringAsync();
            MathCalculation? deserializedResponse = JsonConvert.DeserializeObject<MathCalculation>(jsonResponse);
            
            List<SelectListItem> operations = new List<SelectListItem>
            {
                new() { Value = "1", Text = "+" },
                new() { Value = "2", Text = "-" },
                new() { Value = "3", Text = "*" },
                new() { Value = "4", Text = "/" },
            };

            ViewBag.Operations = operations;
            ViewBag.Result = deserializedResponse.Result;
            
            return View();
        }

        ViewBag.Result = "An error has occured";
        return View();
    }
    
    public async Task<IActionResult> History()
    {
        var token = HttpContext.Session.GetString("currentUser");

        if (token == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        // Preserve message after history is cleared
        if (TempData["ClearMessage"] is string message)
        {
            ViewBag.ClearMessage = message;
        }
        
        HttpResponseMessage response = await http.GetAsync($"api/Math/GetHistory?token={token}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            List<MathCalculation>? deserialized = JsonConvert.DeserializeObject<List<MathCalculation>>(json);

            if (deserialized.Count == 0)
            {
                ViewBag.HistoryMessage = "No history to show";
            }
            
            return View(deserialized);
        }

        ViewBag.HistoryMessage = "No history to show";
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var token = HttpContext.Session.GetString("currentUser");

        if (token == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        HttpResponseMessage response = await http.DeleteAsync($"api/Math/DeleteHistory?token={token}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var deserialized = JsonConvert.DeserializeObject<List<MathCalculation>>(json);

            if (deserialized.Count == 0)
            {
                TempData["ClearMessage"] = "No history to delete";
            }
            else
            {
                TempData["ClearMessage"] = $"Deleted {deserialized.Count} items from history";
            }
        }
        else
        {
            TempData["ClearMessage"] = "Something went wrong";
        }
        
        return RedirectToAction("History");
    }

    public bool ValidateLogin()
    {
        var token = HttpContext.Session.GetString("currentUser");

        return token != null;
    }
}