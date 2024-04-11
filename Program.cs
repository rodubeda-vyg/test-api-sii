using vyg_api_sii.Endpoints;
using vyg_api_sii.Extensions;
using vyg_api_sii.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Extension Methods
builder.Services.AddScoped<HefSignatureExtension>();

// Add services to the container.
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BooksService>();
builder.Services.AddScoped<TransferService>();
builder.Services.AddScoped<ConsultasService>();
builder.Services.AddScoped<PublicacionService>();
builder.Services.AddScoped<SIIService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

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
app.MapBooksEndpoints();
app.MapDocsEndpoints();
app.MapTransEndpoints();

app.Run();
