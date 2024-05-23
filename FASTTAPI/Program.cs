var builder = WebApplication.CreateBuilder(args);
const string FasttApiCorsPolicyName = "FASTTAPICORSPolicy";
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: FasttApiCorsPolicyName,
    policy =>
    {
        var prodSiteUrl = builder.Configuration.GetValue<string>("ProductionSiteUrl");
        var devSiteUrl = builder.Configuration.GetValue<string>("DevSiteUrl");

        if (!string.IsNullOrWhiteSpace(prodSiteUrl))
        {
            policy.WithOrigins(prodSiteUrl).WithMethods("GET", "POST").WithHeaders("*");
        }
        if (!string.IsNullOrWhiteSpace(devSiteUrl))
        {
            policy.WithOrigins(devSiteUrl).WithMethods("GET", "POST").WithHeaders("*");
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(FasttApiCorsPolicyName);


app.UseAuthorization();

app.MapControllers();

app.Run();
