namespace asp_net_core_no_template
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Add middleware.
            app.Use(async (context, next) =>
            {
                var start_time = DateTime.UtcNow;

                // Before endpoint.
                Console.WriteLine(
                    $"Received request: {context.Request.Method} {context.Request.Path}"
                );

                // Execute the next middleware or endpoint, then resume here after it finishes.                
                await next();

                // After endpoint.
                var end_time = DateTime.UtcNow;
                var elapsed_ms = end_time - start_time;

                Console.WriteLine(
                    $"Request processed: {context.Request.Method} {context.Request.Path}\n" +
                    $"Status: {context.Response.StatusCode}\n" + 
                    $"Elapsed time: {elapsed_ms}ms"
                );
            });

            app.MapGet("/", () => "Replying to GET /");

            // Display details of incoming request.
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
            app.MapGet("status/{service}", (string service) =>
            {
                // Assume our web app provides a service called SimulatedDatabase.
                if (service == "SimulatedDatabase")
                {
                    return Results.Ok(new
                    {
                        Service = service,
                        Status = "OK"
                    }
                        );
                }
                else
                {
                    return Results.NotFound(new
                    {
                        Service = service,
                        Error = "Service not found"
                    }

                        );
                }
            });
            app.Run();
        }
    }
}
