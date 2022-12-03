using AppClases;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using WepApi.Model;

namespace WepApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataMigracionController : ControllerBase
    {
        private readonly ILogger<DataMigracionController> _logger;
        private readonly AppDbContext _dbContext;

        public DataMigracionController(
            ILogger<DataMigracionController> logger,
            AppDbContext dbContext
            )
        {
            _logger = logger;
            _dbContext=dbContext;
        }

        //[HttpGet(Name = "GetDataMigracion")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}

        [HttpPost()]
        public IActionResult PostSync(IList<DataMigacion> data)
        {
            _dbContext.BulkInsert(data);
            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> PostAsync(IList<DataMigacion> data)
        {
            await _dbContext.AddRangeAsync(data);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}