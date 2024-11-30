using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;

namespace MTCG.Logic
{
    public class UserService
    {
        private PackageService packageService = new PackageService();
        private StackService stackService = new StackService();

        public bool BuyPackageForUser(User user)
        {
            if (user.CoinCount >= 5)
            {
                user.CoinCount -= 5;
  
                Package package = packageService.CreatePackage();
                stackService.AddPackageToStack(package, user.Stack);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void printStackOfUser(User user)
        {
            foreach (Card card in user.Stack.Cards)
            {
                card.PrintCard();
            }
        }
    }
}
