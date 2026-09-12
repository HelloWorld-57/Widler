using UsersService.Application.Commands;
using UsersService.Application.Exceptions;
using UsersService.Application.Interfaces;
using UsersService.Application.Validation;
using UsersService.Domain.Entities;
using UsersService.DTOs.Response;
using UsersService.Infrastructure.Telemetry;
using static UsersService.Infrastructure.Messaging.Kafka.Events.IntegrationEventNames;

namespace UsersService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IValidatorRunner _validator;
        private readonly ICurrentUser _currentUser;
        private readonly IUserAuthorization _authorization;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository repo, 
            IValidatorRunner validator, 
            ICurrentUser currentUser, 
            IUserAuthorization authorization, 
            ILogger<UserService> logger)
        {
            _repo = repo;
            _validator = validator;
            _currentUser = currentUser;
            _authorization = authorization;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync()
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.GetAll");
            
            var users = await _repo.GetAllAsync();

            return users.Select(MapToResponse).ToList();
        }

        public async Task<UserResponse> GetByIdAsync(string id)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.GetById");
            activity?.SetTag("user.id", id);

            var user = await _repo.GetByIdAsync(id);

            if (user == null)
            {
                throw new NotFoundException(nameof(user), id);
            }

            return MapToResponse(user);
        }

        //public async Task<string> CreateAsync(CreateUserCommand cmd)
        //{
        //    using var activity = Tracing.ActivitySource.StartActivity("UsersService.Create");

        //    await _validator.ValidateAsync(cmd);

        //    var user = new User(
        //        cmd.Id,
        //        cmd.Username,
        //        cmd.Email
        //    );

        //    await _repo.AddAsync(user);
        //    await _repo.SaveChangesAsync();

        //    return user.Id;
        //}

        public async Task UpdateAsync(UpdateUserCommand cmd)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.Update");
            activity?.SetTag("user.id", cmd.UserId);

            await _validator.ValidateAsync(cmd);

            var user = await _repo.GetByIdAsync(cmd.UserId)
                ?? throw new NotFoundException(nameof(User), cmd.UserId);

            if (!_authorization.CanUpdate(user))
            {
                throw new ForbiddenException();
            }

            user.Update(cmd.Name, cmd.SecondName, cmd.BirthDate);

            await _repo.SaveChangesAsync();
        }

        public async Task ReplaceAsync(ReplaceUserCommand cmd)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.Replace");
            activity?.SetTag("user.id", cmd.UserId);

            await _validator.ValidateAsync(cmd);

            var user = await _repo.GetByIdAsync(cmd.UserId) 
                ?? throw new NotFoundException(nameof(User), cmd.UserId);

            if (!_authorization.CanUpdate(user))
            {
                throw new ForbiddenException();
            }

            user.Replace(cmd.Name, cmd.SecondName, cmd.BirthDate);

            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.Delete");
            activity?.SetTag("user.id", id);

            var user = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(User), id);

            if (!_authorization.CanDelete(user))
            {
                throw new ForbiddenException();
            }

            user.Delete();

            await _repo.SaveChangesAsync();
        }

        public async Task CreateFromKeycloakAsync(CreateUserFromKeycloakCommand cmd, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.CreateFromKeycloak");

            activity?.SetTag("user.id", cmd.UserId);
            activity?.SetTag("identity.provider", "keycloak");

            var existingUser = await _repo.GetByIdAsync(cmd.UserId);

            if (existingUser is not null)
            {
                _logger.LogInformation("User already exists. UserId={UserId}", cmd.UserId);

                return;
            }

            var user = new User(
                cmd.UserId,
                cmd.Username,
                cmd.Email);

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateEmailFromKeycloakAsync(UpdateUserEmailFromKeycloakCommand cmd, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.UpdateEmailFromKeycloak");

            activity?.SetTag("user.id", cmd.UserId);
            activity?.SetTag("identity.provider", "keycloak");

            var user = await _repo.GetByIdAsync(cmd.UserId);

            if (user is null)
            {
                throw new NotFoundException(nameof(User), cmd.UserId);
            }

            if (user.Email == cmd.Email)
            {
                _logger.LogDebug("User email is already synchronized. UserId={UserId}", cmd.UserId);

                return;
            }

            user.UpdateEmail(cmd.Email);

            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("User email synchronized from Keycloak. UserId={UserId}", cmd.UserId);
        }

        public async Task EnableFromKeycloakAsync(string userId, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.EnableFromKeycloak");

            activity?.SetTag("user.id", userId);
            activity?.SetTag("identity.provider", "keycloak");

            var user = await _repo.GetByIdAsync(userId);

            if (user is null)
            {
                _logger.LogWarning("User received ENABLE event from Keycloak, but user does not exist in UsersService. UserId={UserId}", userId);

                return;
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Ignoring ENABLE event for deleted user. UserId={UserId}", userId);

                return;
            }

            if (user.IsEnabled)
            {
                _logger.LogDebug("User is already enabled. UserId={UserId}", userId);

                return;
            }

            user.Enable();

            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("User enabled because user was enabled in Keycloak. UserId={UserId}", userId);
        }

        public async Task DisableFromKeycloakAsync(string userId, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.DisableFromKeycloak");

            activity?.SetTag("user.id", userId);
            activity?.SetTag("identity.provider", "keycloak");

            var user = await _repo.GetByIdAsync(userId);

            if (user is null)
            {
                _logger.LogWarning("User received DISABLE event from Keycloak, but user does not exist in UsersService. UserId={UserId}", userId);

                return;
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Ignoring DISABLE event for deleted user. UserId={UserId}", userId);

                return;
            }

            if (!user.IsEnabled)
            {
                _logger.LogDebug("User is already disabled. UserId={UserId}", userId);

                return;
            }

            user.Disable();

            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("User disabled because user was disabled in Keycloak. UserId={UserId}", userId);
        }

        public async Task DeleteFromKeycloakAsync(string userId, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.DeleteFromKeycloak");

            activity?.SetTag("user.id", userId);
            activity?.SetTag("identity.provider", "keycloak");

            var user = await _repo.GetByIdAsync(userId);

            if (user is null)
            {
                _logger.LogWarning("User received DELETE event from Keycloak, but user does not exist in UsersService. UserId={UserId}", userId);

                return;
            }

            if (user.IsDeleted)
            {
                _logger.LogDebug("User is already deleted. UserId={UserId}", userId);

                return;
            }

            user.DeleteFromKeycloak();

            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("User soft deleted because user was deleted in Keycloak. UserId={UserId}", userId);
        }

        private static UserResponse MapToResponse(User user)
            => new(
                user.Id,
                user.Name ?? "",
                user.SecondName ?? "",
                user.Email,
                user.BirthDate ?? DateTime.UtcNow,
                user.CreationDate,
                user.IsEnabled
            );
    }
}
