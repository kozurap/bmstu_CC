﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC_Course.Variables
{
    public class VarGen
    {
        #region Fields
        private ValueIncrementation _incrementor;
        #endregion

        #region Properties
        public string Prefix { get; protected set; }
        #endregion

        #region Constructors
        public VarGen(string prefix)
        {
            _incrementor = new ValueIncrementation();
            Prefix = prefix;
        }
        #endregion

        #region Methods

        public string GetNext()
        {
            int nextValue = _incrementor.GetNext();
            string result = string.Format("{0}{1}", Prefix, nextValue);
            return result;
        }

        public void Reset()
        {
            _incrementor.Reset();
        }

        #endregion
    }
}
