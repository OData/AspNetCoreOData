using System;

namespace QueryBuilder.Edm
{
    /// <summary>
    /// The Edm model name annotation.
    /// </summary>
    public class ModelNameAnnotation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelNameAnnotation"/> class.
        /// </summary>
        /// <param name="name">The model name.</param>
        public ModelNameAnnotation(string name)
        {
            ModelName = name ?? throw Error.ArgumentNull(nameof(name));
        }

        /// <summary>
        /// Gets the backing CLR type for the EDM type.
        /// </summary>
        public string ModelName { get; }
    }
}
