


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

#region Swagger

//Configuration so the UI shows a "Required" textbox for the header
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("IdempotencyKey" , new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-Idempotency-Key",
        Type = SecuritySchemeType.ApiKey,
        Description = "Idempotency key for ensuring idempotent requests"
    });
});

#endregion

#region Connection String


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));


#endregion

#region Services

builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

builder.Services.AddScoped<IdempotencyFilter>();
#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

#endregion

#region FluentValidation

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

#endregion

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
