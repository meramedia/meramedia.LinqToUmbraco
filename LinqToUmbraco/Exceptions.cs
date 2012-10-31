using System;

namespace meramedia.Linq.Core
{
    /// <summary>
    /// Exception for when the provided class does not meet the expected class
    /// </summary>
    [Serializable]
    public class DocTypeMismatchException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocTypeMismatchException"/> class.
        /// </summary>
        /// <param name="actual">The actual doc type alias.</param>
        /// <param name="expected">The expcected doc type alias.</param>
        public DocTypeMismatchException(string actual, string expected) : this(actual, expected, string.Empty) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DocTypeMismatchException"/> class.
        /// </summary>
        /// <param name="actual">The actual doc type alias.</param>
        /// <param name="expected">The expcected doc type alias.</param>
        /// <param name="message">Additional message information.</param>
        public DocTypeMismatchException(string actual, string expected, string message)
            : base(string.Format("DocTypeAlias provided did not match what was expected (provided: {0}, expected: {1}){2}{3}", actual, expected, Environment.NewLine, message))
        {
            Expected = expected;
            Actual = actual;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DocTypeMismatchException"/> class.
        /// </summary>
        /// <param name="actual">The actual doc type alias.</param>
        /// <param name="expected">The expcected doc type alias.</param>
        /// <param name="message">Additional message information.</param>
        /// <param name="innerException">The inner exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
        public DocTypeMismatchException(string actual, string expected, string message, Exception innerException)
            : base(string.Format("DocTypeAlias provided did not match what was expected (provided: {0}, expected: {1}){2}{3}", actual, expected, Environment.NewLine, message), innerException)
        {
            Expected = expected;
            Actual = actual;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MandatoryFailureException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        protected DocTypeMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        /// <summary>
        /// Gets or sets the expected DocTypeAlias
        /// </summary>
        /// <value>The expected DocTypeAlias.</value>
        public string Expected { get; set; }
        /// <summary>
        /// Gets or sets the actual DocTypeAlias
        /// </summary>
        /// <value>The actual DocTypeAlias.</value>
        public string Actual { get; set; }
    }
    [Serializable]
    public class LinqToUmbracoException : Exception
    {
        public LinqToUmbracoException(string message) : base(message)
        {

        }
    }

}
