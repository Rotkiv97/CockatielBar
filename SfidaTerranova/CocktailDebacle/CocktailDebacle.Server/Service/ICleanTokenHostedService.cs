using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


// questo è un servizio che si occupa di invalidare i token scaduti e rimuoverli dal database
namespace CocktailDebacle.Server.Service
{
    public class ICleanTokenHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly PriorityQueue<User, DateTime> _expirationQueue = new();
        private readonly ILogger<ICleanTokenHostedService> _logger;
        private Timer? _timer;

        public ICleanTokenHostedService(IServiceProvider services, ILogger<ICleanTokenHostedService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var validUsers = dbContext.DbUser
                .Where(u => u.TokenExpiration != null && u.TokenExpiration > DateTime.UtcNow)
                .ToList();
            foreach (var user in validUsers)
            {
                if (user.TokenExpiration == null) continue;
                _expirationQueue.Enqueue(user, user.TokenExpiration.Value);
            }
            _timer = new Timer(CheckExpiredTokens, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            return Task.CompletedTask;
        }

        private async void CheckExpiredTokens(object? state)
        {
            var now = DateTime.UtcNow;
            try{
                while (_expirationQueue.TryPeek(out var user, out var expiry) && expiry <= now)
                {
                    _expirationQueue.Dequeue();
                    await InvalidateToken(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking expired tokens");
            }
        }

        public Task TrackToken(User user, DateTime expiry)
        {
            _expirationQueue.Enqueue(user, expiry);
            _logger.LogInformation($"Tracking token for {user.UserName}, expires at {expiry}");
            return Task.CompletedTask;
        }

        private async Task InvalidateToken(User user)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var dbUser = await dbContext.DbUser.FindAsync(user.Id);
            if (dbUser != null && dbUser.TokenExpiration == user.TokenExpiration)
            {
                dbUser.Token = string.Empty;
                dbUser.TokenExpiration = null;
                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Invalidated token for {user.UserName} at {DateTime.UtcNow}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose(){
            _timer?.Dispose();
        } 
    }
}