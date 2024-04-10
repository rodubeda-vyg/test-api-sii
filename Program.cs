using vyg_api_sii.Endpoints;
using vyg_api_sii.Extensions;
using vyg_api_sii.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Extension Methods
builder.Services.AddScoped<HefSignatureExtension>();

// Add services to the container.
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<CesionService>();
builder.Services.AddScoped<ConsultasService>();
builder.Services.AddScoped<PublicacionService>();
builder.Services.AddScoped<SIIService>();

builder.Services.AddControllers();

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

app.UseAuthorization();

app.MapAuthEndpoints();
app.MapTransEndpoints();

app.Run();
