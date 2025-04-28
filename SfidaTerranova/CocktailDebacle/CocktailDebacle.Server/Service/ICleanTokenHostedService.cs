using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


// questo è un servizio che si occupa di invalidare i token scaduti e rimuoverli dal database
namespace CocktailDebacle.Server.Service
{
    public interface ICleanTokenHostedService : IHostedService
    {
        Task TrackToken(User user, DateTime expiry);
    }

    public class CleanTokenHostedService : ICleanTokenHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly PriorityQueue<User, DateTime> _expirationQueue = new();
        private readonly ILogger<CleanTokenHostedService> _logger;
        private Timer? _timer;

        public CleanTokenHostedService(IServiceProvider services, ILogger<CleanTokenHostedService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Inizializzazione all'avvio
            _logger.LogInformation("CleanTokenHostedService starting...");
            await InitializeExpiredTokensAsync();
            _timer = new Timer(CheckExpiredTokens, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            _logger.LogInformation("CleanTokenHostedService started successfully");
        }

        private async Task InitializeExpiredTokensAsync()
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            //Pulisci eventuali token già scaduti all'avvio 
            var expiredUsers = await dbContext.DbUser
                .Where(u => u.TokenExpiration != null && u.TokenExpiration <= DateTime.UtcNow)
                .ToListAsync();
            
            // elimina tutti i totken (da utilizzare per testing)
            // var expiredUsers = await dbContext.DbUser
            //     .Where(u => u.TokenExpiration != null)
            //     .ToListAsync();

            // Console.WriteLine($"UTC now: {DateTime.UtcNow}");
            // Console.WriteLine($"Local now: {DateTime.Now}");
            if (expiredUsers.Count == 0)
            {
                _logger.LogInformation("Non ci sono Token scaduti all'avvio");
                return;
            }
            foreach (var user in expiredUsers)
            {
                // _logger.LogWarning($"Scaduto: {user.UserName} - TokenExpiration: {user.TokenExpiration}, Now: {DateTime.UtcNow}");
                user.Token = string.Empty;
                user.TokenExpiration = null;
            }
            await dbContext.SaveChangesAsync();
            _logger.LogInformation($"Cleaned {expiredUsers.Count} expired tokens at startup");

            // Carica in coda i token ancora validi
            var validUsers = await dbContext.DbUser
                .Where(u => u.TokenExpiration != null && u.TokenExpiration > DateTime.UtcNow)
                .ToListAsync();
            
            foreach (var user in validUsers)
            {
                _expirationQueue.Enqueue(user, user.TokenExpiration!.Value);
            }
            _logger.LogInformation($"Loaded {validUsers.Count} active tokens into tracking queue");
        }

        private async void CheckExpiredTokens(object? state)
        {
            _logger.LogDebug("Checking for expired tokens...");
            
            var now = DateTime.UtcNow;
            try
            {
                while (_expirationQueue.TryPeek(out var user, out var expiry) && expiry <= now)
                {
                    _expirationQueue.Dequeue();
                    _logger.LogInformation($"Token expired for user {user.UserName} (expired at {expiry})");
                    await InvalidateTokenAsync(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token invalidation");
            }
        }

        public Task TrackToken(User user, DateTime expiry)
        {
            _expirationQueue.Enqueue(user, expiry);
            _logger.LogDebug($"Tracking token for {user.UserName}, expires at {expiry}");
            return Task.CompletedTask;
        }

        private async Task InvalidateTokenAsync(User user)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var dbUser = await dbContext.DbUser.FindAsync(user.Id);
            if (dbUser != null && dbUser.TokenExpiration == user.TokenExpiration)
            {
                dbUser.Token = string.Empty;
                dbUser.TokenExpiration = null;
                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Automatically invalidated token for {user.UserName}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}