using MathAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MathAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MathController : Controller
{
    private readonly MathappContext _context;

    public MathController(MathappContext context)
    {
        _context = context;
    }
    
    [HttpGet("GetCalculate")]
    public Dictionary<string, string> GetCalculate()
    {
        Dictionary<string, string> operations = new()
        {
            ["1"] = "+",
            ["2"] = "-",
            ["3"] = "*",
            ["4"] = "/"
        };

        return operations;
    }
    
    /// <summary>Creates and performs a MathCalculation</summary>
    /// <param name="mathCalculation">a MathCalculation object for processing</param>
    /// <returns>A MathCalculation object with the result</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /PostCalulate
    ///     {
    ///        "FirstNumber": 5,
    ///        "SecondNumber": 5,
    ///        "Operation": 1,
    ///        "FirebaseUuid": "{insert token here}"
    ///     }
    /// </remarks>
    /// <response code="201">Returns the newly created calculation</response>
    /// <response code="400">Returns if a request is missing details or fails</response>
    /// <response code="401">Returns if a request is missing a token</response>
    [HttpPost("PostCalculate")]
    [ProducesResponseType(typeof(MathCalculation), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public async Task<IActionResult> PostCalculate
    (
        MathCalculation calc
    )
    {
        if (string.IsNullOrEmpty(calc.FirebaseUuid))
        {
            return Unauthorized(new Error("Token missing!"));
        }

        if
        (
            calc.FirstNumber == null ||
            calc.SecondNumber == null ||
            calc.Operation == null
        )
        {
            return BadRequest(new Error("Math equation not complete!"));
        }

        try
        {
            calc = MathCalculation.Create
            (
                calc.FirstNumber,
                calc.SecondNumber,
                calc.Operation,
                0,
                calc.FirebaseUuid
            );
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        switch (calc.Operation)
        {
            case 1:
                calc.Result = calc.FirstNumber + calc.SecondNumber;
                break;
            case 2:
                calc.Result = calc.FirstNumber - calc.SecondNumber;
                break;
            case 3:
                calc.Result = calc.FirstNumber * calc.SecondNumber;
                break;
            default:
                calc.Result = calc.FirstNumber / calc.SecondNumber;
                break;
        }
        
        _context.Add(calc);
        await _context.SaveChangesAsync();

        return Created(calc.CalculationId.ToString(), calc);
    }
    
    /// <summary>Gets the MathCalculation history for a user</summary>
    /// <param name="Token">Token of the current user.</param>
    /// <returns>A list of MathCalcuation objects</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /GetHistory
    ///     {
    ///        "Token": "{Insert token here}"
    ///     }
    /// </remarks>
    /// <response code="200">Returns the list of calculations for a user</response>
    /// <response code="400">Returns if a request is missing details or fails</response>
    /// <response code="401">Returns if a request is missing a token</response>
    /// <response code="404">Returns if no history found</response>
    [HttpGet("GetHistory")]
    [ProducesResponseType(typeof(List<MathCalculation>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<IActionResult> GetHistory(string? token)
    {
        if (String.IsNullOrEmpty(token))
        {
            return Unauthorized(new Error("Token missing!"));
        }
        
        var items = await _context.MathCalculations
            .Where
            (
                m => m.FirebaseUuid.Equals(token)
            )
            .ToListAsync();

        if (items.Count > 0)
        {
            return Ok(items);
        }
        else
        {
            return NotFound(new Error("No history found!"));
        }
    }
    
    /// <summary>
    /// Deletes the MathCalculation history for a user
    /// </summary>
    /// <param name="Token">Token of the current user.</param>
    /// <returns>List of deleted items</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /DeleteHistory
    ///     {
    ///        "Token": "{Insert token here}"
    ///     }
    /// </remarks>
    /// <response code="200">Returns the list of calculations deleted for a user</response>
    /// <response code="400">Returns if a request is missing details or fails</response>
    /// <response code="401">Returns if a request is missing a token</response>
    /// <response code="404">Returns if no history found</response>
    [HttpDelete("DeleteHistory")]
    [ProducesResponseType(typeof(List<MathCalculation>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteHistory(string? token)
    {
        if (String.IsNullOrEmpty(token))
        {
            return Unauthorized(new Error("Token missing!"));
        }

        var items = await _context.MathCalculations
            .Where
            (
                m => m.FirebaseUuid.Equals(token)
            )
            .ToListAsync();

        if (items.Count > 0)
        {
            _context.RemoveRange(items);
            await _context.SaveChangesAsync();
            return Ok(items);
        }
        else
        {
            return NotFound(new Error("No history found!"));
        }
    }

    // public bool ValidateLogin()
    // {
    //     var token = HttpContext.Session.GetString("currentUser");
    //
    //     return token != null;
    // }
}