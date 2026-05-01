var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var validCaseReferences = new HashSet<string>
{
    "CASE_REF001",
    "CASE_REF002",
    "CASE_REF003",
};

app.MapGet("api/redundancy/cases/{caseRefNumber}", (string caseRefNumber) =>
{
    if (string.IsNullOrWhiteSpace(caseRefNumber))
    {
        return Results.BadRequest(new
        {
            found = false,
            message = "Reference required"
        });

    }

    Console.WriteLine($"Wader received case {caseRefNumber}");
    
    var exists = validCaseReferences.Contains(caseRefNumber);
    if (!exists)
    {
        return Results.NotFound(new
        {
            found = false,
            message = "Reference not found"
        });
    }

    return Results.Ok(new
    {
        found = true,
        message = "Reference found"
    });
});

app.Run();