using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC_Course
{
    public class GrammarHelper
    {
        public static Dictionary<string, Type> TokenTypeToTypeDictionary = new Dictionary<string, Type>() {
            {"int", typeof(int)},
            {"float", typeof(float)},
            {"string", typeof(string)},
        };
    }
}
