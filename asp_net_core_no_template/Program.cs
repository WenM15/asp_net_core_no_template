namespace asp_net_core_no_template
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

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
