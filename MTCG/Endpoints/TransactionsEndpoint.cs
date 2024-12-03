using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Endpoints
{
    public class TransactionsEndpoint : IHttpEndpoint
    {
        private readonly UserService _userService = UserService.Instance;
        private readonly PackageService _packageService = PackageService.Instance;
        private readonly StackService _stackService = StackService.Instance;

        public (int, string?) HandleRequest(HTTPHeader headers, string? body)
        {
            // Check if user is authorized
            string token = HeaderHelper.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (403, "User not authorized!");
            }

            // Buy new package
            if (headers is { Method: "POST", Path: "/transactions/packages" })
            {
                // Check if user has enough coins
                if (user.CoinCount < 5)
                {
                    return (402, "Not enough money");
                }

                Package? package = _packageService.GetRandomPackage();

                if (package == null)
                {
                    return (410, "No packages available");
                }

                // Add cards to user's stack
                _stackService.AddPackageToStack(package, user.Stack);

                // Save changes into database
                user.CoinCount -= 5;
                _userService.SaveUserToDatabase(user);

                return (201, "Package retrieved successfully and added cards to user's stack!");
            }

            return (405, "Invalid request");
        }
    }
}
