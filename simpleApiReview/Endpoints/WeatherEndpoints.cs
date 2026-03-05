using simpleApiReview.Services;

namespace simpleApiReview.Endpoints
{
    public static class WeatherEndpoints
    {
        public static void MapWeatherEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/weatherforecast", (WeatherService service) =>
            {
                return service.GetWeatherForecasts();
            });
        }   
    }
}