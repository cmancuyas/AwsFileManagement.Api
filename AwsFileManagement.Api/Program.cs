using Amazon.S3;
using AwsFileManagement.Api.Middleware;
using AwsFileManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDefaultAWSOptions(
    builder.Configuration.GetAWSOptions());

builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddScoped<IFileService, S3FileService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();