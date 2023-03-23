using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Financal_API.Data;
using Financal_API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace Financal_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoldPricesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public GoldPricesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// 범위에 해당하는 금 가격을 DB에서 조회한다.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpGet("GetGoldPricesInRange")]
        public async Task<ActionResult<IEnumerable<GoldPrice>>> GetGoldPricesInRange(DateTime startDate, DateTime endDate)
        {
            // Get the GoldPrice objects that fall within the specified date range
            List<GoldPrice> goldPrices = await _context.GoldPrice
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .ToListAsync();

            // Return the GoldPrice objects as JSON
            return Ok(goldPrices);
        }

        /// <summary>
        /// 모든 금가격(USD)를 DB에 저장한다.
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetAllGoldDataToDb")]
        public async Task<ActionResult<IEnumerable<GoldPrice>>> SetAllGoldDataToDb()
        {
            try
            {
                // Check if there is any existing data in the GoldPrice table
                bool isAnyExistingData = await _context.GoldPrice.AnyAsync();

                // Define the FRED API endpoint and API key
                string apiKey = _configuration.GetValue<string>("NasdaqApiOptions:ApiKey");

                string url = $"https://data.nasdaq.com/api/v3/datasets/LBMA/GOLD.json?api_key={apiKey}";

                // Download the JSON data from the FRED API
                using HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                // Parse the JSON data into a list of GoldPrice objects
                JObject data = JObject.Parse(json);
                List<GoldPrice> goldPrices = new List<GoldPrice>();
                foreach (var item in data["dataset"]["data"])
                {
                    decimal? value = (decimal?)item[1]; // nullable decimal
                    if (value != null)
                    {
                        goldPrices.Add(new GoldPrice
                        {
                            Date = DateTime.Parse((string)item[0]),
                            Value = (decimal)value
                        });
                    }
                }

                if (isAnyExistingData)
                {
                    // If there is any existing data in the table, remove it all before adding new data
                    _context.GoldPrice.RemoveRange(_context.GoldPrice);
                    await _context.SaveChangesAsync();
                }

                // Add the GoldPrice objects to the database and save changes
                _context.GoldPrice.AddRange(goldPrices);
                await _context.SaveChangesAsync();
                // 반환값 수정
                return Ok(new { success = true, message = "모든 금 가격이 DB에 저장되었습니다." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// DB에 없는 최근 날짜까지의 금 가격을 추가 INSERT한다.
        /// </summary>
        /// <returns></returns>
        [HttpPost("UpdateDailyGoldData")]
        public async Task<IActionResult> UpdateDailyGoldData()
        {
            try
            {
                // Define the FRED API endpoint and API key
                string apiKey = _configuration.GetValue<string>("NasdaqApiOptions:ApiKey");

                // Get the most recent date in the GoldPrice table
                DateTime? mostRecentDate = await _context.GoldPrice.OrderByDescending(p => p.Date).Select(p => p.Date).FirstOrDefaultAsync();

                // Define the start and end dates for the API request
                DateTime startDate = mostRecentDate?.AddDays(1) ?? new DateTime(1968, 1, 2);
                DateTime endDate = DateTime.Today;

                string url = $"https://data.nasdaq.com/api/v3/datasets/LBMA/GOLD.json?start_date={startDate.ToString("yyyy-MM-dd")}&end_date={endDate.ToString("yyyy-MM-dd")}&api_key={apiKey}";

                // Download the JSON data from the FRED API
                using HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                // Parse the JSON data into a list of GoldPrice objects
                JObject data = JObject.Parse(json);
                List<GoldPrice> goldPrices = new List<GoldPrice>();
                foreach (var item in data["dataset"]["data"])
                {
                    decimal? value = (decimal?)item[1]; // nullable decimal
                    if (value != null)
                    {
                        DateTime date = DateTime.Parse((string)item[0]);
                        if (date > mostRecentDate)
                        {
                            goldPrices.Add(new GoldPrice
                            {
                                Date = date,
                                Value = (decimal)value
                            });
                        }
                    }
                }

                // Add the new GoldPrice objects to the database and save changes
                if (goldPrices.Count > 0)
                {
                    _context.GoldPrice.AddRange(goldPrices);
                    await _context.SaveChangesAsync();

                    // 반환값 수정: 200 OK와 함께 메시지를 반환합니다.
                    return Ok(new { message = $"{goldPrices.Count}개의 금 가격이 {startDate.ToString("yyyy-MM-dd")}부터 {endDate.ToString("yyyy-MM-dd")}까지 업데이트되었습니다." });
                }
                else
                {
                    // 반환값 수정: 204 No Content를 반환하고, 필요한 경우 응답 메시지를 Content 속성에 추가합니다.
                    return StatusCode(StatusCodes.Status204NoContent, new { message = "새로운 금 가격이 없습니다." });
                }
            }
            catch (Exception ex)
            {
                // 예외 처리: 500 Internal Server Error를 반환하고, 예외 메시지를 포함한 JSON 응답을 반환합니다.
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// 모든 금 가격을 DB에서 삭제한다.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("DeleteAllGoldData")]
        public async Task<IActionResult> DeleteAllGoldData()
        {
            // Remove all GoldPrice objects from the database
            _context.GoldPrice.RemoveRange(_context.GoldPrice);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
