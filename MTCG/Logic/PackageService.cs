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
        private CardService CardService = new CardService();

        public Package CreatePackage()
        {
            Package package = new Package();

            return package;
        }
    }
}
