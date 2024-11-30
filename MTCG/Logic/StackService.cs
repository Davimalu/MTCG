using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Logic
{
    public class StackService
    {
        #region Singleton
        private static StackService? _instance;

        public static StackService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StackService();
                }

                return _instance;
            }
        }
        #endregion

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
