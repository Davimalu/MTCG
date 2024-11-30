using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;

namespace MTCG.Logic
{
    public class PackageService
    {
        #region Singleton
        private static PackageService? _instance;

        public static PackageService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PackageService();
                }

                return _instance;
            }
        }
        #endregion

        public bool AddCardToPackage(Card card, Package package)
        {
            // Check if package is already full
            if (package.Cards.Count() >= 5)
            {
                return false;
            }

            // Add card to package
            package.Cards.Add(card);
            return true;
        }
    }
}
