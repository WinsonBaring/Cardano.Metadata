using Metadata.Workers;
using Github.Client.Interface;
using Github.Client.Service;

using Carter;
using Metadata.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IGitHubClient, GithubClient>(sp =>
{
    string githubPAT = builder.Configuration.GetValue<string>("Github:PAT") ?? throw new Exception("GITHUB_PAT is not set");
    string githubOwner = builder.Configuration.GetValue<string>("Github:Owner") ?? throw new Exception("GITHUB_OWNER is not set");
    string githubRepo = builder.Configuration.GetValue<string>("Github:Repo") ?? throw new Exception("GITHUB_REPO is not set");
    
    return new GithubClient(githubOwner, githubRepo, githubPAT);
});
builder.Services.AddHostedService<GithubWorker>();

// Register Carter's services
builder.Services.AddCarter();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<TokenMetaDataDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapCarter();
app.Run();
