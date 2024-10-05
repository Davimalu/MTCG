using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;

namespace MTCG.Logic
{
    public class StackService
    {
        private PackageService packageService = new PackageService();

        public void AddCardToStack(Card card, Stack stack)
        {
            stack.Cards.Add(card);
        }


        public void AddPackageToStack(Package package, Stack stack)
        {
            foreach (Card card in package.Cards)
            {
                stack.Cards.Add(card);
            }
        }

        public bool RemoveCardFromStack(Card card, Stack stack)
        {
            if (stack.Cards.Contains(card))
            {
                stack.Cards.Remove(card);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
