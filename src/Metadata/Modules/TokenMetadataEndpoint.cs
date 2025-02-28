using Carter;
using Metadata.Data;
using Microsoft.EntityFrameworkCore;
using Metadata.Models.Entity;
using System.Text.Json;

namespace Metadata.Modules;

public class TokenMetadataEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var groupToken = app.MapGroup("/cardano-token");
        var groupSync = app.MapGroup("/sync");

        const string GetSpecificToken = "GetSpecifcTokenMetadata";
        const string GetSpecificState = "GetSpecificState";

        /////////TOKEN
        ///
        //get all token metadata
        groupToken.MapGet("/get-all", async (TokenMetaDataDbContext db) =>
        {
            var token = await db.TokenMetadatas.ToListAsync();
            if (!token.Any()) return Results.NotFound("No token metadata found.");

            var result = token.Select(t => JsonSerializer.Deserialize<JsonElement>(t.Data));
            return Results.Ok(result);
        });

        //filtered get token metadata given params
        groupToken.MapGet("/get", async (TokenMetaDataDbContext db, string? subject, string? name, string? description, string? policy, string? ticker, string? url, string? logo, int? decimals) =>
        {
            var tokens = await db.TokenMetadatas
                .Where(t =>
                    (string.IsNullOrWhiteSpace(subject) || t.Subject.Contains(subject)) &&
                    (string.IsNullOrWhiteSpace(name) || t.Name.Contains(name)) &&
                    (string.IsNullOrWhiteSpace(description) || t.Description.Contains(description)) &&
                    (string.IsNullOrWhiteSpace(policy) || t.Policy.Contains(policy)) &&
                    (string.IsNullOrWhiteSpace(ticker) || t.Ticker.Contains(ticker)) &&
                    (string.IsNullOrWhiteSpace(url) || t.Url.Contains(url)) &&
                    (string.IsNullOrWhiteSpace(logo) || t.Logo.Contains(logo)) &&
                    (!decimals.HasValue || t.Decimals == decimals.Value)
                )
                .ToListAsync();

            if (tokens == null || !tokens.Any())
            {
                return Results.NotFound("No token metadata found.");
            }

            var result = tokens.Select(t => JsonSerializer.Deserialize<JsonElement>(t.Data));
            return Results.Ok(result);
        }).WithName(GetSpecificToken);


        //bulk add
        groupToken.MapPost("/add-all", async (TokenMetaDataDbContext db, List<TokenMetadata> TokenMetadatas) =>
        {
            if (TokenMetadatas.Count() == 0) return Results.NotFound();

            foreach (var token in TokenMetadatas)
            {
                var tokenMetadata = await db.TokenMetadatas.FindAsync(token.Subject);
                if (tokenMetadata is not null) return Results.Conflict($"Token with subject '{token.Subject}' already exists.");
            }

            await db.TokenMetadatas.AddRangeAsync(TokenMetadatas);
            await db.SaveChangesAsync();
            
            return Results.Ok("Tokens added in the database");
        });

        //add
        groupToken.MapPost("/add", async (TokenMetaDataDbContext db, TokenMetadata tokenInput) =>
        {
            var existingToken = await db.TokenMetadatas.FindAsync(tokenInput.Subject);
            if (existingToken is not null)
            {
                return Results.Conflict($"A token with subject '{tokenInput.Subject}' already exists.");
            }

            var token = new TokenMetadata
            {
                Subject = tokenInput.Subject,
                Name = tokenInput.Name,
                Description = tokenInput.Description,
                Policy = tokenInput.Policy,
                Ticker = tokenInput.Ticker,
                Url = tokenInput.Url,
                Logo = tokenInput.Logo,
                Decimals = tokenInput.Decimals,
                Data = tokenInput.Data
            };

            await db.TokenMetadatas.AddAsync(token);
            await db.SaveChangesAsync();

            return Results.CreatedAtRoute(GetSpecificToken, new {subject = token.Subject}, token);
        });


        /////////SYNC

        //add
        groupSync.MapPost("/add", async (TokenMetaDataDbContext db, SyncState sync) => 
        {
            var existingSync = await db.TokenMetadatas.FindAsync(sync.Sha);
            if (existingSync is not null)
            {
                return Results.Conflict($"A token with subject '{sync.Sha}' already exists.");
            }

            var syncState = new SyncState
            {
                Sha = sync.Sha,
                Date = sync.Date
            };

            await db.SyncStates.AddAsync(syncState);
            await db.SaveChangesAsync();

            return Results.CreatedAtRoute(GetSpecificState, new {sha = syncState.Sha}, syncState);
        });

        //filtered get sync metadata given params
        groupSync.MapGet("/get", async (TokenMetaDataDbContext db, string? sha, DateTime? date) =>
        {
            var states = await db.SyncStates
                .Where(t =>
                    (string.IsNullOrWhiteSpace(sha) || t.Sha.Contains(sha)) &&
                    (!date.HasValue || t.Date.Date == date.Value.Date)
                )
                .ToListAsync();

            if (states == null || !states.Any())
            {
                return Results.NotFound("No state found.");
            }

            return Results.Ok(states);
        }).WithName(GetSpecificState);

    }
}