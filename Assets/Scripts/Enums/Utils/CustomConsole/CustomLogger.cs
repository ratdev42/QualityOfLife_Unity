using System.Diagnostics;
using UnityEngine;

namespace RD42.Utils
{
    public class CustomLogger
    {
        private readonly string tag;
        private readonly string color;
        private readonly Object owner;

        //Ctor 
        public CustomLogger(Object owner, string tag, string color)
        {
            this.owner = owner;
            this.tag = tag;
            this.color = color;
        }

        [Conditional("DEBUG")]
        public void Info(object message)
        {
            Rd42Log.Info(owner, tag, message, color);
        }

        [Conditional("DEBUG")]
        public void Warning(object message)
        {
            Rd42Log.Warning(owner, tag, message);
        }

        [Conditional("DEBUG")]
        public void Error(object message)
        {
            Rd42Log.Error(owner, tag, message);
        }
    }
}