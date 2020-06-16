using MixerInteractive.InteractiveError;
using MixerInteractive.Wire;
using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.Methods
{
    public class MethodHandlerManager
    {
        private Dictionary<string, Func<Method,Reply>> _handlers = new Dictionary<string, Func<Method, Reply>>();

        public void AddHandler(string name, Func<Method, Reply> handler)
        {
            _handlers.Add(name, handler);
        }

        public void RemoveHandler(string name)
        {
            _handlers.Remove(name);
        }

        public Reply Handle(Method method)
        {
            if (_handlers.ContainsKey(method.Name))
                return _handlers[method.Name](method);

            if (method.Discard)
                return null;

            throw new UnknownMethodNameException($"Client cannot process {method.Name}.");
        }
    }
}
