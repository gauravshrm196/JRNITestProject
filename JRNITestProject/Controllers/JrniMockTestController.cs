using JRNITestProject.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace JRNITestProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JrniMockTestController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        public JrniMockTestController()
        {
            _httpClient = new HttpClient();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEventsByEmail(string email)
        {
            try
            {
                var apiUrl = $"https://1uf1fhi7yk.execute-api.eu-west-2.amazonaws.com/default/events?email={email}";
                var response = await _httpClient.GetAsync(apiUrl);

                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:

                        var contentStream = await response.Content.ReadAsStreamAsync();
                        var content = await JsonSerializer.DeserializeAsync<EventApiResponse>(contentStream);

                        var eventsFilteredByStatus = new List<Event>();
                        if (content.Number_of_events > 0)
                        {
                            eventsFilteredByStatus = content.Events.FindAll(e => (e.Status == "Busy" || e.Status == "OutOfOffice") && e.Start_time > DateTime.UtcNow);
                        }

                        return Ok(eventsFilteredByStatus);

                    case System.Net.HttpStatusCode.BadRequest:
                        return StatusCode((int)response.StatusCode, new { message = "Bad Request" });

                    case System.Net.HttpStatusCode.InternalServerError:
                        return StatusCode((int)response.StatusCode, new { message = "Internal Server Error" });

                    case System.Net.HttpStatusCode.ServiceUnavailable:
                        return StatusCode((int)response.StatusCode, new { message = "Gateway TimeOut" });

                    case System.Net.HttpStatusCode.TooManyRequests:
                        return StatusCode((int)response.StatusCode, new { message = "Too Many Attempts" });
                    default:
                        return StatusCode((int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
