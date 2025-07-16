using backend.DTOS.User;
using System;

namespace backend.DTOS.Auth
{
	public class AuthResponseDto
	{
		public UserDto User { get; set; } // The user details
		public string Token { get; set; } // The JWT token
		public string Message { get; set; } 
	}
}