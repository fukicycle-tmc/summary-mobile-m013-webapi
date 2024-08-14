using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/v1/payment", ([FromBody] RequestDTO requestDto) =>
{
    if (requestDto.CardNumber == "1234-5678-1234-0001" &&
        requestDto.ExpireYearMonth == "2029-10" &&
        requestDto.SecurityCode == "333")
    {
        ResponseDTO responseDTO = new ResponseDTO(200, "Success");
        return Results.Extensions._200(responseDTO);
    }
    if (requestDto.CardNumber == "0000-0000-0000-0000")
    {
        ResponseDTO responseDTO = new ResponseDTO(402, "Low credit");
        return Results.Extensions._402(responseDTO);
    }
    try
    {
        DateTime dateTime = DateTime.Parse($"{requestDto.ExpireYearMonth}-01");
        int dim = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
        DateTime expireDateTime = DateTime.Parse($"{dateTime:yyyy-MM}-{dim:00}");
        if (DateTime.Today > expireDateTime)
        {
            ResponseDTO responseDTO = new ResponseDTO(402, "Credit card is expired.");
            return Results.Extensions._402(responseDTO);
        }
        else
        {
            ResponseDTO responseDTO = new ResponseDTO(402, "Something went wrong.");
            return Results.Extensions._402(responseDTO);
        }
    }
    catch (Exception)
    {
        ResponseDTO responseDTO = new ResponseDTO(400, "Your credit card information is invalid.");
        return Results.Extensions._400(responseDTO);
    }
});


app.Run();


record RequestDTO(string CardNumber, string ExpireYearMonth, string SecurityCode, int Price);
record ResponseDTO(int StatusCode, string Message);

static class ResultsExtensions
{
    public static IResult _402(this IResultExtensions resultExtensions, ResponseDTO responseDTO)
    {
        return new _402Result(responseDTO);
    }

    public static IResult _400(this IResultExtensions resultExtensions, ResponseDTO responseDTO)
    {
        return new _400Result(responseDTO);
    }

    public static IResult _200(this IResultExtensions resultExtensions, ResponseDTO responseDTO)
    {
        return new _200Result(responseDTO);
    }
}

class _402Result : IResult
{
    private readonly ResponseDTO _response;
    public _402Result(ResponseDTO response)
    {
        _response = response;
    }
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = 402;
        return httpContext.Response.WriteAsJsonAsync(_response);
    }
}

class _400Result : IResult
{
    private readonly ResponseDTO _response;
    public _400Result(ResponseDTO response)
    {
        _response = response;
    }
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = 400;
        return httpContext.Response.WriteAsJsonAsync(_response);
    }
}

class _200Result : IResult
{
    private readonly ResponseDTO _response;
    public _200Result(ResponseDTO response)
    {
        _response = response;
    }
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = 200;
        return httpContext.Response.WriteAsJsonAsync(_response);
    }
}