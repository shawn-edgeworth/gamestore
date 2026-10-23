using GameStore.Api.Models;

namespace GameStore.Api.Mappers
{
    public static class UserMapper
    {
        public static UserDetailsResponse ToDetailsResponse(this User user)
        {
            return new UserDetailsResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
            };
        }
    }
}