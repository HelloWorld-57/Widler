using UsersService.Application.Commands;
using UsersService.Application.Exceptions;
using UsersService.Application.Interfaces;
using UsersService.Application.Validation;
using UsersService.Domain.Entities;
using UsersService.DTOs.Response;
using UsersService.Infrastructure.Telemetry;

namespace UsersService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IValidatorRunner _validator;

        public UserService(IUserRepository repo, IValidatorRunner validator)
        {
            _repo = repo;
            _validator = validator;
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

        public async Task<string> CreateAsync(CreateUserCommand cmd)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.Create");

            await _validator.ValidateAsync(cmd);

            var user = new User(
                cmd.Name,
                cmd.SecondName,
                cmd.Email,
                cmd.BirthDate
            );

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();

            return user.Id;
        }

        public async Task UpdateAsync(UpdateUserCommand cmd)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.Update");
            activity?.SetTag("user.id", cmd.UserId);

            await _validator.ValidateAsync(cmd);

            var user = await _repo.GetByIdAsync(cmd.UserId)
                ?? throw new NotFoundException(nameof(User), cmd.UserId);

            user.Update(cmd.Name, cmd.SecondName, cmd.Email, cmd.BirthDate);

            await _repo.SaveChangesAsync();
        }

        public async Task ReplaceAsync(ReplaceUserCommand cmd)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.Replace");
            activity?.SetTag("user.id", cmd.UserId);

            await _validator.ValidateAsync(cmd);

            var user = await _repo.GetByIdAsync(cmd.UserId) 
                ?? throw new NotFoundException(nameof(User), cmd.UserId);

            user.Replace(cmd.Name, cmd.SecondName, cmd.Email, cmd.BirthDate);

            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            using var activity = Tracing.ActivitySource.StartActivity("UsersService.Delete");
            activity?.SetTag("user.id", id);

            var user = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(User), id);

            user.Delete();

            await _repo.SaveChangesAsync();
        }

        private static UserResponse MapToResponse(User user)
            => new(
                user.Id,
                user.Name,
                user.SecondName,
                user.Email,
                user.BirthDate,
                user.CreationDate
            );
    }
}
