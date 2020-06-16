using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.InteractiveError
{
    public static class ErrorUtil
    {
        static Dictionary<int, Type> Errors = new Dictionary<int, Type>();
    
public static Base FromSocketMessage(this Base error)
        {
            if (Errors.ContainsKey(error.Code))
            {
                var err = (Base)Activator.CreateInstance(Errors[error.Code], args: new object[] { error.Message, error.Code });

                return err;
            }

            return new Base(error.Message, error.Code);
        }
    }
}
