using CarWebAPI.Models;
using CarWebAPI.DTO;

namespace CarWebAPI.Mappers
{
    public interface ICarMapper :
        IMapper<CarDTO, Car>,
        IMapper<CreateCarDTO, Car>,
        IMapper<UpdateCarDTO, Car>
    {
        CarDTO MapToDTO(Car car);
        CreateCarDTO MapToCreateDTO(Car car);
        UpdateCarDTO MapToUpdateDTO(Car car);
    }
}
