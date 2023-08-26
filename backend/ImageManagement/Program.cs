using Amazon;
using Amazon.S3;
using ImageManagement.Data;
using ImageManagement.Data.Repositories;
using ImageManagement.Utilities;
using MongoDB.Driver;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Register the MongoDbContext
builder.Services.AddSingleton(provider =>
{
    var awsSecretManagerSettings = builder.Configuration.GetSection("AWSSecretManager").Get<AWSSecretManagerSettings>();
    var secretsService = new SecretsService(awsSecretManagerSettings.SecretNameMongoDbSettings, awsSecretManagerSettings.Region);

    return new Lazy<Task<IMongoDatabase>>(async () =>
    {
        var secretJson = await secretsService.GetSecretAsync();
        var mongoSettings = JsonSerializer.Deserialize<MongoDbSettings>(secretJson);
        var settings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        var client = new MongoClient(settings);
        return client.GetDatabase(mongoSettings.DatabaseName);
    });
});


builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddTransient<IImageFileRepository, MongoImageFileRepository>();



// Configure AWS client
builder.Services.AddSingleton(provider =>
{
    var awsSecretManagerSettings = builder.Configuration.GetSection("AWSSecretManager").Get<AWSSecretManagerSettings>();
    var secretsService = new SecretsService(awsSecretManagerSettings.SecretNameAWSSettings, awsSecretManagerSettings.Region);

    return new Lazy<Task<IAmazonS3>>(async () =>
    {
        var secretJson = await secretsService.GetSecretAsync();
        var awsSettings = JsonSerializer.Deserialize<AWSSettings>(secretJson);
        var client = new AmazonS3Client(awsSettings.AWS_Access_key, awsSettings.AWS_Secret_Key, RegionEndpoint.GetBySystemName(awsSettings.Region));
        return client;
    });
});



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Configure logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

var dbContext = app.Services.GetRequiredService<MongoDbContext>();
dbContext.InitializeAsync().Wait();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
