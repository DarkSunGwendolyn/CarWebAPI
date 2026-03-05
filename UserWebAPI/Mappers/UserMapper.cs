using UserWebAPI.DTO;
using UserWebAPI.Models;

namespace UserWebAPI.Mappers
{
    public class UserMapper : IUserMapper
    {
        public UserDTO MapToDTO(User user)
            => new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                HashPassword = user.HashPassword,
                Username = user.Username,
                FName = user.FName,
                LName = user.LName
            };

        public CreateUserDTO MapToCreateDTO(User user)
           => new CreateUserDTO
           {
               Email = user.Email,
               HashPassword = user.HashPassword,
               Username = user.Username,
               FName = user.FName,
               LName = user.LName
           };

        public UpdateUserDTO MapToUpdateDTO(User user)
           => new UpdateUserDTO
           {
               Email = user.Email,
               HashPassword = user.HashPassword,
               Username = user.Username,
               FName = user.FName,
               LName = user.LName
           };

        public User Map(UserDTO user)
           => new User
           {
               Email = user.Email,
               HashPassword = user.HashPassword,
               Username = user.Username,
               FName = user.FName,
               LName = user.LName
           };

        public User Map(CreateUserDTO user)
           => new User
           {
               Email = user.Email,
               HashPassword = user.HashPassword,
               Username = user.Username,
               FName = user.FName,
               LName = user.LName
           };

        public User Map(UpdateUserDTO user)
           => new User
           {
               Email = user.Email,
               HashPassword = user.HashPassword,
               Username = user.Username,
               FName = user.FName,
               LName = user.LName
           };
    }
}
