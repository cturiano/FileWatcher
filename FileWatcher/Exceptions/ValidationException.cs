using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FileWatcher.Exceptions
{
    [Serializable]
    internal class ValidationException : Exception
    {
        #region Constructors

        public ValidationException()
        {
        }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception inner) : base(message, inner)
        {
        }

        public ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ResourceReferenceProperty = info.GetString("ResourceReferenceProperty");
        }

        #endregion

        #region Properties

        public string ResourceReferenceProperty { get; set; }

        #endregion

        #region Public Methods

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("ResourceReferenceProperty", ResourceReferenceProperty);
            base.GetObjectData(info, context);
        }

        #endregion
    }
}