namespace asp_net_core_no_template
{
    public class Program
    {        
        public static void Main(string[] args)
        {
            var services = new[]
            {
                new { Name = "MockApi", State = "OK", Dependencies = new[] { "MockCache" } },
                new { Name = "MockCache", State = "Degraded", Dependencies = new[] { "MockDb" } },
                new { Name = "MockDb", State = "OK", Dependencies = Array.Empty<string>() }
            };

            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Add middleware.
            app.Use(async (context, next) =>
            {             
                // <<<<<<<<-----[ Before endpoint ]----->>>>>>>>

                // Conditional middleware. Don't log for echo requests.
                var path = context.Request.Path;
                var should_log = !path.StartsWithSegments("/echo");

                if (should_log)
                {
                    Console.WriteLine(
                        $"Received request: {context.Request.Method} {context.Request.Path}"
                    );
                }

                var start_time = DateTime.UtcNow;

                // Execute the next middleware or endpoint, then resume here after it finishes.                
                await next();

                // <<<<<<<<-----[ After endpoint ]----->>>>>>>>
                var end_time = DateTime.UtcNow;
                var elapsed_ms = end_time - start_time;

                if (should_log)
                {
                    Console.WriteLine(
                        $"Request processed: {context.Request.Method} {context.Request.Path}\n" +
                        $"Status: {context.Response.StatusCode}\n" +
                        $"Elapsed time: {elapsed_ms}ms"
                    );
                }                
            });

            app.MapGet("/", () => "Replying to GET /");

            // Display details of incoming request.
            // Try:
            // /echo
            // /echo?name=bob&age=8
            app.MapGet("/echo", (HttpContext context) =>
            {
                return Results.Ok(new
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path.Value,
                    Query = context.Request.Query.ToDictionary(
                        q => q.Key,
                        q => q.Value.ToString()
                    )
                });
            });

            // Route values.
            // Try:
            // /status/MockApi
            // /status/MockCache
            // /status/MockDb
            // /status/RealApi
            app.MapGet("status/{service}", (string service) =>
            {
                var match = services.FirstOrDefault(s => string.Equals(s.Name, service, StringComparison.OrdinalIgnoreCase));

                if (match == null)
                {
                    return Results.NotFound(new {Service = service, Error = "Service not found"});
                }

                return Results.Ok(new {Service = match.Name, State = match.State });
            });

            // Simple guideline on when to use route values and when to use query:
            // Route identifies what resource. Query modifies how much data is returned.

            // Route values + Query + Default values
            app.Run();
        }
    }
}
