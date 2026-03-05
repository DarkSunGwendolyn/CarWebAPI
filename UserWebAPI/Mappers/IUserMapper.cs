using UserWebAPI.DTO;
using UserWebAPI.Models;

namespace UserWebAPI.Mappers
{
    public interface IUserMapper :
        IMapper<UserDTO, User>,
        IMapper<CreateUserDTO, User>,
        IMapper<UpdateUserDTO, User>
    {
        UserDTO MapToDTO(User user);
        CreateUserDTO MapToCreateDTO(User user);
        UpdateUserDTO MapToUpdateDTO(User user);
    }
}
