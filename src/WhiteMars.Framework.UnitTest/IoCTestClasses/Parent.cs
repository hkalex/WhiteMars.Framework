using System;

namespace WhiteMars.Framework.UnitTest
{
    public class Parent : IParent
    {
        public Parent(IChild child)
        {
            this.Child = child;
        }

        #region IParent implementation

        public IChild Child { get; private set; }

        #endregion
    }
}

