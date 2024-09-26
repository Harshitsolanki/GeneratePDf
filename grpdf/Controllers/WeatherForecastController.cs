using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
            List<byte[]> images = new List<byte[]>();
            //string rootDir = "C:\\Users\\HP\\source\\repos\\grpdf\\grpdf\\";
            string rootDir = "/root/app/qweigh/GRPDF/";
            string imageDir = rootDir+ "swb-images/";
            _logger.LogInformation("Start");
            string htmlbody = string.Empty;
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<!DOCTYPE html>");
            htmlBuilder.Append("<html>");
            htmlBuilder.Append("<head>");
            htmlBuilder.Append("</head>");
            htmlBuilder.Append("<body>");
            htmlBuilder.Append("<table style='width:940px'>");
            int cols = 2;
            string[] files = null;
            if (Directory.Exists(imageDir))
            {
                string folderPath = imageDir;
                files = Directory.GetFiles(folderPath);

                if (files != null && files.Length > 0)
                {
                    Array.Sort(files);
                    foreach (string file in files)
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(file);
                        images.Add(bytes);
                    }

                }
            }
            if (images != null)
            {
                if (images.Count > 0)
                {
                    decimal imgcount = images.Count;
                    Int64 imgcountdividecount = Convert.ToInt64(Math.Ceiling(imgcount / 2));
                    int imgvalue = 0;
                    for (int i = 0; i < imgcountdividecount; i++)
                    {
                        htmlBuilder.Append("<tr style='page-break-inside: avoid;'>"); // Added CSS to avoid page break inside rows
                        for (int j = 0; j < cols; j++)
                        {
                            if (imgvalue == images.Count)
                            {
                                break;
                            }
                            string base64img = Convert.ToBase64String(images[imgvalue]);
                            // Append the image HTML

                            htmlBuilder.Append($"<td><img style='width:100%' src='data:image/png;base64,{base64img}' alt='Image {imgvalue}'></td>");
                            imgvalue += 1;
                        }
                        htmlBuilder.Append("</tr>");
                    }
                    htmlBuilder.Append("</table>");
                    htmlBuilder.Append("</body>");
                    htmlBuilder.Append("</html>");
                    htmlbody = htmlBuilder.ToString();
                }
            }


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
            HtmlContent = htmlbody,
            WebSettings = { DefaultEncoding = "utf-8" },
            HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
        }
    }
            };
            _logger.LogInformation("Before convert");
            byte[] pdf = _converter.Convert(doc);
            _logger.LogInformation("After convert"+ pdf.Length);
            string filePath = Path.Combine(rootDir, DateTime.Now.ToShortDateString().Replace("-", "").Replace("/", ""));


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
