using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace grpdf.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IConverter _converter;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConverter converter)
        {
            _logger = logger;
            _converter = converter;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("Start");
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Landscape,
        PaperSize = PaperKind.A4Plus,
    },
                Objects = {
        new ObjectSettings() {
            PagesCount = true,
            HtmlContent = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. In consectetur mauris eget ultrices  iaculis. Ut                               odio viverra, molestie lectus nec, venenatis turpis.",
            WebSettings = { DefaultEncoding = "utf-8" },
            HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
        }
    }
            };
            _logger.LogInformation("Before convert");
            byte[] pdf = _converter.Convert(doc);
            _logger.LogInformation("After convert"+ pdf.Length);
            string filePath = Path.Combine("/root/app/qweigh/GRPDF/", DateTime.Now.ToShortDateString().Replace("-", "").Replace("/", ""));


            _logger.LogInformation(filePath);
            string outputPath = filePath + "output.pdf";
            _logger.LogInformation(outputPath);
            // Write the byte array to the PDF file
            System.IO.File.WriteAllBytesAsync(outputPath, pdf);
            _logger.LogInformation("File Written");
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = outputPath
            })
            .ToArray();
        }
    }
}
