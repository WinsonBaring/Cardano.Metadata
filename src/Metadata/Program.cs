using Metadata.Workers;
using Github.Client.Interface;
using Github.Client.Models;
using Github.Client.Service;
using System.Net.Http.Headers;
using System.Reflection;
using Carter;
using Metadata.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.Configure<GithubOptions>(builder.Configuration.GetSection("Github"));

// set up the database context
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("DefaultConnection is not set");
builder.Services.AddDbContext<TokenMetaDataDbContext>(options =>
    options.UseNpgsql(connectionString));

// get the appsettings env
GithubOptions githubOptions = builder.Configuration.GetSection("Github").Get<GithubOptions>() ?? throw new Exception("GITHUB_OWNER is not set");

// set up HTTP client
builder.Services.AddSingleton<IGitHubClient, GithubClient>((serviceProvider) =>
{
    HttpClient httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(githubOptions.GithubClient);
    ProductInfoHeaderValue productValue = new("CardanoTokenMetadataService", Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown Version");
    ProductInfoHeaderValue commentValue = new("(+https://github.com/SAIB-Inc/Cardano.Metadata)");
    httpClient.DefaultRequestHeaders.UserAgent.Add(productValue);
    httpClient.DefaultRequestHeaders.UserAgent.Add(commentValue);
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubOptions.PAT);


    string githubOwner = builder.Configuration.GetValue<string>("Github:Owner") ?? throw new Exception("GITHUB_OWNER is not set");
    string githubRepo = builder.Configuration.GetValue<string>("Github:Repo") ?? throw new Exception("GITHUB_REPO is not set");
    ILogger<GithubClient> logger = serviceProvider.GetRequiredService<ILogger<GithubClient>>();
    
    return new GithubClient(httpClient,githubOwner, githubRepo, logger );
});

// run the github worker
builder.Services.AddHostedService<GithubWorker>();
// builder.Services.AddHostedService<NewGithubWorker>();
// Register Carter's services
builder.Services.AddCarter();


WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapCarter();
app.Run();
