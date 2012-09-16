using System;

namespace meramedia.Linq.Core
{
    /// <summary>
    /// Typed switch implementation from Bart De Smet: http://community.bartdesmet.net/blogs/bart/archive/2008/03/30/a-functional-c-type-switch.aspx
    /// </summary>
    public class Switch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Switch"/> class.
        /// </summary>
        /// <param name="o">The o.</param>
        public Switch(object o)
        {
            Obj = o;
        }

        /// <summary>
        /// Gets or sets the object being operated against
        /// </summary>
        /// <value>The obj.</value>
        public object Obj { get; private set; }
    
        /// <summary>
        /// Case statement
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="a">Action method to execute for case evaluation</param>
        /// <returns></returns>
        public Switch Case<T>(Action<T> a)
        {
            return Case<T>(o => true, a, false);
        }

        /// <summary>
        /// Case statement
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="a">Action method to execute for case evaluation</param>
        /// <param name="fallThrough">if set to <c>true</c> fall through to next case statement.</param>
        /// <returns></returns>
        public Switch Case<T>(Action<T> a, bool fallThrough)
        {
            return Case<T>(o => true, a, fallThrough);
        }

        /// <summary>
        /// Case statement
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="c">The funcation to eveluate against to object.</param>
        /// <param name="a">Action method to execute for case evaluation</param>
        /// <returns></returns>
        public Switch Case<T>(Func<T, bool> c, Action<T> a)
        {
            return Case<T>(c, a, false);
        }

        /// <summary>
        /// Case statement
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="c">The funcation to eveluate against to object.</param>
        /// <param name="a">Action method to execute for case evaluation</param>
        /// <param name="fallThrough">if set to <c>true</c> fall through to next case statement.</param>
        /// <returns></returns>
        public Switch Case<T>(Func<T, bool> c, Action<T> a, bool fallThrough)
        {
            if (Obj is T)
            {
                T t = (T) Obj;
                if (c(t))
                {
                    a(t);
                    return fallThrough ? this : null;
                }
            }

            return this;
        }

        /// <summary>
        /// Defaults case
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="a">Action to perform</param>
        /// <returns></returns>
        public Switch Default<T>(Action<T> a)
        {
            a((T) Obj);
            return this;
        }
    }
}
